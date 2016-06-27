using System.Windows;
using System.Windows.Controls;

namespace TwitchChat.Code.Helpers
{
    //  Helpers to add autoscrolling functionality to ScrollViewer control
    public static class ScrollViewerEx
    {
        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(false, AutoScrollPropertyChanged));

        private static void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ScrollViewer))
                return;

            var scrollViewer = (ScrollViewer) d;

            if ((bool) e.NewValue)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
            else
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!(sender is ScrollViewer))
                return;

            var scrollViewer = (ScrollViewer) sender;

            if (e.ExtentHeightChange > 0)
                scrollViewer.ScrollToBottom();
        }
    }
}
