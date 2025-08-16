using Client.Models;
using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

public class DBManager
{
    // DB 파일명 상수 정의
    private const string INFO_NODES_DB = "info_nodes.db";
    private const string INFO_LINKS_DB = "info_links.db";
    private const string INFO_PROPERTIES_DB = "info_properties.db";
    private const string INFO_TYPES_DB = "info_types.db";
    private const string METADATA_FILE = "metadata.json";

    private string _tempDirPath;
    private string _projectFilePath;

    public ProjectMetadata projectMetadata { get; private set; }

    public DBManager()
    {
        _tempDirPath = Path.Combine(Path.GetTempPath(), "NodeFlowTemp", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirPath);
        projectMetadata = new ProjectMetadata();
    }

    ~DBManager()
    {
        CleanupTempFiles();
    }

    private string GetConnectionString(string dbName)
    {
        string dbPath = Path.Combine(_tempDirPath, dbName);
        return $"Data Source={dbPath};Version=3;";
    }

    private string GetCreateTableSql(string dbName)
    {
        switch (dbName)
        {
            case INFO_NODES_DB:
                // NodeModel의 필드와 INFO_NODES_DB 테이블 스키마 불일치
                // NodeModel의 NODE_TITLE, DATE_START 등을 위한 스키마가 필요
                // 기존 코드를 기반으로 유지
                return @"CREATE TABLE IF NOT EXISTS INFO_NODES (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            NODE_TITLE VARCHAR(50),
                            POS_X DOUBLE,
                            POS_Y DOUBLE,
                            ID_TYPE INT,
                            DATE_START DATETIME,
                            DATE_END DATETIME,
                            ASSIGNEE VARCHAR(50),
                            PATH VARCHAR(200)
                        );";
            case INFO_LINKS_DB:
                return @"CREATE TABLE IF NOT EXISTS INFO_LINKS (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ID_NODE_SRC INTEGER,
                            ID_NODE_TGT INTEGER,
                            CREATED_AT DATETIME,
                            FOREIGN KEY(ID_NODE_SRC) REFERENCES INFO_NODES(ID) ON DELETE RESTRICT,
                            FOREIGN KEY(ID_NODE_TGT) REFERENCES INFO_NODES(ID) ON DELETE RESTRICT
                        );";
            case INFO_PROPERTIES_DB:
                // **INFO_PROPERTIES 테이블 스키마 수정:** ID, ID_TYPE, NAME, VALUE를 포함하도록 수정
                return @"CREATE TABLE IF NOT EXISTS INFO_PROPERTIES (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ID_TYPE INTEGER,
                            NAME VARCHAR(20),
                            VALUE TEXT,
                            FOREIGN KEY(ID_TYPE) REFERENCES INFO_TYPES(ID) ON DELETE RESTRICT
                        );";
            case INFO_TYPES_DB:
                return @"CREATE TABLE IF NOT EXISTS INFO_TYPES (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            TYPE VARCHAR(10),
                            COLOR_R INTEGER,
                            COLOR_G INTEGER,
                            COLOR_B INTEGER
                        );";
            default:
                return string.Empty;
        }
    }

    private void CreateAndOpenDatabase(string dbName)
    {
        string dbPath = Path.Combine(_tempDirPath, dbName);
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
        SQLiteConnection.CreateFile(dbPath);
        using (var conn = new SQLiteConnection(GetConnectionString(dbName)))
        {
            conn.Open();
            string sql = GetCreateTableSql(dbName);
            if (!string.IsNullOrEmpty(sql))
            {
                using (var command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public void CreateNewProject()
    {
        CleanupTempFiles();
        Directory.CreateDirectory(_tempDirPath);

        CreateAndOpenDatabase(INFO_NODES_DB);
        CreateAndOpenDatabase(INFO_LINKS_DB);
        CreateAndOpenDatabase(INFO_PROPERTIES_DB);
        CreateAndOpenDatabase(INFO_TYPES_DB);
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
            ZipFile.ExtractToDirectory(filePath, _tempDirPath, true);
        }
        catch (IOException)
        {
            // 압축 해제 중 오류가 발생하면, 임시 폴더를 다시 정리하고 재시도
            CleanupTempFiles();
            Directory.CreateDirectory(_tempDirPath);
            ZipFile.ExtractToDirectory(filePath, _tempDirPath, true);
        }

        string metadataPath = Path.Combine(_tempDirPath, METADATA_FILE);
        if (File.Exists(metadataPath))
        {
            string json = File.ReadAllText(metadataPath);
            projectMetadata = JsonSerializer.Deserialize<ProjectMetadata>(json);
        }
        else
        {
            projectMetadata = new ProjectMetadata();
        }
    }

    public void SaveProject(string filePath, ObservableCollection<NodeViewModel> nodes, ObservableCollection<LinkViewModel> links, ObservableCollection<PropertyItem> properties)
    {
        // 1. 메모리의 모든 컬렉션 데이터를 DB 파일에 반영합니다.
        // 기존 데이터 삭제 후 새로운 데이터 삽입 (가장 단순한 방법)
        DeleteAllNodes();
        foreach (var nodeVm in nodes)
        {
            AddNode(nodeVm.NodeData); // NodeViewModel에서 NodeModel 추출 후 저장
        }

        // (Link와 Property도 동일한 방식으로 처리)
        DeleteAllLinks();
        DeleteAllProperties();

        // 2. 메타데이터 업데이트 및 저장합니다.
        _projectFilePath = filePath;
        projectMetadata.LastModifiedDate = DateTime.Now;

        string metadataPath = Path.Combine(_tempDirPath, METADATA_FILE);
        string json = JsonSerializer.Serialize(projectMetadata);
        File.WriteAllText(metadataPath, json);

        // 3. 임시 폴더의 모든 내용을 압축하여 최종 파일을 생성합니다.
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        ZipFile.CreateFromDirectory(_tempDirPath, filePath);
    }

    public void CleanupTempFiles()
    {
        if (Directory.Exists(_tempDirPath))
        {
            Directory.Delete(_tempDirPath, true);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------
    // **INFO_NODES 테이블 CRUD 메서드**

    /// <summary>
    /// 새로운 노드를 INFO_NODES 테이블에 추가합니다.
    /// </summary>
    /// <param name="node">추가할 NodeModel 객체</param>
    /// <returns>DB에서 생성된 ID가 포함된 NodeModel 객체</returns>
    public NodeModel AddNode(NodeModel node)
    {
        string connString = GetConnectionString(INFO_NODES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string insertSql = @"
                INSERT INTO INFO_NODES (NODE_TITLE, ID_TYPE, DATE_START, DATE_END, ASSIGNEE)
                VALUES (@nodeTitle, @idType, @dateStart, @dateEnd, @assignee);
                SELECT last_insert_rowid();";

            using (var cmd = new SQLiteCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@nodeTitle", node.NODE_TITLE);
                cmd.Parameters.AddWithValue("@idType", node.ID_NODE);
                cmd.Parameters.AddWithValue("@dateStart", node.DATE_START?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@dateEnd", node.DATE_END?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@assignee", node.ASSIGNEE);

                long newId = (long)cmd.ExecuteScalar();
                node.ID_NODE = (int)newId;
                return node;
            }
        }
    }

    /// <summary>
    /// 기존 노드 데이터를 INFO_NODES 테이블에서 수정합니다.
    /// </summary>
    /// <param name="node">수정할 NodeModel 객체</param>
    public void UpdateNode(NodeModel node)
    {
        string connString = GetConnectionString(INFO_NODES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string updateSql = @"
                UPDATE INFO_NODES
                SET NODE_TITLE = @nodeTitle,
                    ID_TYPE = @idType,
                    DATE_START = @dateStart,
                    DATE_END = @dateEnd,
                    ASSIGNEE = @assignee
                WHERE ID = @id;";

            using (var cmd = new SQLiteCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@nodeTitle", node.NODE_TITLE);
                cmd.Parameters.AddWithValue("@idType", node.ID_TYPE);
                cmd.Parameters.AddWithValue("@dateStart", node.DATE_START?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@dateEnd", node.DATE_END?.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@assignee", node.ASSIGNEE);
                cmd.Parameters.AddWithValue("@id", node.ID_NODE);
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// 지정된 ID의 노드를 INFO_NODES 테이블에서 삭제합니다.
    /// </summary>
    /// <param name="nodeId">삭제할 노드의 ID</param>
    public void DeleteNode(int nodeId)
    {
        string connString = GetConnectionString(INFO_NODES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_NODES WHERE ID = @id;";

            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.Parameters.AddWithValue("@id", nodeId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// INFO_NODES 테이블의 모든 노드 데이터를 불러옵니다.
    /// </summary>
    /// <returns>NodeModel 리스트</returns>
    public List<NodeModel> GetAllNodes()
    {
        List<NodeModel> nodes = new List<NodeModel>();
        string connString = GetConnectionString(INFO_NODES_DB);

        if (!File.Exists(Path.Combine(_tempDirPath, INFO_NODES_DB))) return nodes;

        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string selectSql = "SELECT ID, NODE_TITLE, ID_TYPE, DATE_START, DATE_END, ASSIGNEE FROM INFO_NODES;";
            using (var cmd = new SQLiteCommand(selectSql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nodes.Add(new NodeModel
                        {
                            ID_NODE = reader.GetInt32(0),
                            NODE_TITLE = reader.IsDBNull(1) ? null : reader.GetString(1),
                            ID_TYPE = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            DATE_START = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            DATE_END = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            ASSIGNEE = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }
            }
        }
        return nodes;
    }
    //---------------------------------------------------------------------------------------------------------------------------------


    // **INFO_PROPERTIES 테이블 CRUD 메서드**

    // 속성 아이템 추가
    public PropertyItem AddPropertyItem(PropertyItem item)
    {
        string connString = GetConnectionString(INFO_PROPERTIES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string insertSql = "INSERT INTO INFO_PROPERTIES (ID_TYPE, NAME, VALUE) VALUES (@ID_TYPE, @NAME, @VALUE); SELECT last_insert_rowid();";
            using (var cmd = new SQLiteCommand(insertSql, conn))
            {
                // PropertyItem.Type은 string이지만 DB 스키마는 INTEGER이므로 변환 필요
                // int.Parse() 또는 Convert.ToInt32() 사용
                int idType = int.Parse(item.Type);
                cmd.Parameters.AddWithValue("@ID_TYPE", idType);
                cmd.Parameters.AddWithValue("@NAME", item.Name);
                cmd.Parameters.AddWithValue("@VALUE", item.Value?.ToString());
                long newId = (long)cmd.ExecuteScalar();
                item.ID = (int)newId;
                return item;
            }
        }
    }

    // 속성 아이템 수정
    public void UpdatePropertyItem(PropertyItem item)
    {
        string connString = GetConnectionString(INFO_PROPERTIES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string updateSql = "UPDATE INFO_PROPERTIES SET ID_TYPE = @ID_TYPE, NAME = @NAME, VALUE = @VALUE WHERE ID = @ID;";
            using (var cmd = new SQLiteCommand(updateSql, conn))
            {
                int idType = int.Parse(item.Type);
                cmd.Parameters.AddWithValue("@ID_TYPE", idType);
                cmd.Parameters.AddWithValue("@NAME", item.Name);
                cmd.Parameters.AddWithValue("@VALUE", item.Value?.ToString());
                cmd.Parameters.AddWithValue("@ID", item.ID);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // 속성 아이템 삭제
    public void DeletePropertyItem(int propertyId)
    {
        string connString = GetConnectionString(INFO_PROPERTIES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_PROPERTIES WHERE ID = @ID;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", propertyId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // 모든 속성 아이템 불러오기
    public ObservableCollection<PropertyItem> LoadProperties()
    {
        var properties = new ObservableCollection<PropertyItem>();
        string connString = GetConnectionString(INFO_PROPERTIES_DB);

        if (!File.Exists(Path.Combine(_tempDirPath, INFO_PROPERTIES_DB))) return properties;

        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string sql = "SELECT ID, ID_TYPE, NAME, VALUE FROM INFO_PROPERTIES";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        properties.Add(new PropertyItem
                        {
                            ID = reader.GetInt32(0),
                            Type = reader.GetInt32(1).ToString(), // ID_TYPE은 INTEGER이므로 int로 읽고 string으로 변환
                            Name = reader.GetString(2),
                            Value = reader.IsDBNull(3) ? null : reader.GetString(3) // VALUE는 TEXT로 읽음
                        });
                    }
                }
            }
        }
        return properties;
    }

    // (참고) INFO_TYPES 테이블 데이터 로드 메서드
    public ObservableCollection<NodeProcessType> LoadNodeProcessTypes()
    {
        var types = new ObservableCollection<NodeProcessType>();
        string connString = GetConnectionString(INFO_TYPES_DB);

        if (!File.Exists(Path.Combine(_tempDirPath, INFO_TYPES_DB))) return types;

        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string sql = "SELECT ID, TYPE, COLOR_R, COLOR_G, COLOR_B FROM INFO_TYPES";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(new NodeProcessType
                        {
                            ID = reader.GetInt32(0),
                            NAME = reader.GetString(1), // TYPE 컬럼은 NAME으로 매핑
                            COLOR_R = reader.GetInt32(2),
                            COLOR_G = reader.GetInt32(3),
                            COLOR_B = reader.GetInt32(4)
                        });
                    }
                }
            }
        }
        return types;
    }

    // 테이블 초기화
    /// <summary>
    /// INFO_NODES 테이블의 모든 노드 데이터를 삭제합니다.
    /// </summary>
    private void DeleteAllNodes()
    {
        string connString = GetConnectionString(INFO_NODES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_NODES;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// INFO_LINKS 테이블의 모든 링크 데이터를 삭제합니다.
    /// </summary>
    private void DeleteAllLinks()
    {
        string connString = GetConnectionString(INFO_LINKS_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_LINKS;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// INFO_PROPERTIES 테이블의 모든 속성 데이터를 삭제합니다.
    /// </summary>
    private void DeleteAllProperties()
    {
        string connString = GetConnectionString(INFO_PROPERTIES_DB);
        using (var conn = new SQLiteConnection(connString))
        {
            conn.Open();
            string deleteSql = "DELETE FROM INFO_PROPERTIES;";
            using (var cmd = new SQLiteCommand(deleteSql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}