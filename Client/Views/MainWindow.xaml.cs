using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.ViewModels;
using Client.Models;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isAddingNodeMode = false;
        private MainWindowViewModel viewModel;

        // 캔버스 패닝 관련 상태 변수
        private Point _lastMousePosition;
        private bool _isPanning = false;

        // 💡 링크 그리기 관련 상태 변수 추가
        private bool _isDrawingLink = false;
        private Line _tempLinkLine = null;
        private NodeViewModel _startLinkNode = null;
        private Point _startLinkPosition; // 💡 시작 커넥터의 위치를 저장할 변수

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
        }

        private void AddNodeButton_Click(object sender, RoutedEventArgs e)
        {
            _isAddingNodeMode = !_isAddingNodeMode;
            if (_isAddingNodeMode)
            {
                // 노드 추가 모드 시작
                mainCanvas.Cursor = Cursors.Cross;
                AddNodeButton.Content = "취소";
                this.viewModel.SelectedNode = null;
            }
            else
            {
                // 노드 추가 모드 종료
                mainCanvas.Cursor = Cursors.Arrow;
                AddNodeButton.Content = "노드 추가";
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 이벤트가 발생한 원본 요소를 가져옵니다.
            var originalSource = e.OriginalSource as FrameworkElement;
            // 💡 노드나 커넥터를 찾기 위해 하나의 변수로 통일합니다.
            Views.Node nodeControl = IsAncestorOfType<Views.Node>(originalSource);

            // 💡 노드 추가 모드가 아닐 때 링크 드래그 시작 로직을 수정합니다.
            if (!_isAddingNodeMode)
            {
                // 💡 클릭된 요소가 Ellipse인지 먼저 확인하고, 이름으로 커넥터인지 판별합니다.
                var clickedConnector = originalSource as Ellipse;
                if (clickedConnector != null && (clickedConnector.Name == "OutputConnector" || clickedConnector.Name == "InputConnector"))
                {
                    // 커넥터가 클릭된 경우, 그 부모 요소 중 Node 컨트롤을 찾습니다.
                    if (nodeControl != null)
                    {
                        // 커넥터가 클릭되었고, 유효한 노드 내부에 있다면
                        _isDrawingLink = true;
                        _startLinkNode = nodeControl.DataContext as NodeViewModel;

                        // 💡 시작점 위치를 커넥터의 중심 좌표로 설정합니다.
                        _startLinkPosition = clickedConnector.TranslatePoint(new Point(clickedConnector.ActualWidth / 2, clickedConnector.ActualHeight / 2), mainCanvas);

                        // 임시 선을 생성하여 캔버스에 추가하고 드래그 준비를 합니다.
                        _tempLinkLine = new Line
                        {
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 2,
                            StrokeDashArray = new DoubleCollection { 2, 2 }
                        };
                        mainCanvas.Children.Add(_tempLinkLine);

                        // 마우스 위치에 따라 선의 시작점과 끝점을 설정합니다.
                        _tempLinkLine.X1 = _startLinkPosition.X;
                        _tempLinkLine.Y1 = _startLinkPosition.Y;
                        _tempLinkLine.X2 = e.GetPosition(mainCanvas).X;
                        _tempLinkLine.Y2 = e.GetPosition(mainCanvas).Y;

                        mainCanvas.CaptureMouse();
                        mainCanvas.Cursor = Cursors.Cross;

                        // 뷰모델의 선택된 노드를 해제하여 혼란을 방지합니다.
                        viewModel.SelectedNode = null;
                        return; // 이벤트를 처리했으므로 추가 처리를 중단합니다.
                    }
                }

                // 노드나 커넥터가 아닌 빈 공간을 클릭했을 때
                // 💡 기존의 nodeControl 변수를 재활용합니다.
                if (nodeControl == null)
                {
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.SelectedNode = null;
                    }
                }
            }
            else
            {
                // 노드 추가 모드일 때 기존 로직 유지
                Point clickPosition = e.GetPosition(mainCanvas);
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.AddNodeAtPositionCommand.Execute(clickPosition);
                }

                _isAddingNodeMode = false;
                mainCanvas.Cursor = Cursors.Arrow;
                AddNodeButton.Content = "노드 추가";
            }
        }

        // 💡 마우스 왼쪽 버튼 놓았을 때 이벤트 핸들러 추가
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingLink)
            {
                mainCanvas.ReleaseMouseCapture();
                mainCanvas.Cursor = Cursors.Arrow;

                // 💡 마우스가 놓인 위치의 요소를 직접 찾습니다.
                Point currentPosition = e.GetPosition(mainCanvas);
                var hitTestResult = VisualTreeHelper.HitTest(mainCanvas, currentPosition);

                Views.Node endNodeControl = null;
                Ellipse endConnector = null;

                if (hitTestResult != null)
                {
                    // 마우스가 놓인 위치의 시각적 요소를 찾습니다.
                    FrameworkElement hitElement = hitTestResult.VisualHit as FrameworkElement;

                    // 해당 요소가 Ellipse인지 확인합니다.
                    endConnector = hitElement as Ellipse;
                    if (endConnector != null && (endConnector.Name == "OutputConnector" || endConnector.Name == "InputConnector"))
                    {
                        // 커넥터가 맞다면 그 부모인 노드 컨트롤을 찾습니다.
                        endNodeControl = IsAncestorOfType<Views.Node>(endConnector);
                    }
                }

                if (endNodeControl != null && endNodeControl.DataContext != _startLinkNode)
                {
                    // 다른 노드 위에서 마우스를 놓았다면, 뷰모델의 ConnectNodesCommand를 실행합니다.
                    NodeViewModel endNodeViewModel = endNodeControl.DataContext as NodeViewModel;
                    if (endNodeViewModel != null)
                    {
                        // 뷰모델의 커맨드를 두 번 호출하여 링크를 완성합니다.
                        // 첫 번째 호출은 시작 노드를, 두 번째 호출은 끝 노드를 지정합니다.
                        viewModel.ConnectNodesCommand.Execute(_startLinkNode);
                        viewModel.ConnectNodesCommand.Execute(endNodeViewModel);
                    }

                    // 💡 임시 선을 제거하지 않고, 영구적인 링크로 변환합니다.
                    Line permanentLink = new Line
                    {
                        X1 = _tempLinkLine.X1,
                        Y1 = _tempLinkLine.Y1,
                        X2 = currentPosition.X, // 끝점은 현재 마우스 위치로
                        Y2 = currentPosition.Y,
                        Stroke = Brushes.Black, // 영구적인 선 색상을 검은색으로 변경
                        StrokeThickness = 2
                    };
                    mainCanvas.Children.Remove(_tempLinkLine);
                    mainCanvas.Children.Add(permanentLink);
                }
                else
                {
                    // 💡 유효한 노드에 연결하지 못했다면 임시 선을 제거합니다.
                    if (_tempLinkLine != null)
                    {
                        mainCanvas.Children.Remove(_tempLinkLine);
                    }
                }

                // 상태 초기화
                _isDrawingLink = false;
                _startLinkNode = null;
                _tempLinkLine = null;
            }
        }

        /// <summary>
        /// 주어진 DependencyObject의 조상 중에서 특정 타입의 요소를 찾습니다.
        /// </summary>
        /// <typeparam name="T">찾으려는 조상 요소의 타입</typeparam>
        /// <param name="element">시작 요소</param>
        /// <returns>찾은 조상 요소, 없으면 null</returns>
        private T IsAncestorOfType<T>(DependencyObject element) where T : DependencyObject
        {
            while (element != null && !(element is T))
            {
                element = VisualTreeHelper.GetParent(element);
            }
            return element as T;
        }

        // 캔버스 마우스 왼쪽 버튼 누름 (패닝 시작)
        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 컨트롤 위에서 클릭하면 이벤트 처리 중단
            if (e.OriginalSource is UIElement && e.OriginalSource != mainCanvas)
            {
                return;
            }

            _isPanning = true;
            // 캔버스 자체의 좌표를 가져옵니다.
            _lastMousePosition = e.GetPosition(mainCanvas);
            mainCanvas.CaptureMouse();
        }

        // 캔버스 마우스 왼쪽 버튼 놓음 (패닝 종료)
        private void MainCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            mainCanvas.ReleaseMouseCapture();
        }

        // 캔버스 마우스 이동 (패닝 중)
        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 💡 링크 드래그 로직 추가
            if (_isDrawingLink)
            {
                Point currentPosition = e.GetPosition(mainCanvas);
                if (_tempLinkLine != null)
                {
                    // 임시 선의 끝점을 마우스 위치에 따라 업데이트합니다.
                    _tempLinkLine.X2 = currentPosition.X;
                    _tempLinkLine.Y2 = currentPosition.Y;
                }
                return; // 링크 드래그 중에는 패닝 로직을 건너뜁니다.
            }

            // 기존 패닝 로직 유지
            if (_isPanning)
            {
                var viewModel = DataContext as MainWindowViewModel;
                if (viewModel == null) return;

                Point currentPosition = e.GetPosition(mainCanvas);

                // 이동 거리를 계산하여 Offset에 적용
                viewModel.OffsetX += currentPosition.X - _lastMousePosition.X;
                viewModel.OffsetY += currentPosition.Y - _lastMousePosition.Y;

                _lastMousePosition = currentPosition;
            }
        }

        // 캔버스 마우스 휠 (확대/축소)
        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null) return;

            // 캔버스 좌표를 뷰포트 좌표로 변환
            Point canvasPosition = e.GetPosition(mainCanvas);

            double zoomFactor = 1.1; // 확대/축소 배율

            double oldScale = viewModel.Scale;

            if (e.Delta > 0)
            {
                viewModel.Scale *= zoomFactor;
            }
            else
            {
                viewModel.Scale /= zoomFactor;
            }

            // 확대/축소 후 마우스 위치가 고정되도록 Offset 조정
            double newScale = viewModel.Scale;
            viewModel.OffsetX = canvasPosition.X - ((canvasPosition.X - viewModel.OffsetX) / oldScale) * newScale;
            viewModel.OffsetY = canvasPosition.Y - ((canvasPosition.Y - viewModel.OffsetY) / oldScale) * newScale;

            e.Handled = true;
        }
    }
}
