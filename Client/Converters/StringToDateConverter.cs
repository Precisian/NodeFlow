using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class StringToDateConverter : IValueConverter
    {
        // View -> ViewModel (DatePicker의 SelectedDate -> PropertyItem.Value)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // DateTime 객체를 "yyyy-MM-dd" 형식의 문자열로 변환
                return dateTime.ToString("yyyy-MM-dd");
            }

            // 변환에 실패하면 null 반환
            return null;
        }

        // ViewModel -> View (PropertyItem.Value -> DatePicker의 SelectedDate)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString)
            {
                // 문자열을 DateTime 객체로 변환
                if (DateTime.TryParse(dateString, out DateTime result))
                {
                    return result;
                }
            }
            else if (value is DateTime dateTime)
            {
                // 이미 DateTime 객체일 경우 그대로 반환
                return dateTime;
            }

            // 변환 실패 시 null 반환
            return null;
        }
    }
}