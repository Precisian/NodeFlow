using Client.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ObservableCollection<NodeProcessType> NodeProcessTypes { get; }

        // INotifyPropertyChanged를 구현하는 도우미 메서드
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly Action<NodeViewModel> _selectAction;

        public ICommand SelectNodeCommand { get; }
        public ICommand DragNodeCommand { get; }
        public ICommand ConnectNodesCommand { get; set; }

        public NodeModel NodeData { get; private set; }

        public NodeViewModel(NodeModel nodeData, Action<NodeViewModel> selectAction, ObservableCollection<NodeProcessType> nodeProcessTypes)
        {
            NodeProcessTypes = nodeProcessTypes;
            this.NodeData = nodeData;
            this._selectAction = selectAction;

            SelectNodeCommand = new RelayCommand(param => _selectAction(this));
            DragNodeCommand = new RelayCommand(DragNode);

            BasicProperties = new ObservableCollection<PropertyItem>();
            UpdateBasicProperties();
        }

        /// <summary>
        /// NodeModel의 데이터를 기반으로 BasicProperties 컬렉션을 업데이트합니다.
        /// 각 PropertyItem에 이벤트 핸들러를 등록하여 변경 사항을 감지합니다.
        /// </summary>
        public void UpdateBasicProperties()
        {
            // 💡 가장 중요한 부분: 기존 아이템들을 모두 삭제합니다.
            // 기존 아이템들의 이벤트 핸들러를 먼저 제거
            foreach (var item in BasicProperties)
            {
                item.PropertyChanged -= OnPropertyItemChanged;
            }
            BasicProperties.Clear();

            // NodeModel의 데이터를 기반으로 새로운 PropertyItem을 추가
            BasicProperties.Add(new PropertyItem { Name = "ID", Value = NodeData.ID_NODE, Type = "Integer" });
            BasicProperties.Add(new PropertyItem { Name = "작업명", Value = NodeData.NODE_TITLE, Type = "String" });
            BasicProperties.Add(new PropertyItem { Name = "담당자", Value = NodeData.Assignee, Type = "String" });
            BasicProperties.Add(new PropertyItem { Name = "시작일", Value = NodeData.DATE_START, Type = "Date" });
            BasicProperties.Add(new PropertyItem { Name = "종료일", Value = NodeData.DATE_END, Type = "Date" });
            BasicProperties.Add(new PropertyItem { Name = "진행 상태", Value = NodeData.ProcessType, Type = "NodeProcessType" });

            // 💡 새로 추가된 아이템들에 이벤트 핸들러 다시 등록
            foreach (var item in BasicProperties)
            {
                item.PropertyChanged += OnPropertyItemChanged;
            }
        }

        private void OnPropertyItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value" && sender is PropertyItem changedItem)
            {
                // PropertyItem의 변경 내용을 NodeModel에 반영
                switch (changedItem.Name)
                {
                    case "작업명":
                        NodeData.NODE_TITLE = changedItem.Value?.ToString();
                        OnPropertyChanged(nameof(TaskName));
                        break;
                    case "담당자":
                        NodeData.Assignee = changedItem.Value?.ToString();
                        OnPropertyChanged(nameof(Assignee));
                        break;
                    case "시작일":
                        NodeData.DATE_START = changedItem.Value as DateTime?;
                        OnPropertyChanged(nameof(StartDate));
                        break;
                    case "종료일":
                        NodeData.DATE_END = changedItem.Value as DateTime?;
                        OnPropertyChanged(nameof(EndDate));
                        break;
                    case "진행 상태":
                        // ComboBox에서 선택된 NodeProcessType 객체를 직접 할당
                        NodeData.ProcessType = changedItem.Value as NodeProcessType;
                        OnPropertyChanged(nameof(ProcessType));
                        OnPropertyChanged(nameof(NodeHeaderColor)); // 상태 변경 시 노드 색상도 업데이트
                        break;
                }
            }
        }

        // 이하는 기존 코드와 동일합니다.

        // 노드의 헤더 색상 속성
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

        // 노드의 작업명 속성
        public string TaskName
        {
            get => NodeData.NODE_TITLE;
            set
            {
                if (NodeData.NODE_TITLE != value)
                {
                    NodeData.NODE_TITLE = value;
                    OnPropertyChanged(nameof(TaskName));
                    var item = BasicProperties.FirstOrDefault(p => p.Name == "작업명");
                    if (item != null)
                    {
                        item.Value = value;
                    }
                }
            }
        }

        // 노드의 시작일 속성
        public DateTime? StartDate
        {
            get => NodeData.DATE_START;
            set
            {
                if (NodeData.DATE_START != value)
                {
                    NodeData.DATE_START = value;
                    OnPropertyChanged(nameof(StartDate));
                    var item = BasicProperties.FirstOrDefault(p => p.Name == "시작일");
                    if (item != null)
                    {
                        item.Value = value;
                    }
                }
            }
        }

        // 노드의 종료일 속성
        public DateTime? EndDate
        {
            get => NodeData.DATE_END;
            set
            {
                if (NodeData.DATE_END != value)
                {
                    NodeData.DATE_END = value;
                    OnPropertyChanged(nameof(EndDate));
                    var item = BasicProperties.FirstOrDefault(p => p.Name == "종료일");
                    if (item != null)
                    {
                        item.Value = value;
                    }
                }
            }
        }

        // 노드의 담당자 속성
        public string Assignee
        {
            get => NodeData.Assignee;
            set
            {
                if (NodeData.Assignee != value)
                {
                    NodeData.Assignee = value;
                    OnPropertyChanged(nameof(Assignee));
                    var item = BasicProperties.FirstOrDefault(p => p.Name == "담당자");
                    if (item != null)
                    {
                        item.Value = value;
                    }
                }
            }
        }

        // 노드의 진행 상태
        public NodeProcessType ProcessType
        {
            get => NodeData.ProcessType;
            set
            {
                if (NodeData.ProcessType != value)
                {
                    NodeData.ProcessType = value;
                    OnPropertyChanged(nameof(ProcessType));
                    var item = BasicProperties.FirstOrDefault(p => p.Name == "진행 상태");
                    if (item != null)
                    {
                        item.Value = value;
                    }
                }
            }
        }

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
            if (parameter is Point newPosition)
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

        public const double Default_NodeWidth = 150;
        public const double Default_NodeHeight = 135;
    }
}