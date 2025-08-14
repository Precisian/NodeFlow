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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Views
{
    /// <summary>
    /// Node.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Node : UserControl
    {
        // 노드 헤더의 색상을 바인딩하기 위한 의존성 속성
        public static readonly DependencyProperty NodeHeaderColorProperty =
            DependencyProperty.Register("NodeHeaderColor", typeof(Brush), typeof(Node), new PropertyMetadata(Brushes.LightGray));

        // 노드 제목을 바인딩하기 위한 의존성 속성
        public static readonly DependencyProperty NodeTitleProperty =
            DependencyProperty.Register("NodeTitle", typeof(string), typeof(Node), new PropertyMetadata("{작업명}"));

        // 작업명을 바인딩하기 위한 의존성 속성
        public static readonly DependencyProperty TaskNameProperty =
            DependencyProperty.Register("TaskName", typeof(string), typeof(Node), new PropertyMetadata(string.Empty));

        // 시작일을 바인딩하기 위한 의존성 속성
        public static readonly DependencyProperty StartDateProperty =
            DependencyProperty.Register("StartDate", typeof(string), typeof(Node), new PropertyMetadata(string.Empty));

        // 종료일을 바인딩하기 위한 의존성 속성
        public static readonly DependencyProperty EndDateProperty =
            DependencyProperty.Register("EndDate", typeof(string), typeof(Node), new PropertyMetadata(string.Empty));

        // 담당자를 바인딩하기 위한 의존성 속성
        public static readonly DependencyProperty AssigneeProperty =
            DependencyProperty.Register("Assignee", typeof(string), typeof(Node), new PropertyMetadata(string.Empty));

        public Brush NodeHeaderColor
        {
            get => (Brush)GetValue(NodeHeaderColorProperty);
            set => SetValue(NodeHeaderColorProperty, value);
        }

        public string NodeTitle
        {
            get => (string)GetValue(NodeTitleProperty);
            set => SetValue(NodeTitleProperty, value);
        }

        public string TaskName
        {
            get => (string)GetValue(TaskNameProperty);
            set => SetValue(TaskNameProperty, value);
        }

        public string StartDate
        {
            get => (string)GetValue(StartDateProperty);
            set => SetValue(StartDateProperty, value);
        }

        public string EndDate
        {
            get => (string)GetValue(EndDateProperty);
            set => SetValue(EndDateProperty, value);
        }

        public string Assignee
        {
            get => (string)GetValue(AssigneeProperty);
            set => SetValue(AssigneeProperty, value);
        }

        public Node()
        {
            InitializeComponent();
        }
    }
}
