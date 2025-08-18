// ProjectMetadata.cs (새 파일로 추가)
using System;
using System.Text.Json.Serialization; // JsonPropertyName 어노테이션을 사용하기 위해 추가

public class ProjectMetadata
{
    // JSON의 "ProjectName"과 매핑되는 속성
    [JsonPropertyName("ProjectName")]
    public string ProjectName { get; set; }

    // JSON의 "Description"과 매핑되는 속성
    [JsonPropertyName("Description")]
    public string Description { get; set; }

    // JSON의 "CreationDate"와 매핑되는 속성
    [JsonPropertyName("CreationDate")]
    public DateTime? CreationDate { get; set; }

    // JSON의 "LastModifiedDate"와 매핑되는 속성
    // Nullable 타입으로 변경하여 JSON에 값이 없거나 유효하지 않아도 오류 발생을 방지
    [JsonPropertyName("LastModifiedDate")]
    public DateTime? LastModifiedDate { get; set; }

    public ProjectMetadata()
    {
        // 생성 시 현재 날짜/시간으로 초기화
        CreationDate = DateTime.Now;
        LastModifiedDate = DateTime.Now;
    }
}