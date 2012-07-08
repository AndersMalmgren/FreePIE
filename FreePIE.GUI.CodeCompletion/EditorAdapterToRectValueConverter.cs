using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace FreePIE.GUI.CodeCompletion
{
    public class EditorAdapterToRectValueConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        private IEnumerable<Func<IEnumerable<object>, bool>> ValidValuesFilters
        {
            get
            {
                return new List<Func<IEnumerable<object>, bool>>()
                           {
                               values => values.Any(obj => obj is bool),
                               values => values.Any(obj => obj is Rect),
                               values => values.Any(obj => obj is EditorAdapterBase)
                           };
            }
        }

        private struct Values
        {
            public bool IsOpen { get; set; }
            public Rect CurrentPlacementRectangle { get; set; }
            public EditorAdapterBase Target { get; set; }
        }

        private Values GetTypedValues(IEnumerable<object> values)
        {
            return new Values()
                       {
                           IsOpen = (bool)values.Single(x => x is bool),
                           Target = values.Single(x => x is EditorAdapterBase) as EditorAdapterBase,
                           CurrentPlacementRectangle = (Rect)values.Single(x => x is Rect)
                       };
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            
            if(values.Length != 3)
                throw new InvalidOperationException("There must be three (3) values present: IsOpen, current PlacementRectangle and target EditorAdapter.");

            if (!ValidValuesFilters.All(filter => filter(values)))
                return new Rect();

            Values v = GetTypedValues(values);

            if (v.IsOpen)
                return v.CurrentPlacementRectangle;

            Rect rect = v.Target.GetVisualPosition();
            return rect.IsEmpty ? default(Rect) : new Rect(rect.X, rect.Y + 1, rect.Width, rect.Height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
