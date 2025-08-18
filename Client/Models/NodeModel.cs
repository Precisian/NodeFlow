// NodeModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Client.Models
{
    // INFO_TYPES 테이블의 데이터 구조에 해당하는 클래스
    public class NodeProcessType
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public int COLOR_R { get; set; }
        public int COLOR_G { get; set; }
        public int COLOR_B { get; set; }
    }

    public class NodeModel : INotifyPropertyChanged
    {
        // PropertyChanged 이벤트 정의
        public event PropertyChangedEventHandler PropertyChanged;

        // 속성 변경 이벤트를 발생시키는 메서드
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        // INFO_NODES 테이블과 일치하는 속성들

        // 노드 번호 (Primary Key)
        private int _idNode;
        public int ID_NODE
        {
            get => _idNode;
            set
            {
                if (_idNode != value)
                {
                    _idNode = value;
                    OnPropertyChanged(nameof(ID_NODE));
                }
            }
        }

        // 노드 제목
        private string _nodeTitle;
        public string NODE_TITLE
        {
            get => _nodeTitle;
            set
            {
                if (_nodeTitle != value)
                {
                    _nodeTitle = value;
                    OnPropertyChanged(nameof(NODE_TITLE));
                }
            }
        }

        // 시작일
        private DateTime? _dateStart;
        public DateTime? DATE_START
        {
            get => _dateStart;
            set
            {
                if (_dateStart != value)
                {
                    _dateStart = value;
                    OnPropertyChanged(nameof(DATE_START));
                }
            }
        }

        // 종료일
        private DateTime? _dateEnd;
        public DateTime? DATE_END
        {
            get => _dateEnd;
            set
            {
                if (_dateEnd != value)
                {
                    _dateEnd = value;
                    OnPropertyChanged(nameof(DATE_END));
                }
            }
        }

        // 담당자
        private string _assignee;
        public string Assignee
        {
            get => _assignee;
            set
            {
                if (_assignee != value)
                {
                    _assignee = value;
                    OnPropertyChanged(nameof(Assignee));
                }
            }
        }

        // 노드 타입 (INFO_TYPES 테이블의 ID와 연결)
        private int _idType;
        public int ID_TYPE
        {
            get => _idType;
            set
            {
                if (_idType != value)
                {
                    _idType = value;
                    OnPropertyChanged(nameof(ID_TYPE));

                    // ID_TYPE 변경 시 노드 색상과 타입 정보 업데이트
                    UpdateNodeColorAndType();
                }
            }
        }

        //---------------------------------------------------------
        // UI 표시를 위한 추가 속성
        // DB에서 가져온 타입 정보 (이 속성은 INotifyPropertyChanged 구현 필요 없음)
        public NodeProcessType ProcessType { get; set; }

        // UI에 바인딩할 Color 속성
        private Color _nodeColor;
        public Color NodeColor
        {
            get => _nodeColor;
            set
            {
                if (_nodeColor != value)
                {
                    _nodeColor = value;
                    OnPropertyChanged(nameof(NodeColor));
                }
            }
        }

        private double _xPosition;
        private double _yPosition;
        // 캔버스 내의 X 좌표입니다. (뷰의 위치와 관련된 속성)
        public double XPosition
        {
            get => _xPosition;
            set
            {
                if (_xPosition != value)
                {
                    _xPosition = value;
                    OnPropertyChanged(nameof(XPosition));
                }
            }
        }

        // 캔버스 내의 Y 좌표입니다. (뷰의 위치와 관련된 속성)
        public double YPosition
        {
            get => _yPosition;
            set
            {
                if (_yPosition != value)
                {
                    _yPosition = value;
                    OnPropertyChanged(nameof(YPosition));
                }
            }
        }

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                if(_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                if(_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }
        private string _pathCustom;
        public string PathCustom
        {
            get => _pathCustom;
            set
            {
                if (_pathCustom != value)
                {
                    _pathCustom = value;
                    OnPropertyChanged(nameof(PathCustom));
                }
            }
        }

        // 사용자 정의 속성을 저장하는 컬렉션
        private ObservableCollection<PropertyItem> _customProperties;
        public ObservableCollection<PropertyItem> CustomProperties
        {
            get => _customProperties;
            set
            {
                _customProperties = value;
                OnPropertyChanged(nameof(CustomProperties));
            }
        }

        //---------------------------------------------------------
        // 생성자
        public NodeModel()
        {
            CustomProperties = new ObservableCollection<PropertyItem>();
        }

        // DB에서 데이터를 불러온 후 호출할 메서드
        public void UpdateNodeColorAndType(NodeProcessType type = null)
        {
            if (type != null)
            {
                // NodeType 객체의 RGB 값을 사용해 색상 설정
                this.NodeColor = Color.FromRgb((byte)type.COLOR_R, (byte)type.COLOR_G, (byte)type.COLOR_B);
                this.ProcessType = type;
            }
            else
            {
                // 기본값 설정
                this.NodeColor = Colors.Gray;
                this.ProcessType = new NodeProcessType { NAME = "알 수 없음" };
            }
        }
    }
}