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

namespace Client.Views
{
    /// <summary>
    /// WIndowAddNode.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WIndowAddNode : Window
    {
        public WIndowAddNode()
        {
            InitializeComponent();

            // 콤보박스 항목을 코드에서 동적으로 추가
            StatusComboBox.Items.Add("계획");
            StatusComboBox.Items.Add("진행중");
            StatusComboBox.Items.Add("완료");
            StatusComboBox.Items.Add("보류");
            StatusComboBox.Items.Add("진행불가");
            StatusComboBox.Items.Add("실패");
            StatusComboBox.SelectedIndex = 0; // 초기값 설정
        }
    }
}
