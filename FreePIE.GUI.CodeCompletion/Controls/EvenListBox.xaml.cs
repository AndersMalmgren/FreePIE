using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Controls
{
    public class EventArgs<T, K> : EventArgs
    {
        public T Arg1 { get; private set; }
        public K Arg2 { get; private set; }

        public EventArgs(T arg1, K arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }

    /// <summary>
    /// Interaction logic for EvenListBox.xaml
    /// </summary>
    public partial class EvenListBox
    {

        public EvenListBox()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
                          {
                              this.itemHeight = CalculateItemHeight();
                              UpdateHeight();
                          };
           PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as FrameworkElement;

            if (!(source is TextBlock) || source == this || this == source.TemplatedParent)
                return;
            
            OnItemClicked(source.DataContext, e);
        }

        private void OnItemClicked(object dataContext, MouseButtonEventArgs e)
        {
            if (ItemClicked != null)
                ItemClicked(this, new EventArgs<object, MouseButtonEventArgs>(dataContext, e));
        }

        public event EventHandler<EventArgs<object, MouseButtonEventArgs>> ItemClicked; 

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            UpdateHeight();
            base.OnItemsChanged(e);
        }

        private void UpdateHeight()
        {
            this.Height = CalculateHeight();
        }

        private double itemHeight;

        public static readonly DependencyProperty DisplayedRowsProperty =
            DependencyProperty.Register("DisplayedRows", typeof (int), typeof (EvenListBox), new PropertyMetadata(default(int)));

        public int DisplayedRows
        {
            get { return (int) GetValue(DisplayedRowsProperty); }
            set { SetValue(DisplayedRowsProperty, value); }
        }

        private double CalculateItemHeight()
        {
            if (Items.Count <= 0)
                return double.NaN;

            var container = ItemContainerGenerator.ContainerFromIndex(0) as Control;

            if (container == null)
                return double.NaN;

            container.Measure(new Size(200, 200));

            double itemHeight;

            itemHeight = Math.Abs(container.ActualHeight) < 0.01 ? container.DesiredSize.Height : container.ActualHeight;
            itemHeight = itemHeight + container.Margin.Bottom + container.Margin.Top + container.Padding.Bottom + container.Padding.Top;

            return itemHeight;
        }

        private double CalculateHeight()
        {
            if (Items.Count <= DisplayedRows)
                return double.NaN;

            return itemHeight * DisplayedRows + 5;
        }

    }
}
