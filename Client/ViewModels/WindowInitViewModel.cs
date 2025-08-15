using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
using System.Windows;

namespace Client.ViewModels
{
    public class WindowInitViewModel
    {
        // 뷰 전환을 알리는 이벤트 정의
        public event EventHandler RequestViewChange;

        public WindowInitViewModel()
        {
            // 예시: 3초 후 뷰 전환을 요청
            Task.Delay(3000).ContinueWith(_ =>
            {
                // UI 스레드에서 이벤트 발생
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 이벤트 발생
                    RequestViewChange?.Invoke(this, EventArgs.Empty);
                });
            });
        }
    }
}