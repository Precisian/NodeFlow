// LinkModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models
{
    /// <summary>
    /// 노드 간의 연결 정보를 나타내는 데이터 모델 클래스
    /// </summary>
    public class LinkModel : INotifyPropertyChanged
    {
        // PropertyChanged 이벤트 정의
        public event PropertyChangedEventHandler PropertyChanged;
        // 속성 변경 이벤트를 발생시키는 메서드
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // 데이터베이스의 ID에 대응하는 고유 식별자
        private int _id;
        public int ID 
        {
            get => _id;
            set
            {
                if(value != _id)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            } 
        }


        // 연결의 시작 노드 ID
        private int _id_node_src;
        public int ID_NODE_SRC 
        {
            get => _id_node_src;
            set
            {
                if (_id_node_src != value)
                {
                    _id_node_src = value;
                    OnPropertyChanged(nameof(ID_NODE_SRC));
                }
            }
        }

        // 연결의 대상 노드 ID
        private int _id_node_tgt;
        public int ID_NODE_TGT
        {
            get => _id_node_tgt;
            set
            {
                if (_id_node_tgt != value)
                {
                    _id_node_tgt = value;
                    OnPropertyChanged(nameof(ID_NODE_TGT));
                }
            }
        }

        // 연결이 생성된 날짜 및 시간
        public DateTime? CREATED_AT { get; set; }

        public LinkModel(int id_src, int id_tgt)
        {
            // 기본 생성자
            CREATED_AT = DateTime.Now;

            ID_NODE_SRC = id_src;
            ID_NODE_TGT = id_tgt;
        }
    }
}