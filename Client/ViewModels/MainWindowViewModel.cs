using Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 노드들의 목록을 저장하는 ObservableCollection
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

        // 현재 선택된 노드를 나타내는 속성
        private NodeViewModel _selectedNode;
        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                OnPropertyChanged(nameof(SelectedNode));
            }
        }

        // 추가 속성 편집을 위한 콤보박스 아이템 목록입니다.
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

        // ICommand 인터페이스를 구현하는 명령들입니다.
        // XAML의 메뉴 아이템에 바인딩됩니다.
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
            // ObservableCollection 초기화
            Nodes = new ObservableCollection<NodeViewModel>();
            ComboBoxItems = new ObservableCollection<string> { "Text", "Number", "Date", "Assignee" };

            // 명령 초기화
            // 실제 로직은 각 메서드에 구현해야 합니다.
            NewProjectCommand = new RelayCommand(NewProject);
            LoadProjectCommand = new RelayCommand(LoadProject);
            SaveProjectCommand = new RelayCommand(SaveProject);
            ExitApplicationCommand = new RelayCommand(ExitApplication);
            AddNodeCommand = new RelayCommand(AddNode);
            RemoveNodeCommand = new RelayCommand(RemoveNode, CanRemoveNode);
            ManageNodesCommand = new RelayCommand(ManageNodes);
            AdjustCanvasSizeCommand = new RelayCommand(AdjustCanvasSize);
            ShowPropertiesCommand = new RelayCommand(ShowProperties);
            ShowHelpCommand = new RelayCommand(ShowHelp);
            DeletePropertiesCommand = new RelayCommand(DeleteProperties);
            ResetViewCommand = new RelayCommand(ResetView);

            // 테스트용 초기 노드 추가
            //Nodes.Add(new NodeViewModel(new NodeModel
            //{
            //    NodeTitle = "초기 노드",
            //    TaskName = "테스트 작업",
            //    StartDate = "2024-01-01",
            //    EndDate = "2024-01-05",
            //    Assignee = "김철수"
            //})
            //{ XPosition = 50, YPosition = 50 });
        }

        // 아래는 각 명령에 대한 실제 로직을 구현하는 메서드입니다.
        // 현재는 예시를 위해 비워두거나 간단한 로직을 추가했습니다.
        private void NewProject(object parameter) { /* 새 프로젝트 로직 */ }
        private void LoadProject(object parameter) { /* 프로젝트 불러오기 로직 */ }
        private void SaveProject(object parameter) { /* 프로젝트 저장 로직 */ }
        private void ExitApplication(object parameter) { /* 종료 로직 */ }

        private void AddNode(object parameter)
        {
            var newNode = new NodeViewModel(new NodeModel
            {
                NodeTitle = "새 노드",
                TaskName = "새로운 작업",
                StartDate = "2024-01-01",
                EndDate = "2024-01-05",
                Assignee = "새 담당자"
            });
            Nodes.Add(newNode);
            SelectedNode = newNode;
        }

        private void RemoveNode(object parameter)
        {
            if (SelectedNode != null)
            {
                Nodes.Remove(SelectedNode);
                SelectedNode = null;
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

        private void ResetView(object parameter) { /* 시점 초기화 */ }
    }
}