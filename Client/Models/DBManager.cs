using Client.Models;
using Client.ViewModels;
using Client.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;

/// <summary>
/// 프로젝트의 데이터베이스 관련 작업을 관리하는 클래스입니다.
/// 모든 데이터를 단일 SQLite 파일에 저장하도록 개선되었습니다.
/// 개별 CRUD 작업을 외부에서 직접 호출할 수 있도록 공개 메서드를 추가했습니다.
/// </summary>
public class DBManager
{
    // 단일 DB 파일명 상수 정의
    private const string PROJECT_DB = "project.db";
    private const string METADATA_FILE = "metadata.json";

    private string _tempDirPath;
    private string _projectFilePath;

    public ProjectMetadata projectMetadata { get; private set; }

    public DBManager()
    {
        // 프로젝트 임시 저장 디렉토리 생성
        _tempDirPath = Path.Combine(Path.GetTempPath(), "NodeFlowTemp", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirPath);
        projectMetadata = new ProjectMetadata();
    }

    /// <summary>
    /// DB 파일 연결 문자열을 반환합니다.
    /// </summary>
    private string GetConnectionString()
    {
        string dbPath = Path.Combine(_tempDirPath, PROJECT_DB);
        return $"Data Source={dbPath};Version=3;";
    }

    /// <summary>
    /// 단일 데이터베이스 파일에 모든 테이블을 생성하고 초기화합니다.
    /// </summary>
    private void CreateAndOpenDatabase()
    {
        string dbPath = Path.Combine(_tempDirPath, PROJECT_DB);
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
        SQLiteConnection.CreateFile(dbPath);
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS INFO_NODES (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    NODE_TITLE VARCHAR(50),
                    POS_X DOUBLE,
                    POS_Y DOUBLE,
                    ID_TYPE INT,
                    DATE_START DATETIME,
                    DATE_END DATETIME,
                    ASSIGNEE VARCHAR(50),
                    PATH VARCHAR(200)
                );
                CREATE TABLE IF NOT EXISTS INFO_LINKS (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ID_NODE_SRC INTEGER,
                    ID_NODE_TGT INTEGER,
                    CREATED_AT DATETIME,
                    FOREIGN KEY(ID_NODE_SRC) REFERENCES INFO_NODES(ID) ON DELETE CASCADE,
                    FOREIGN KEY(ID_NODE_TGT) REFERENCES INFO_NODES(ID) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS INFO_PROPERTIES (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    TYPE VARCHAR(10) NOT NULL,
                    NAME VARCHAR(20),
                    VALUE TEXT
                );
                CREATE TABLE IF NOT EXISTS INFO_TYPES (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    TYPE VARCHAR(10),
                    COLOR_R INTEGER,
                    COLOR_G INTEGER,
                    COLOR_B INTEGER
                );";

            using (var command = new SQLiteCommand(createTablesSql, conn))
            {
                command.ExecuteNonQuery();
            }
            conn.Close(); // 명시적으로 연결을 닫습니다.
        }
    }

    public void CreateNewProject()
    {
        CleanupTempFiles();
        Directory.CreateDirectory(_tempDirPath);
        CreateAndOpenDatabase();
        projectMetadata = new ProjectMetadata();
        _projectFilePath = null;
    }

    public void LoadProject(string filePath)
    {
        _projectFilePath = filePath;
        CleanupTempFiles();
        Directory.CreateDirectory(_tempDirPath);

        try
        {
            if (File.Exists(filePath))
            {
                ZipFile.ExtractToDirectory(filePath, _tempDirPath, true);
            }
            else
            {
                throw new FileNotFoundException($"프로젝트 파일이 존재하지 않습니다: {filePath}");
            }

            string metadataPath = Path.Combine(_tempDirPath, METADATA_FILE);
            if (File.Exists(metadataPath))
            {
                string json = File.ReadAllText(metadataPath, Encoding.UTF8);
                projectMetadata = JsonSerializer.Deserialize<ProjectMetadata>(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"프로젝트 로드 중 오류 발생: {ex.Message}");
            CleanupTempFiles();
            projectMetadata = new ProjectMetadata();
        }
    }

    /// <summary>
    /// 메모리의 모든 컬렉션 데이터를 DB 파일에 저장하고 압축합니다.
    /// 단일 트랜잭션과 연결을 사용하여 데이터 충돌을 방지합니다.
    /// </summary>
    public void SaveProject(string filePath, ObservableCollection<NodeViewModel> nodes, ObservableCollection<LinkViewModel> links, ObservableCollection<PropertyItem> properties)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 모든 기존 데이터 삭제
                    DeleteAllData(conn);

                    // 새로운 노드 데이터 저장
                    string insertNodeSql = @"
                        INSERT INTO INFO_NODES (NODE_TITLE, ID_TYPE, DATE_START, DATE_END, ASSIGNEE, POS_X, POS_Y, PATH)
                        VALUES (@nodeTitle, @idType, @dateStart, @dateEnd, @assignee, @posx, @posy, @path);";
                    using (var cmd = new SQLiteCommand(insertNodeSql, conn))
                    {
                        foreach (var nodeVm in nodes)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@nodeTitle", nodeVm.NodeData.NODE_TITLE);
                            cmd.Parameters.AddWithValue("@idType", nodeVm.NodeData.ID_TYPE);
                            cmd.Parameters.AddWithValue("@dateStart", nodeVm.NodeData.DATE_START?.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@dateEnd", nodeVm.NodeData.DATE_END?.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@assignee", nodeVm.NodeData.Assignee);
                            cmd.Parameters.AddWithValue("@posx", nodeVm.NodeData.XPosition);
                            cmd.Parameters.AddWithValue("@posy", nodeVm.NodeData.YPosition);
                            cmd.Parameters.AddWithValue("@path", nodeVm.NodeData.PathCustom);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 새로운 링크 데이터 저장
                    string insertLinkSql = @"
                        INSERT INTO INFO_LINKS (ID_NODE_SRC, ID_NODE_TGT, CREATED_AT)
                        VALUES (@idSrc, @idTgt, @createdAt);";
                    using (var cmd = new SQLiteCommand(insertLinkSql, conn))
                    {
                        foreach (var linkVm in links)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@idSrc", linkVm.LinkData.ID_NODE_SRC);
                            cmd.Parameters.AddWithValue("@idTgt", linkVm.LinkData.ID_NODE_TGT);
                            cmd.Parameters.AddWithValue("@createdAt", linkVm.LinkData.CREATED_AT?.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 새로운 속성 데이터 저장
                    string insertPropertySql = "INSERT INTO INFO_PROPERTIES (TYPE, NAME, VALUE) VALUES (@type, @name, @value);";
                    using (var cmd = new SQLiteCommand(insertPropertySql, conn))
                    {
                        foreach (var propertyItem in properties)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@type", propertyItem.Type);
                            cmd.Parameters.AddWithValue("@name", propertyItem.Name);
                            cmd.Parameters.AddWithValue("@value", propertyItem.Value?.ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"프로젝트 저장 실패: {ex.Message}");
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close(); // 트랜잭션 종료 후 명시적으로 연결을 닫습니다.
                }
            }
        }

        try
        {
            // 메타데이터 업데이트 및 저장
            _projectFilePath = filePath;
            projectMetadata.LastModifiedDate = DateTime.Now;
            projectMetadata.ProjectName = Path.GetFileNameWithoutExtension(filePath);
            string metadataPath = Path.Combine(_tempDirPath, METADATA_FILE);
            string json = JsonSerializer.Serialize(projectMetadata);
            File.WriteAllText(metadataPath, json);

            // 임시 폴더의 모든 내용을 압축
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            ZipFile.CreateFromDirectory(_tempDirPath, filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"프로젝트 압축 또는 파일 쓰기 실패: {ex.Message}");
            throw;
        }
    }

    public void CleanupTempFiles()
    {
        try
        {
            if (Directory.Exists(_tempDirPath))
            {
                Directory.Delete(_tempDirPath, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"임시 파일 정리 실패: {ex.Message}");
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------
    // **공개 CRUD 메서드**

    /// <summary>
    /// 새로운 노드를 INFO_NODES 테이블에 추가합니다.
    /// </summary>
    public int AddNode(NodeModel node)
    {
        int newId = 0;
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string insertSql = @"
                INSERT INTO INFO_NODES (NODE_TITLE, ID_TYPE, DATE_START, DATE_END, ASSIGNEE, POS_X, POS_Y)
                VALUES (@nodeTitle, @idType, @dateStart, @dateEnd, @assignee, @posx, @posy);
                SELECT last_insert_rowid();";
            using (var cmd = new SQLiteCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@nodeTitle", node.NODE_TITLE);
                cmd.Parameters.AddWithValue("@idType", node.ID_TYPE);
                cmd.Parameters.AddWithValue("@dateStart", node.DATE_START?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@dateEnd", node.DATE_END?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@assignee", node.Assignee);
                cmd.Parameters.AddWithValue("@posx", node.XPosition);
                cmd.Parameters.AddWithValue("@posy", node.YPosition);
                newId = (int)(long)cmd.ExecuteScalar();
                Console.WriteLine($"생성된 노드번호 : {newId}");
            }
            conn.Close();
        }
        return newId;
    }

    /// <summary>
    /// 기존 노드 데이터를 INFO_NODES 테이블에서 수정합니다.
    /// </summary>
    public void UpdateNode(NodeModel node)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string updateSql = @"
                UPDATE INFO_NODES
                SET NODE_TITLE = @nodeTitle,
                    ID_TYPE = @idType,
                    DATE_START = @dateStart,
                    DATE_END = @dateEnd,
                    ASSIGNEE = @assignee,
                    POS_X = @posx,
                    POS_Y = @posy
                WHERE ID = @id;";
            using (var cmd = new SQLiteCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@nodeTitle", node.NODE_TITLE);
                cmd.Parameters.AddWithValue("@idType", node.ID_TYPE);
                cmd.Parameters.AddWithValue("@dateStart", node.DATE_START?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@dateEnd", node.DATE_END?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@assignee", node.Assignee);
                cmd.Parameters.AddWithValue("@posx", node.XPosition);
                cmd.Parameters.AddWithValue("@posy", node.YPosition);
                cmd.Parameters.AddWithValue("@id", node.ID_NODE);
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }

    /// <summary>
    /// 지정된 ID의 노드를 INFO_NODES 테이블에서 삭제합니다.
    /// </summary>
    public void DeleteNode(int nodeId)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_NODES WHERE ID = @id;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.Parameters.AddWithValue("@id", nodeId);
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }

    /// <summary>
    /// 새로운 링크를 INFO_LINKS 테이블에 추가합니다.
    /// </summary>
    public void AddLink(LinkModel link)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string insertSql = @"
                INSERT INTO INFO_LINKS (ID_NODE_SRC, ID_NODE_TGT, CREATED_AT)
                VALUES (@idSrc, @idTgt, @createdAt);";
            using (var cmd = new SQLiteCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@idSrc", link.ID_NODE_SRC);
                cmd.Parameters.AddWithValue("@idTgt", link.ID_NODE_TGT);
                cmd.Parameters.AddWithValue("@createdAt", link.CREATED_AT?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }

    /// <summary>
    /// 지정된 ID의 링크를 INFO_LINKS 테이블에서 삭제합니다.
    /// </summary>
    public void DeleteLink(int nodeId)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();

            // 1. 삭제 전 행(row) 수 확인 및 출력
            string countSqlBefore = "SELECT COUNT(*) FROM INFO_LINKS WHERE ID_NODE_SRC = @nodeId OR ID_NODE_TGT = @nodeId;";
            using (var cmdCountBefore = new SQLiteCommand(countSqlBefore, conn))
            {
                cmdCountBefore.Parameters.AddWithValue("@nodeId", nodeId);
                long countBefore = (long)cmdCountBefore.ExecuteScalar();
                Console.WriteLine($"[DBManager] 삭제 전, 연결된 링크 수: {countBefore}개");
            }

            // 2. 링크 삭제
            string deleteSql = "DELETE FROM INFO_LINKS WHERE ID_NODE_SRC = @nodeId OR ID_NODE_TGT = @nodeId;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.Parameters.AddWithValue("@nodeId", nodeId);
                int rowsAffected = cmd.ExecuteNonQuery(); // 삭제된 행의 수를 반환합니다.
                Console.WriteLine($"[DBManager] 총 {rowsAffected}개의 링크가 삭제되었습니다.");
            }

            // 3. 삭제 후 전체 행(row) 수 확인 및 출력
            string countSqlAfter = "SELECT COUNT(*) FROM INFO_LINKS;";
            using (var cmdCountAfter = new SQLiteCommand(countSqlAfter, conn))
            {
                long countAfter = (long)cmdCountAfter.ExecuteScalar();
                Console.WriteLine($"[DBManager] 삭제 후, 총 링크 수: {countAfter}개");
            }

            conn.Close();
        }
    }

    /// <summary>
    /// 새로운 속성 아이템을 추가합니다.
    /// </summary>
    public int AddPropertyItem(PropertyItem item)
    {
        int newId = 0;
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string insertSql = "INSERT INTO INFO_PROPERTIES (TYPE, NAME, VALUE) VALUES (@type, @name, @value); SELECT last_insert_rowid();";
            using (var cmd = new SQLiteCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@type", item.Type);
                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@value", item.Value?.ToString());
                newId = (int)(long)cmd.ExecuteScalar();
            }
            conn.Close();
        }
        return newId;
    }

    /// <summary>
    /// 기존 속성 아이템을 수정합니다.
    /// </summary>
    public void UpdatePropertyItem(PropertyItem item)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string updateSql = "UPDATE INFO_PROPERTIES SET TYPE = @type, NAME = @name, VALUE = @value WHERE ID = @id;";
            using (var cmd = new SQLiteCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@type", item.Type);
                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@value", item.Value?.ToString());
                cmd.Parameters.AddWithValue("@id", item.ID);
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }

    /// <summary>
    /// 지정된 ID의 속성 아이템을 삭제합니다.
    /// </summary>
    public void DeletePropertyItem(int propertyId)
    {
        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_PROPERTIES WHERE ID = @ID;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", propertyId);
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------
    // **모든 데이터 로드 메서드**

    /// <summary>
    /// INFO_NODES 테이블의 모든 노드 데이터를 불러옵니다.
    /// </summary>
    public List<NodeModel> GetAllNodes()
    {
        List<NodeModel> nodes = new List<NodeModel>();
        string dbPath = Path.Combine(_tempDirPath, PROJECT_DB);
        if (!File.Exists(dbPath)) return nodes;

        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string selectSql = "SELECT ID, NODE_TITLE, ID_TYPE, DATE_START, DATE_END, ASSIGNEE, POS_X, POS_Y, PATH FROM INFO_NODES;";
            using (var cmd = new SQLiteCommand(selectSql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var node = new NodeModel
                        {
                            ID_NODE = reader.GetInt32(0),
                            NODE_TITLE = reader.IsDBNull(1) ? null : reader.GetString(1),
                            ID_TYPE = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            DATE_START = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            DATE_END = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            Assignee = reader.IsDBNull(5) ? null : reader.GetString(5),
                            XPosition = reader.GetDouble(6),
                            YPosition = reader.GetDouble(7)
                        };
                        nodes.Add(node);
                    }
                }
            }
            conn.Close();
        }
        return nodes;
    }

    /// <summary>
    /// INFO_LINKS 테이블의 모든 링크 데이터를 불러옵니다.
    /// </summary>
    public List<LinkModel> GetAllLinks()
    {
        List<LinkModel> links = new List<LinkModel>();
        string dbPath = Path.Combine(_tempDirPath, PROJECT_DB);
        if (!File.Exists(dbPath)) return links;

        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string selectSql = "SELECT ID, ID_NODE_SRC, ID_NODE_TGT, CREATED_AT FROM INFO_LINKS;";
            using (var cmd = new SQLiteCommand(selectSql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        links.Add(new LinkModel
                        {
                            ID = reader.GetInt32(0),
                            ID_NODE_SRC = reader.GetInt32(1),
                            ID_NODE_TGT = reader.GetInt32(2),
                            CREATED_AT = reader.IsDBNull(3) ? (DateTime?)null : DateTime.ParseExact(reader.GetString(3), "yyyy-MM-dd HH:mm:ss", null)
                        });
                    }
                }
            }
            conn.Close();
        }
        return links;
    }

    /// <summary>
    /// INFO_PROPERTIES 테이블의 모든 속성 데이터를 불러옵니다.
    /// </summary>
    public List<PropertyItem> GetAllProperties()
    {
        List<PropertyItem> properties = new List<PropertyItem>();
        string dbPath = Path.Combine(_tempDirPath, PROJECT_DB);
        if (!File.Exists(dbPath)) return properties;

        using (var conn = new SQLiteConnection(GetConnectionString()))
        {
            conn.Open();
            string sql = "SELECT ID, TYPE, NAME, VALUE FROM INFO_PROPERTIES";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        properties.Add(new PropertyItem
                        {
                            ID = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            Name = reader.GetString(2),
                            Value = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
            conn.Close();
        }
        return properties;
    }

    /// <summary>
    /// INFO_TYPES 테이블의 모든 타입 데이터를 불러옵니다.
    /// </summary>
    //public List<TypeItem> GetAllTypes()
    //{
    //    List<TypeItem> types = new List<TypeItem>();
    //    string dbPath = Path.Combine(_tempDirPath, PROJECT_DB);
    //    if (!File.Exists(dbPath)) return types;

    //    using (var conn = new SQLiteConnection(GetConnectionString()))
    //    {
    //        conn.Open();
    //        string sql = "SELECT ID, TYPE, COLOR_R, COLOR_G, COLOR_B FROM INFO_TYPES";
    //        using (var cmd = new SQLiteCommand(sql, conn))
    //        {
    //            using (var reader = cmd.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    types.Add(new TypeItem
    //                    {
    //                        ID = reader.GetInt32(0),
    //                        Type = reader.GetString(1),
    //                        ColorR = reader.GetInt32(2),
    //                        ColorG = reader.GetInt32(3),
    //                        ColorB = reader.GetInt32(4)
    //                    });
    //                }
    //            }
    //        }
    //        conn.Close();
    //    }
    //    return types;
    //}

    //---------------------------------------------------------------------------------------------------------------------------------
    // **내부 메서드**
    private void DeleteAllData(SQLiteConnection conn)
    {
        string deleteSql = "DELETE FROM INFO_NODES; DELETE FROM INFO_LINKS; DELETE FROM INFO_PROPERTIES; DELETE FROM INFO_TYPES;";
        using (var cmd = new SQLiteCommand(deleteSql, conn))
        {
            cmd.ExecuteNonQuery();
        }

        // AUTO INCREMENT 초기화: DELETE 문과 동일한 트랜잭션 내에서 실행
        string resetSql = @"
            UPDATE sqlite_sequence SET seq = 0 WHERE name = 'INFO_NODES';
            UPDATE sqlite_sequence SET seq = 0 WHERE name = 'INFO_LINKS';
            UPDATE sqlite_sequence SET seq = 0 WHERE name = 'INFO_PROPERTIES';
            UPDATE sqlite_sequence SET seq = 0 WHERE name = 'INFO_TYPES';
        ";
        using (var cmd = new SQLiteCommand(resetSql, conn))
        {
            cmd.ExecuteNonQuery();
        }
    }
}
