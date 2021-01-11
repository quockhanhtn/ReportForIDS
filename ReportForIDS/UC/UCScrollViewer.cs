using System.Windows.Controls;

namespace ReportForIDS.UC
{
   public class UCScrollViewer : ScrollViewer
   {
      public UCScrollViewer() : base()
      {
         this.Loaded += UCScrollViewer_Loaded;

         HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
         VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      }

      private void UCScrollViewer_Loaded(object sender, System.Windows.RoutedEventArgs e)
      {
         this.PreviewMouseWheel += UCScrollViewer_PreviewMouseWheel;
      }

      private void UCScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
      {
         this.ScrollToVerticalOffset(this.VerticalOffset - e.Delta);
         e.Handled = true;
      }
   }
}