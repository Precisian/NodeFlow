// NodeViewModel.cs
using Client.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        // DataGrid에 표시될 속성 컬렉션
        public ObservableCollection<PropertyItem> BasicProperties { get; private set; }

        // INotifyPropertyChanged를 구현하는 도우미 메서드
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 노드 선택 액션을 상위 뷰모델로부터 전달받음
        private readonly Action<NodeViewModel> _selectAction;

        // 노드 클릭 시 실행될 커맨드
        public ICommand SelectNodeCommand { get; }
        public ICommand DragNodeCommand { get; }

        // 추가: 외부에서 전달받을 커맨드
        public ICommand ConnectNodesCommand { get; set; }

        // 노드 데이터 모델
        public NodeModel NodeData { get; private set; }

        public NodeViewModel(NodeModel nodeModel, Action<NodeViewModel> selectAction)
        {
            this.NodeData = nodeModel;
            this._selectAction = selectAction;

            // SelectNodeCommand 초기화:
            // 이 커맨드가 실행되면 _selectAction을 호출하고, 현재 인스턴스(this)를 파라미터로 전달합니다.
            SelectNodeCommand = new RelayCommand(param => _selectAction(this));
            DragNodeCommand = new RelayCommand(DragNode);

            // 기본 속성들
            BasicProperties = new ObservableCollection<PropertyItem>();
            UpdateBasicProperties();
        }

        public void UpdateBasicProperties()
        {
            BasicProperties.Clear();

            // 노드 모델의 데이터를 PropertyItem으로 변환하여 추가합니다.
            BasicProperties.Add(new PropertyItem { Name = "ID", Value = NodeData.ID_NODE, Type = "Integer" });
            BasicProperties.Add(new PropertyItem { Name = "작업명", Value = NodeData.NODE_TITLE, Type = "String" });
            BasicProperties.Add(new PropertyItem { Name = "담당자", Value = NodeData.Assignee, Type = "String" });
            BasicProperties.Add(new PropertyItem { Name = "시작일", Value = NodeData.DATE_START?.ToShortDateString(), Type = "Date" });
            BasicProperties.Add(new PropertyItem { Name = "종료일", Value = NodeData.DATE_END?.ToShortDateString(), Type = "Date" });
            BasicProperties.Add(new PropertyItem { Name = "진행 상태", Value = NodeData.ProcessType?.NAME, Type = "String" });
        }

        // 노드의 헤더 색상 속성입니다.
        public Color NodeHeaderColor
        {
            get => NodeData.NodeColor;
            set
            {
                if (NodeData.NodeColor != value)
                {
                    NodeData.NodeColor = value;
                    OnPropertyChanged(nameof(NodeHeaderColor));
                }
            }
        }

        // 노드의 작업명 속성입니다.
        public string TaskName
        {
            get => NodeData.NODE_TITLE;
            set
            {
                if (NodeData.NODE_TITLE != value)
                {
                    NodeData.NODE_TITLE = value;
                    OnPropertyChanged(nameof(TaskName));
                }
            }
        }

        // 노드의 시작일 속성입니다.
        public string StartDate
        {
            get => NodeData.DATE_START?.ToString("d");
            set
            {
                if (DateTime.TryParse(value, out DateTime newDate))
                {
                    NodeData.DATE_START = newDate;
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        // 노드의 종료일 속성입니다.
        public string EndDate
        {
            get => NodeData.DATE_END?.ToString("d");
            set
            {
                if (DateTime.TryParse(value, out DateTime newDate))
                {
                    NodeData.DATE_END = newDate;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }

        // 노드의 담당자 속성입니다.
        public string Assignee
        {
            get => NodeData.Assignee;
            set
            {
                if (NodeData.Assignee != value)
                {
                    NodeData.Assignee = value;
                    OnPropertyChanged(nameof(Assignee));
                }
            }
        }

        // 캔버스 내의 X 좌표입니다. (뷰의 위치와 관련된 속성) 
        public double XPosition
        {
            get => NodeData.XPosition;
            set
            {
                if (NodeData.XPosition != value)
                {
                    NodeData.XPosition = value;
                    OnPropertyChanged(nameof(XPosition));
                }
            }
        }

        // 캔버스 내의 Y 좌표입니다. (뷰의 위치와 관련된 속성)
        public double YPosition
        {
            get => NodeData.YPosition;
            set
            {
                if (NodeData.YPosition != value)
                {
                    NodeData.YPosition = value;
                    OnPropertyChanged(nameof(YPosition));
                }
            }
        }

        public const double Default_NodeWidth = 150;
        public const double Default_NodeHeight = 135;

        public double Width
        {
            get => NodeData.Width;
            set
            {
                if (NodeData.Width != value)
                {
                    NodeData.Width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Height
        {
            get => NodeData.Height;
            set
            {
                if (NodeData.Height != value)
                {
                    NodeData.Height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        // 노드의 선택 상태를 나타냅니다.
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private void DragNode(object parameter)
        {
            if(parameter is Point newPosition)
            {
                XPosition = newPosition.X;
                YPosition = newPosition.Y;
            }
        }

        private double _nodeScale = 1.0;
        public double NodeScale
        {
            get => _nodeScale;
            set
            {
                if (_nodeScale != value)
                {
                    _nodeScale = value;
                    OnPropertyChanged(nameof(NodeScale));
                }
            }
        }
    }
}
