using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Data;

namespace FreePIE.GUI.CodeCompletion
{

    public static class Helper
    {
        public static T Get<T>(this IEnumerable<object> values, int index = 0)
        {
            return values.Where(x => x is T).Cast<T>().Skip(index).Take(1).Single();
        }
    }

    public class PopupOpenConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 3 || !values.All(val => val is bool))
                return false;

            //Window isn't active.
            if (!values.Get<bool>())
                return false;

            //Completion items are empty.
            if (!values.Get<bool>(1))
                return false;

            return values.Get<bool>(2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
