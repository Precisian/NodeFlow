using Client.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        // 모델 인스턴스를 필드로 가집니다.
        private NodeModel _nodeModel;

        private double _xPosition;
        private double _yPosition;
        private bool _isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        // INotifyPropertyChanged를 구현하는 도우미 메서드
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 노드의 제목 속성입니다.
        public string NodeTitle
        {
            get => _nodeModel.NodeTitle;
            set
            {
                _nodeModel.NodeTitle = value;
                OnPropertyChanged(nameof(NodeTitle));
            }
        }

        // 노드의 헤더 색상 속성입니다.
        public string NodeHeaderColor
        {
            get => _nodeModel.NodeHeaderColor;
            set
            {
                _nodeModel.NodeHeaderColor = value;
                OnPropertyChanged(nameof(NodeHeaderColor));
            }
        }

        // 노드의 작업명 속성입니다.
        public string TaskName
        {
            get => _nodeModel.TaskName;
            set
            {
                _nodeModel.TaskName = value;
                OnPropertyChanged(nameof(TaskName));
            }
        }

        // 노드의 시작일 속성입니다.
        public string StartDate
        {
            get => _nodeModel.StartDate;
            set
            {
                _nodeModel.StartDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        // 노드의 종료일 속성입니다.
        public string EndDate
        {
            get => _nodeModel.EndDate;
            set
            {
                _nodeModel.EndDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        // 노드의 담당자 속성입니다.
        public string Assignee
        {
            get => _nodeModel.Assignee;
            set
            {
                _nodeModel.Assignee = value;
                OnPropertyChanged(nameof(Assignee));
            }
        }

        // 캔버스 내의 X 좌표입니다. (뷰의 위치와 관련된 속성)
        public double XPosition
        {
            get => _xPosition;
            set
            {
                _xPosition = value;
                OnPropertyChanged(nameof(XPosition));
            }
        }

        // 캔버스 내의 Y 좌표입니다. (뷰의 위치와 관련된 속성)
        public double YPosition
        {
            get => _yPosition;
            set
            {
                _yPosition = value;
                OnPropertyChanged(nameof(YPosition));
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

        // 노드를 제거하는 명령입니다.
        public ICommand RemoveNodeCommand { get; }

        public NodeViewModel(NodeModel nodeModel)
        {
            _nodeModel = nodeModel;
            // RemoveNodeCommand = new RelayCommand(RemoveNode);
        }

        // 노드를 제거하는 로직. 이 부분은 MainWindowViewModel에서 처리할 수 있습니다.
        // private void RemoveNode(object parameter)
        // {
        //     // 제거 로직 구현
        // }
    }
}