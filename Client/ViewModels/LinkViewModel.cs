using System.ComponentModel;
using System.Windows;
using Client.Models;
using Client.ViewModels;

namespace Client.ViewModels
{
    public class LinkViewModel : INotifyPropertyChanged
    {
        private NodeViewModel _startNode;
        private NodeViewModel _endNode;
        private Point _startPoint;
        private Point _endPoint;

        public event PropertyChangedEventHandler PropertyChanged;

        public LinkModel LinkData { get; private set; } 
        public NodeViewModel StartNode
        {
            get => _startNode;
            set
            {
                if (_startNode != value)
                {
                    // 기존 노드의 PropertyChanged 이벤트 구독 해제
                    if (_startNode != null)
                    {
                        _startNode.PropertyChanged -= OnNodePositionChanged;
                    }

                    _startNode = value;
                    // 새 노드의 PropertyChanged 이벤트 구독
                    if (_startNode != null)
                    {
                        _startNode.PropertyChanged += OnNodePositionChanged;
                        UpdatePoints();
                    }
                }
            }
        }

        public NodeViewModel EndNode
        {
            get => _endNode;
            set
            {
                if (_endNode != value)
                {
                    // 기존 노드의 PropertyChanged 이벤트 구독 해제
                    if (_endNode != null)
                    {
                        _endNode.PropertyChanged -= OnNodePositionChanged;
                    }

                    _endNode = value;
                    // 새 노드의 PropertyChanged 이벤트 구독
                    if (_endNode != null)
                    {
                        _endNode.PropertyChanged += OnNodePositionChanged;
                        UpdatePoints();
                    }
                }
            }
        }

        public Point StartPoint
        {
            get => _startPoint;
            private set
            {
                if (_startPoint != value)
                {
                    _startPoint = value;
                    OnPropertyChanged(nameof(StartPoint));
                    UpdatePoints();
                }
            }
        }

        public Point EndPoint
        {
            get => _endPoint;
            private set
            {
                if (_endPoint != value)
                {
                    _endPoint = value;
                    OnPropertyChanged(nameof(EndPoint));
                    UpdatePoints();
                }
            }
        }

        public LinkViewModel(NodeViewModel startNode, NodeViewModel endNode)
        {
            LinkData = new LinkModel();
            StartNode = startNode;
            EndNode = endNode;
        }

        private void OnNodePositionChanged(object sender, PropertyChangedEventArgs e)
        {
            // 노드의 위치가 변경되었을 때만 링크의 좌표를 업데이트합니다.
            if (e.PropertyName == nameof(NodeViewModel.XPosition) || e.PropertyName == nameof(NodeViewModel.YPosition))
            {
                UpdatePoints();
            }
        }

        private void UpdatePoints()
        {
            // 💡 EndNode가 null인지 확인하고, null이면 함수를 종료합니다.
            if (StartNode == null || EndNode == null)
            {
                return;
            }

            // 시작 노드와 끝 노드의 위치를 기반으로 링크의 시작점과 끝점을 계산합니다.
            // 노드 뷰의 폭과 높이가 150x135라고 가정하고 중앙점을 계산합니다.
            double nodeWidth = NodeViewModel.Default_NodeWidth;
            double nodeHeight = NodeViewModel.Default_NodeHeight;

            // 시작 노드의 우측 중앙 지점을 계산
            StartPoint = new Point(StartNode.XPosition + nodeWidth, StartNode.YPosition + nodeHeight / 2);
            LinkData.ID_NODE_SRC = StartNode.NodeData.ID_NODE;

            // 끝 노드의 좌측 중앙 지점을 계산
            EndPoint = new Point(EndNode.XPosition, EndNode.YPosition + nodeHeight / 2);
            LinkData.ID_NODE_TGT = EndNode.NodeData.ID_NODE;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
