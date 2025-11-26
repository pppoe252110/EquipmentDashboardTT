using EquipmentDashboardTT.Models.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace EquipmentDashboardTT.Converters
{
    public class WrapperToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WrapperBase<object> wrapper)
            {
                return wrapper.DisplayName;
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
