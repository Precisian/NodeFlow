// WIndowAddNode.xaml.cs
using Client.ViewModels;
using Client.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Client.Views
{
    public partial class WIndowAddNode : Window
    {
        private AddNodeViewModel viewModel;

        // 추가된 노드 데이터를 외부에 노출할 속성
        public NodeModel AddedNode { get; private set; }

        public WIndowAddNode(ObservableCollection<NodeProcessType> listCombo)
        {
            InitializeComponent();

            // 뷰모델 인스턴스 생성 및 DataContext 설정
            viewModel = new AddNodeViewModel(listCombo);
            this.DataContext = viewModel;

            // 뷰모델의 이벤트 구독
            viewModel.RequestClose += ViewModel_RequestClose;
        }

        private void ViewModel_RequestClose()
        {
            // 뷰모델로부터 전달받은 NewNode 데이터를 AddedNode에 저장
            this.AddedNode = viewModel.NewNode;
            // 창을 닫습니다.
            this.DialogResult = true;
            this.Close();
        }

        // (선택 사항) 취소 버튼을 위한 로직
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}