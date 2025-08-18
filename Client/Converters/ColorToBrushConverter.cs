using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Client.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 입력 값이 Color 타입인지 확인
            if (value is Color color)
            {
                // Color 값을 SolidColorBrush로 변환하여 반환
                return new SolidColorBrush(color);
            }
            return Brushes.Transparent; // 변환 실패 시 투명 브러시 반환
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 필요에 따라 역변환 로직 구현 (여기서는 사용하지 않음)
            return DependencyProperty.UnsetValue;
        }
    }
}

