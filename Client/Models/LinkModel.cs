// LinkModel.cs
using System;

namespace Client.Models
{
    /// <summary>
    /// 노드 간의 연결 정보를 나타내는 데이터 모델 클래스
    /// </summary>
    public class LinkModel
    {
        // 데이터베이스의 ID에 대응하는 고유 식별자
        public int ID { get; set; }

        // 연결의 시작 노드 ID
        public int ID_NODE_SRC { get; set; }

        // 연결의 대상 노드 ID
        public int ID_NODE_TGT { get; set; }

        // 연결이 생성된 날짜 및 시간
        public DateTime CREATED_AT { get; set; }

        public LinkModel()
        {
            // 기본 생성자
            CREATED_AT = DateTime.Now;
        }
    }
}