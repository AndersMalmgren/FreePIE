using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;

namespace FreePIE.GUI.CodeCompletion.Controls
{
    /// <summary>
    /// Interaction logic for EvenListBox.xaml
    /// </summary>
    public partial class EvenListBox : ListBox
    {

        public EvenListBox()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
                          {
                              this.itemHeight = CalculateItemHeight();
                              UpdateHeight();
                          };
        }

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
