using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Markup;
using FreePIE.GUI.CodeCompletion.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace FreePIE.GUI.CodeCompletion
{
    public class EditorAdapterConverter : MarkupExtension, IValueConverter
    {
        public Dictionary<Type, Func<object, EditorAdapterBase>> factories; 

        public EditorAdapterConverter()
        {
            factories = new Dictionary<Type, Func<object, EditorAdapterBase>>()
                            {
                                { typeof(TextArea), val => new AvalonEditorAdapter((TextArea)val) }
                            };
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
                return null;

            Type valType = value.GetType();

            if(!factories.ContainsKey(valType))
                throw new NotSupportedException("Conversion from " + valType + " to EditorAdapterBase is currently not supported.");

            return factories[valType](value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
