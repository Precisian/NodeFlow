using Client.Models;
using Client.Views;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // 캔버스 사이즈
        private double _canvasWidth;
        public double CanvasWidth {
            get => _canvasWidth;
            set
            {
                if (_canvasWidth != value)
                {
                    _canvasWidth = value;
                    OnPropertyChanged(nameof(CanvasWidth));
                }
            }
        }

        private double _canvasHeight;
        public double CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                if (_canvasHeight != value)
                {
                    _canvasHeight = value;
                    OnPropertyChanged(nameof(CanvasHeight));
                }
            }
        }
        
        // 새롭게 추가할 캔버스 변환 관련 속성들
        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        private double _offsetX = 0;
        public double OffsetX
        {
            get => _offsetX;
            set
            {
                if (_offsetX != value)
                {
                    _offsetX = value;
                    OnPropertyChanged(nameof(OffsetX));
                }
            }
        }

        private double _offsetY = 0;
        public double OffsetY
        {
            get => _offsetY;
            set
            {
                if (_offsetY != value)
                {
                    _offsetY = value;
                    OnPropertyChanged(nameof(OffsetY));
                }
            }
        }

        private DBManager _dbManager;
        private string _currentFilePath;
        public ProjectMetadata projectMetadata { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<NodeViewModel> _nodes;
        public ObservableCollection<NodeViewModel> Nodes
        {
            get => _nodes;
            set
            {
                _nodes = value;
                OnPropertyChanged(nameof(Nodes));
            }
        }

        // 선택된 노드를 나타내는 속성
        private NodeViewModel _selectedNode;
        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    // 이전 선택된 노드가 있다면 선택 해제
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = false;
                    }

                    _selectedNode = value;
                    OnPropertyChanged(nameof(SelectedNode));

                    // 새롭게 선택된 노드가 있다면 선택 상태로 변경
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = true;
                    }
                }
            }
        }
        private ObservableCollection<LinkViewModel> _links;
        public ObservableCollection<LinkViewModel> Links
        {
            get => _links;
            set
            {
                _links = value;
                OnPropertyChanged(nameof(Links));
            }   
        }

        private ObservableCollection<PropertyItem> _properties;
        public ObservableCollection<PropertyItem> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                OnPropertyChanged(nameof(Properties));
            }
        }

        private ObservableCollection<string> _comboBoxItems;
        public ObservableCollection<string> ComboBoxItems
        {
            get => _comboBoxItems;
            set
            {
                _comboBoxItems = value;
                OnPropertyChanged(nameof(ComboBoxItems));
            }
        }

        private ObservableCollection<NodeProcessType> _nodeProcessTypes;
        public ObservableCollection<NodeProcessType> NodeProcessTypes
        {
            get => _nodeProcessTypes;
            set
            {
                _nodeProcessTypes = value;
                OnPropertyChanged(nameof(NodeProcessTypes));
            }
        }

        private bool _isProjectOpen;
        public bool IsProjectOpen
        {
            get => _isProjectOpen;
            set
            {
                if (_isProjectOpen != value)
                {
                    _isProjectOpen = value;
                    OnPropertyChanged(nameof(IsProjectOpen));

                    ((RelayCommand)SaveProjectCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AddNodeAtPositionCommand).RaiseCanExecuteChanged(); // 커맨드 이름 변경
                    ((RelayCommand)RemoveNodeCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ManageNodesCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AdjustCanvasSizeCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ShowPropertiesCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeletePropertiesCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ResetViewCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isNodeSelected;
        public bool IsNodeSelected
        {
            get => _isNodeSelected;
            set
            {
                if (_isNodeSelected != value)
                {
                    _isNodeSelected = value;
                    OnPropertyChanged(nameof(IsNodeSelected));
                }
            }
        }

        private string _windowTitle;
        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (_windowTitle != value)
                {
                    _windowTitle = value;
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        public bool IsDirty { get; set; } = false;

        public ICommand NewProjectCommand { get; }
        public ICommand LoadProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand ExitApplicationCommand { get; }
        // 💡 기존 AddNodeCommand를 제거하고 AddNodeAtPositionCommand로 변경
        public ICommand AddNodeAtPositionCommand { get; }
        public ICommand RemoveNodeCommand { get; }
        public ICommand ManageNodesCommand { get; }
        public ICommand AdjustCanvasSizeCommand { get; }
        public ICommand ShowPropertiesCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand DeletePropertiesCommand { get; }
        public ICommand ResetViewCommand { get; }
        public ICommand ConnectNodesCommand { get; }
        public ICommand DeleteLinkSelectedNodeCommand { get; }

        public MainWindowViewModel()
        {
            // 캔버스 사이즈 기본값 초기화
            this.CanvasWidth = 4000;
            this.CanvasHeight = 4000;

            // 데이터 담을 컬렉션 생성
            this.Nodes = new ObservableCollection<NodeViewModel>();
            this.Links = new ObservableCollection<LinkViewModel>();
            this.Properties = new ObservableCollection<PropertyItem>();

            _dbManager = new DBManager();
            this.projectMetadata = _dbManager.projectMetadata;

            this.NodeProcessTypes = new ObservableCollection<NodeProcessType> {
                new NodeProcessType { ID = 1, NAME = "계획", COLOR_R = 128, COLOR_G = 128, COLOR_B = 128 },
                new NodeProcessType { ID = 2, NAME = "진행중", COLOR_R = 255, COLOR_G = 165, COLOR_B = 0 },
                new NodeProcessType { ID = 3, NAME = "완료", COLOR_R = 0, COLOR_G = 128, COLOR_B = 0 },
                new NodeProcessType { ID = 4, NAME = "보류", COLOR_R = 255, COLOR_G = 255, COLOR_B = 0 },
                new NodeProcessType { ID = 5, NAME = "진행불가", COLOR_R = 255, COLOR_G = 0, COLOR_B = 0 },
                new NodeProcessType { ID = 6, NAME = "실패", COLOR_R = 0, COLOR_G = 0, COLOR_B = 0 }
            };

            NewProjectCommand = new RelayCommand(NewProject);
            LoadProjectCommand = new RelayCommand(LoadProject);
            SaveProjectCommand = new RelayCommand(SaveProject, CanProjectExist);
            ExitApplicationCommand = new RelayCommand(ExitApplication);
            AddNodeAtPositionCommand = new RelayCommand(AddNodeAtPosition, CanProjectExist);
            RemoveNodeCommand = new RelayCommand(RemoveNode, CanRemoveNode);
            ManageNodesCommand = new RelayCommand(ManageNodes, CanProjectExist);
            AdjustCanvasSizeCommand = new RelayCommand(AdjustCanvasSize, CanProjectExist);
            ShowPropertiesCommand = new RelayCommand(ShowProperties, CanProjectExist);
            ShowHelpCommand = new RelayCommand(ShowHelp);
            DeletePropertiesCommand = new RelayCommand(DeleteProperties, CanProjectExist);
            ResetViewCommand = new RelayCommand(ResetView, CanProjectExist);
            ConnectNodesCommand = new RelayCommand(ConnectNodes);
            DeleteLinkSelectedNodeCommand = new RelayCommand(DeleteLinksForSelectedNode, CanRemoveNode);


            IsProjectOpen = false;
            UpdateWindowTitle();
        }

        // 이 메서드는 NodeViewModel의 커맨드로부터 호출됩니다.
        private void ConnectNodes(object parameter)
        {
            if (parameter is LinkViewModel link)
            {
                //이미 링크가 존재하는지 확인하는 로직 추가
                var existingLink = Links.FirstOrDefault(l => (l.StartNode == link.StartNode && l.EndNode == link.EndNode));

                // 이미 같은 링크가 존재하면 새로 만들지 않습니다.
                if (existingLink == null)
                {
                    Console.WriteLine($"링크 생성: {link.StartNode.NodeData.ID_NODE} -> {link.EndNode.NodeData.ID_NODE}");

                    this.Links.Add(link);
                    _dbManager.AddLink(Links.Last().LinkData);
                }
                else
                {
                    Console.WriteLine("이미 존재하는 링크입니다.");
                }
            }
        }

        // 새 프로젝트 생성 로직
        private void NewProject(object parameter)
        {
            this._dbManager.CreateNewProject();

            while (this.Links.Count > 0)
            {
                this.Links.Remove(this.Links.First());
            }
            while (this.Nodes.Count > 0)
            {
                this.Nodes.Remove(this.Nodes.First());
            }

            this.IsProjectOpen = true;
            this.IsDirty = false;
            this._currentFilePath = null;
            this.SelectedNode = null;
            this.projectMetadata.ProjectName = "[제목 없음]";
            Console.WriteLine("[DBManager] 새 프로젝트 생성");
            UpdateWindowTitle();

            ResetView(null);
        }

        // 프로젝트 불러오기 로직
        private void LoadProject(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "NodeFlow 파일(*.nf)|*.nf";

            if (openFileDialog.ShowDialog() == true)
            {
                // 파일 경로 설정 및 DBManager 로드
                this._currentFilePath = openFileDialog.FileName;
                this._dbManager.LoadProject(this._currentFilePath);
                this.projectMetadata = this._dbManager.projectMetadata;

                // 1. 노드 데이터 불러오기 및 뷰 모델에 추가
                var loadedNodes = _dbManager.GetAllNodes();
                // 새 ObservableCollection으로 재할당하여 기존 데이터 모두 교체
                this.Nodes.Clear();
                double nodeWidth = NodeViewModel.Default_NodeWidth;
                double nodeHeight = NodeViewModel.Default_NodeHeight;

                foreach (NodeModel nodeModel in loadedNodes)
                {
                    NodeViewModel nodeViewModel = new NodeViewModel(nodeModel, this.SelectNode, this.NodeProcessTypes);

                    // 노드 크기 설정
                    nodeViewModel.Width = nodeWidth;
                    nodeViewModel.Height = nodeHeight;
                    var processType = this.NodeProcessTypes.FirstOrDefault(t => t.ID == nodeModel.ID_TYPE);
                    if (processType != null)
                    {
                        nodeModel.ProcessType = processType;

                        // 💡 노드 헤더 색상도 NodeProcessType의 COLOR_R,G,B 값으로 설정합니다.
                        nodeModel.NodeColor = Color.FromRgb(
                            (byte)processType.COLOR_R,
                            (byte)processType.COLOR_G,
                            (byte)processType.COLOR_B
                        );
                    }
                    // 전체 목록에 노드 추가
                    this.Nodes.Add(nodeViewModel);
                }

                // 2. 링크 데이터 불러오기 및 뷰 모델에 추가
                var loadedLinks = _dbManager.GetAllLinks();
                this.Links.Clear();
                foreach (var linkModel in loadedLinks)
                {
                    // LinkModel의 ID를 사용하여 해당하는 NodeViewModel을 찾습니다.
                    var startNode = this.Nodes.FirstOrDefault(n => n.NodeData.ID_NODE == linkModel.ID_NODE_SRC);
                    var endNode = this.Nodes.FirstOrDefault(n => n.NodeData.ID_NODE == linkModel.ID_NODE_TGT);

                    LinkViewModel newLink = new LinkViewModel(startNode, endNode);
                    newLink.LinkData.ID_NODE_SRC = linkModel.ID_NODE_SRC;
                    newLink.LinkData.ID_NODE_TGT = linkModel.ID_NODE_TGT;

                    this.Links.Add(newLink);
                }

                // 3. 프로퍼티 데이터 불러오기 및 뷰 모델에 추가
                var loadedProperties = _dbManager.GetAllProperties();

                Console.WriteLine($"[DBManager] 파일 불러오기 : {this._currentFilePath}");

                this.IsDirty = false;
                this.IsProjectOpen = true;
                UpdateWindowTitle();

                // 뷰 리셋
                ResetView(null);
            }
        }


        // 프로젝트 저장 로직
        private void SaveProject(object parameter)
        {
            if (string.IsNullOrEmpty(this._currentFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "NodeFlow 파일 (*.nf)|*.nf";
                saveFileDialog.DefaultExt = "nf";

                if (saveFileDialog.ShowDialog() == true)
                {
                    this._currentFilePath = saveFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            try
            {
                this.projectMetadata.ProjectName = Path.GetFileNameWithoutExtension(this._currentFilePath);


                this._dbManager.SaveProject(this._currentFilePath, this.Nodes, this.Links, this.Properties);

                UpdateWindowTitle();
                MessageBox.Show("프로젝트가 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                this.IsDirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"프로젝트 저장에 실패했습니다: {ex.Message}", "저장 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 애플리케이션 종료 로직
        private void ExitApplication(object parameter)
        {
            if (IsDirty)
            {
                MessageBoxResult result = MessageBox.Show(
                    "저장되지 않은 변경사항이 있습니다. 종료하시겠습니까?",
                    "종료 확인",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    SaveProject(null);
                    _dbManager.CleanupTempFiles();
                    Application.Current.Shutdown();
                }
                else if (result == MessageBoxResult.No)
                {
                    _dbManager.CleanupTempFiles();
                    Application.Current.Shutdown();
                }
            }
            else
            {
                _dbManager.CleanupTempFiles();
                Application.Current.Shutdown();
            }
        }

        // 노드 추가
        private void AddNodeAtPosition(object parameter)
        {
            if (parameter is Point position)
            {
                // AddNode 창을 띄울 때 MainWindowViewModel의 NodeProcessTypes를 전달
                WIndowAddNode addNodeView = new WIndowAddNode(this.NodeProcessTypes);

                if (addNodeView.ShowDialog() == true)
                {
                    // 노드의 너비와 높이를 미리 정의하거나 NodeViewModel에서 가져옵니다.
                    double nodeWidth = NodeViewModel.Default_NodeWidth;
                    double nodeHeight = NodeViewModel.Default_NodeHeight;

                    // 💡 마우스 커서의 좌표를 기준으로 노드 중심을 계산합니다.
                    double centeredX = position.X - (nodeWidth / 2);
                    double centeredY = position.Y - (nodeHeight / 2);

                    // 💡 WIndowAddNode에서 반환된 객체를 이용해 새 NodeModel을 생성합니다.
                    //    ProcessType 객체가 이미 선택되어 있으므로 그대로 사용합니다.

                    var newNodeModel = new NodeModel
                    {
                        NODE_TITLE = addNodeView.AddedNode.NODE_TITLE,
                        ProcessType = addNodeView.AddedNode.ProcessType,
                        ID_TYPE = addNodeView.AddedNode.ID_TYPE,
                        Assignee = addNodeView.AddedNode.Assignee,
                        DATE_START = addNodeView.AddedNode.DATE_START,
                        DATE_END = addNodeView.AddedNode.DATE_END,
                        ID_NODE = addNodeView.AddedNode.ID_NODE,
                        NodeColor = addNodeView.AddedNode.NodeColor,
                        XPosition = centeredX, // 💡 중앙에 위치한 X 좌표
                        YPosition = centeredY, // 💡 중앙에 위치한 Y 좌표
                        Width = nodeWidth,
                        Height = nodeHeight
                    };

                    // 노드 번호 부여 및 DB에 추가
                    newNodeModel.ID_NODE = _dbManager.AddNode(newNodeModel);

                    // 💡 올바르게 초기화된 NodeModel을 NodeViewModel 생성자에 전달합니다.
                    var newNodeViewModel = new NodeViewModel(newNodeModel, this.SelectNode, this.NodeProcessTypes);

                    Nodes.Add(newNodeViewModel);
                    IsDirty = true;
                }
            }
        }

        // 노드 삭제 로직
        private void RemoveNode(object parameter)
        {
            if (SelectedNode != null)
            {
                int nodeid = SelectedNode.NodeData.ID_NODE;

                // 노드와 연결된 모든 링크를 찾아서 삭제
                // 💡 LINQ를 사용하여 삭제할 링크를 필터링합니다.
                var linksToRemove = _links.Where(l => l.StartNode.NodeData.ID_NODE == nodeid || l.EndNode.NodeData.ID_NODE == nodeid).ToList();

                foreach (var link in linksToRemove)
                {
                    Links.Remove(link);
                }
                // 링크 삭제
                _dbManager.DeleteLink(nodeid);

                // 노드 삭제
                _dbManager.DeleteNode(nodeid);
                Nodes.Remove(SelectedNode);

                SelectedNode = null;
                this.IsDirty = true;
            }
        }

        private bool CanRemoveNode(object parameter)
        {
            return SelectedNode != null;
        }

        private void ManageNodes(object parameter) { /* 노드 목록 관리 로직 */ }
        private void AdjustCanvasSize(object parameter) { /* 캔버스 크기 조절 로직 */ }
        private void ShowProperties(object parameter) { /* 속성 창 표시 로직 */ }
        private void ShowHelp(object parameter) { /* 도움말 표시 로직 */ }
        private void DeleteProperties(object parameter) { /* 속성 삭제 로직 */ }
        // 시점 초기화
        private void ResetView(object parameter) 
        {
            Scale = 1.0;
            OffsetX = 0;
            OffsetY = 0;
        }

        // 선택한 노드의 모든 링크 삭제
        private void DeleteLinksForSelectedNode(object parameter)
        {
            if (SelectedNode != null)
            {
                int nodeId = SelectedNode.NodeData.ID_NODE;

                // 1. 뷰모델의 Links 컬렉션에서 삭제할 링크들을 먼저 찾습니다.
                var linksToRemove = Links.Where(l =>
                    l.StartNode.NodeData.ID_NODE == nodeId || l.EndNode.NodeData.ID_NODE == nodeId
                ).ToList();

                // 2. 데이터베이스에서 해당 노드와 관련된 모든 링크를 삭제합니다.
                _dbManager.DeleteLink(nodeId);

                // 3. 뷰모델의 ObservableCollection에서 해당 링크들을 제거합니다.
                // 이 과정이 UI 갱신을 트리거합니다.
                foreach (var link in linksToRemove)
                {
                    Links.Remove(link);
                }

                this.IsDirty = true;
            }
        }

        private bool CanProjectExist(object parameter)
        {
            return IsProjectOpen;
        }

        private void UpdateWindowTitle()
        {
            string projectName = projectMetadata.ProjectName;
            Console.WriteLine($"프로젝트 이름 : {projectName}");

            if (!IsProjectOpen)
            {
                this.WindowTitle = "NodeFlow";
                return;
            }

            if (!string.IsNullOrEmpty(this._currentFilePath))
            {
                this.WindowTitle = $"NodeFlow - {projectName}";
            }
            else
            {
                this.WindowTitle = $"NodeFlow - {projectName}*";
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 노드 클릭 시 호출되어 SelectedNode 속성을 업데이트하는 메서드
        private void SelectNode(NodeViewModel node)
        {
            this.SelectedNode = node;
            IsNodeSelected = node == null ? false : true;
        }
    }
}