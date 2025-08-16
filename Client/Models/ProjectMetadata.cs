// ProjectMetadata.cs (새 파일로 추가)
using System;

public class ProjectMetadata
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModifiedDate { get; set; }

    public ProjectMetadata()
    {
        CreationDate = DateTime.Now;
        LastModifiedDate = DateTime.Now;
    }
}