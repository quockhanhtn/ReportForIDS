using ReportForIDS.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ReportForIDS.UC
{
   /// <summary>
   /// Interaction logic for UCSelectField.xaml
   /// </summary>
   public partial class UCSelectField : UserControl
   {
      public UCSelectField()
      {
         InitializeComponent();
      }

      private void OnChecked(object sender, RoutedEventArgs e)
      {
      }

      private void ToggleButton_Click(object sender, RoutedEventArgs e)
      {
         var dt = (this.DataContext as UCSelectFieldViewModel);
         if (dt != null)
         {
            dt.SelectedFieldChangedCommand.Execute(null);
         }
      }
   }
}