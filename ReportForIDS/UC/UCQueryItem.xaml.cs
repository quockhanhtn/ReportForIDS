using System.Windows.Controls;

namespace ReportForIDS.UC
{
   /// <summary>
   /// Interaction logic for UCQueryItem.xaml
   /// </summary>
   public partial class UCQueryItem : UserControl
   {
      public UCQueryItem()
      {
         InitializeComponent();
      }

      private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
      {
         e.Handled = !((System.Windows.Controls.ComboBox)sender).IsDropDownOpen;
      }
   }
}