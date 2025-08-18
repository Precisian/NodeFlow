using Client.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.ViewModels
{
    // INotifyPropertyChanged 구현
    public class AddNodeViewModel : INotifyPropertyChanged
    {
        // 추가 버튼에 바인딩할 커맨드
        public ICommand AddNodeCommand { get; }

        // 취소 버튼에 바인딩할 커맨드
        public ICommand CancelCommand { get; }
        // 뷰에 창 닫기를 요청하는 이벤트
        public event Action RequestClose;


        // 노드 추가 버튼을 누를 때 뷰에서 입력된 값을 전달받을 속성
        private NodeModel _newNode;
        public NodeModel NewNode
        {
            get => _newNode;
            set
            {
                if (_newNode != value)
                {
                    _newNode = value;
                    OnPropertyChanged(nameof(NewNode));
                }
            }
        }

        // 콤보박스에 바인딩될 아이템 소스
        private ObservableCollection<NodeProcessType> _processTypes;
        public ObservableCollection<NodeProcessType> ProcessTypes
        {
            get => _processTypes;
            set
            {
                _processTypes = value;
                OnPropertyChanged(nameof(ProcessTypes));
            }
        }

        // 콤보박스에서 선택된 아이템
        private NodeProcessType _selectedType;
        public NodeProcessType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged(nameof(SelectedType));

                    // 💡 해결 방법: 이 지점에서 NodeModel의 정보를 즉시 업데이트합니다.
                    if (NewNode != null && value != null)
                    {
                        NewNode.ID_TYPE = value.ID;
                        NewNode.NodeColor = Color.FromRgb(
                            (byte)value.COLOR_R,
                            (byte)value.COLOR_G,
                            (byte)value.COLOR_B
                        );
                    }

                    // 기존 UpdateNodeColor() 메서드 호출은 유지하여 SelectedNodeColor를 업데이트
                    UpdateNodeColor();
                }
            }
        }

        // Rectangle에 바인딩될 색상
        private SolidColorBrush _selectedNodeColor;
        public SolidColorBrush SelectedNodeColor
        {
            get => _selectedNodeColor;
            set
            {
                if (_selectedNodeColor != value)
                {
                    _selectedNodeColor = value;
                    OnPropertyChanged(nameof(SelectedNodeColor));
                }
            }
        }

        // 선택된 타입의 색상을 업데이트하는 메서드
        private void UpdateNodeColor()
        {
            if (SelectedType != null)
            {
                // 선택된 타입의 RGB 값으로 SolidColorBrush 생성
                this.SelectedNodeColor = new SolidColorBrush(Color.FromRgb(
                    (byte)SelectedType.COLOR_R,
                    (byte)SelectedType.COLOR_G,
                    (byte)SelectedType.COLOR_B
                ));
            }
            else
            {
                // 선택된 항목이 없으면 기본 색상 설정
                this.SelectedNodeColor = new SolidColorBrush(Colors.Transparent);
            }
        }

        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // NewNode 속성 변경 시 호출될 메서드
        private void NewNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // NODE_TITLE 또는 ASSIGNEE가 변경되었을 때만 CommandManager에 재평가 요청
            if (e.PropertyName == nameof(NewNode.NODE_TITLE) || e.PropertyName == nameof(NewNode.Assignee))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public AddNodeViewModel(ObservableCollection<NodeProcessType> listCombo)
        {
            this.ProcessTypes = listCombo;
            if (this.ProcessTypes.Count > 0)
            {
                this.SelectedType = this.ProcessTypes[0];
            }

            // NewNode 객체 초기화
            NewNode = new NodeModel();

            this.NewNode.PropertyChanged += NewNode_PropertyChanged;

            // 커맨드 초기화 및 메서드 연결
            AddNodeCommand = new RelayCommand(OnAddNode);
        }
        

        // 추가 버튼 클릭 시 실행될 메서드
        private void OnAddNode(object parameter)
        {
            // 선택된 타입의 ID를 NewNode에 할당
            NewNode.ID_TYPE = SelectedType.ID;

            // 뷰모델을 호출한 쪽에 NewNode 데이터를 전달
            // 창 닫기 요청 이벤트 호출
            RequestClose?.Invoke();
        }
    }
}