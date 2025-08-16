using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Models;

namespace Client.ViewModels
{
    public class LinkViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 핵심 데이터인 LinkModel을 속성으로 가짐
        public LinkModel LinkData { get; private set; }

        public LinkViewModel(LinkModel model)
        {
            this.LinkData = model;
        }

        // 링크의 UI 관련 속성 (예: 시작점, 끝점)이 필요하다면 여기에 추가
        // private Point _startPoint;
        // public Point StartPoint
        // {
        //     get => _startPoint;
        //     set
        //     {
        //         _startPoint = value;
        //         OnPropertyChanged(nameof(StartPoint));
        //     }
        // }
    }
}
