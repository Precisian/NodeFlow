using Client.Models;
using Client.Views;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Media;
using System.Linq;

namespace Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private DBManager _dbManager;
        private string _currentFilePath;
        public ProjectMetadata projectMetadata { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        // MVVM 패턴에 맞게 ObservableCollection<NodeViewModel> 사용
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

        // 현재 선택된 노드를 NodeViewModel로 변경
        private NodeViewModel _selectedNode;
        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    _selectedNode = value;
                    OnPropertyChanged(nameof(SelectedNode));
                    ((RelayCommand)RemoveNodeCommand).RaiseCanExecuteChanged();
                }
            }
        }
        // 링크들의 목록을 저장하는 ObservableCollection
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

        // 속성들의 목록을 저장하는 ObservableCollection
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
                    ((RelayCommand)AddNodeCommand).RaiseCanExecuteChanged();
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
        public ICommand AddNodeCommand { get; }
        public ICommand RemoveNodeCommand { get; }
        public ICommand ManageNodesCommand { get; }
        public ICommand AdjustCanvasSizeCommand { get; }
        public ICommand ShowPropertiesCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand DeletePropertiesCommand { get; }
        public ICommand ResetViewCommand { get; }

        public MainWindowViewModel()
        {
            _dbManager = new DBManager();
            this.projectMetadata = _dbManager.projectMetadata;

            this.Nodes = new ObservableCollection<NodeViewModel>();
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
            SaveProjectCommand = new RelayCommand(SaveProject);
            ExitApplicationCommand = new RelayCommand(ExitApplication);
            AddNodeCommand = new RelayCommand(AddNode, CanAddNode);
            RemoveNodeCommand = new RelayCommand(RemoveNode, CanRemoveNode);
            ManageNodesCommand = new RelayCommand(ManageNodes);
            AdjustCanvasSizeCommand = new RelayCommand(AdjustCanvasSize);
            ShowPropertiesCommand = new RelayCommand(ShowProperties);
            ShowHelpCommand = new RelayCommand(ShowHelp);
            DeletePropertiesCommand = new RelayCommand(DeleteProperties);
            ResetViewCommand = new RelayCommand(ResetView);

            IsProjectOpen = false;
            UpdateWindowTitle();
        }

        // 새 프로젝트 생성 로직
        private void NewProject(object parameter)
        {
            this._dbManager.CreateNewProject();
            this.Nodes.Clear(); // 컬렉션 초기화
            this.IsProjectOpen = true;
            this._currentFilePath = null;
            this.projectMetadata.ProjectName = "[제목 없음]";
            Console.WriteLine("[DBManager] 새 프로젝트 생성");
            UpdateWindowTitle();
        }

        // 프로젝트 불러오기 로직
        private void LoadProject(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "NodeFlow 파일(*.nf)|*.nf";

            if (openFileDialog.ShowDialog() == true)
            {
                this._currentFilePath = openFileDialog.FileName;
                this._dbManager.LoadProject(this._currentFilePath);

                // DB에서 NodeModel 리스트를 불러와 NodeViewModel 컬렉션으로 변환
                var loadedNodes = _dbManager.GetAllNodes();
                this.Nodes.Clear();
                foreach (var nodeModel in loadedNodes)
                {
                    this.Nodes.Add(new NodeViewModel(nodeModel));
                }

                Console.WriteLine($"[DBManager] 파일 불러오기 : {this._currentFilePath}");
                this.IsProjectOpen = true;
                UpdateWindowTitle();
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

                // DBManager에 데이터 저장 요청
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

        // 노드 추가 로직
        private void AddNode(object parameter)
        {
            WIndowAddNode addNodeView = new WIndowAddNode(this.NodeProcessTypes);

            if (addNodeView.ShowDialog() == true)
            {
                var newNodeModel = new NodeModel
                {
                    NODE_TITLE = addNodeView.AddedNode.NODE_TITLE,
                    ProcessType = addNodeView.AddedNode.ProcessType,
                    ID_TYPE =  addNodeView.AddedNode.ID_TYPE,
                    ASSIGNEE = addNodeView.AddedNode.ASSIGNEE,
                    DATE_START = addNodeView.AddedNode.DATE_START,
                    DATE_END = addNodeView.AddedNode.DATE_END,
                    ID_NODE = addNodeView.AddedNode.ID_NODE,
                    NodeColor = addNodeView.AddedNode.NodeColor
                    // 다른 속성들도 설정...
                };

                // DB에 먼저 노드 저장 (ID를 받기 위함)
                _dbManager.AddNode(newNodeModel);

                // NodeModel을 기반으로 NodeViewModel 생성 후 컬렉션에 추가
                var newNodeViewModel = new NodeViewModel(newNodeModel);
                Nodes.Add(newNodeViewModel);

                // 새로 추가된 노드를 자동으로 선택
                SelectedNode = newNodeViewModel;
                IsDirty = true;
            }
        }

        // 노드 제거 로직
        private void RemoveNode(object parameter)
        {
            if (SelectedNode != null)
            {
                // DB에서 노드 삭제
                _dbManager.DeleteNode(SelectedNode.NodeData.ID_NODE);

                // ViewModel 컬렉션에서 노드 제거
                Nodes.Remove(SelectedNode);
                SelectedNode = null;
                this.IsDirty = true;
            }
        }

        private bool CanRemoveNode(object parameter)
        {
            return SelectedNode != null;
        }

        // 나머지 메서드들은 그대로 유지
        private void ManageNodes(object parameter) { /* 노드 목록 관리 로직 */ }
        private void AdjustCanvasSize(object parameter) { /* 캔버스 크기 조절 로직 */ }
        private void ShowProperties(object parameter) { /* 속성 창 표시 로직 */ }
        private void ShowHelp(object parameter) { /* 도움말 표시 로직 */ }
        private void DeleteProperties(object parameter) { /* 속성 삭제 로직 */ }
        private void ResetView(object parameter) { /* 시점 초기화 */ }

        private bool CanAddNode(object parameter)
        {
            return IsProjectOpen;
        }

        private void UpdateWindowTitle()
        {
            //string projectName = string.IsNullOrEmpty(projectMetadata.ProjectName) ? "[제목 없음]" : projectMetadata.ProjectName;
            string projectName = projectMetadata.ProjectName;
            Console.WriteLine($"프로젝트 이름 : {projectName}");

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
    }
}