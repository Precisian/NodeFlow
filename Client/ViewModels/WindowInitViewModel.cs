using Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
    public class WindowInitViewModel : INotifyPropertyChanged
    {
        // 뷰 전환을 알리는 이벤트 정의
        public event EventHandler RequestViewChange;

        // PropertyChanged 이벤트를 정의합니다.
        public event PropertyChangedEventHandler PropertyChanged;

        // Private 필드를 사용하여 값을 저장합니다.
        private string _str;

        // UI에 바인딩할 Public 속성입니다.
        public string str
        {
            get => _str;
            set
            {
                if (_str != value)
                {
                    _str = value;
                    // 값이 변경되었음을 UI에 알립니다.
                    OnPropertyChanged(nameof(str));
                }
            }
        }

        public WindowInitViewModel()
        {
            str = "Now Loading.";
            ChangeTextAsync(); // 비동기 메서드를 호출합니다.
        }

        // 비동기적으로 텍스트를 변경하는 메서드
        private async void ChangeTextAsync()
        {
            int cnt = 0;
            while (cnt < 3)
            {
                await Task.Delay(500);
                cnt++;
                // 속성 값을 변경하면 setter에서 OnPropertyChanged가 호출됩니다.
                str += ".";
            }

            // 뷰 전환 이벤트 호출
            // UI 스레드에서 Dispatcher를 통해 안전하게 호출합니다.
            Application.Current.Dispatcher.Invoke(() =>
            {
                RequestViewChange?.Invoke(this, EventArgs.Empty);
            });
        }

        // 속성 변경 이벤트를 발생시키는 메서드
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}