// NodeViewModel.cs
using Client.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Media;
using System.Windows.Media;

namespace Client.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        // 핵심 데이터인 NodeModel을 속성으로 가집니다.
        public NodeModel NodeData { get; private set; }

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
            get => NodeData.NODE_TITLE;
            set
            {
                if (NodeData.NODE_TITLE != value)
                {
                    NodeData.NODE_TITLE = value;
                    OnPropertyChanged(nameof(NodeTitle));
                }
            }
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
        public string Manager
        {
            get => NodeData.ASSIGNEE;
            set
            {
                if (NodeData.ASSIGNEE != value)
                {
                    NodeData.ASSIGNEE = value;
                    OnPropertyChanged(nameof(Manager));
                }
            }
        }

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
        // 이 명령은 MainWindowViewModel에 존재해야 하므로 주석 처리 또는 제거 필요
        // public ICommand RemoveNodeCommand { get; }

        public NodeViewModel(NodeModel nodeModel)
        {
            this.NodeData = nodeModel; // 생성자에서 NodeData 속성에 모델을 할당
        }
    }
}