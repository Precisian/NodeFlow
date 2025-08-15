using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Client.ViewModels;

namespace Client.Views
{
    public partial class WindowInit : Window
    {
        private readonly WindowInitViewModel _viewModel;

        public WindowInit()
        {
            InitializeComponent();

            _viewModel = new WindowInitViewModel();
            this.DataContext = _viewModel;

            // 뷰모델의 이벤트를 구독
            _viewModel.RequestViewChange += OnRequestViewChange;
        }

        // 뷰 전환 이벤트 핸들러
        private void OnRequestViewChange(object sender, EventArgs e)
        {
            // Main Window의 인스턴스를 생성
            MainWindow mainWindow = new MainWindow();

            // 새로운 윈도우를 보여줌
            mainWindow.Show();

            // 현재 윈도우를 닫음
            this.Close();

            // 구독 해제 (메모리 누수 방지)
            _viewModel.RequestViewChange -= OnRequestViewChange;
        }
    }
}
