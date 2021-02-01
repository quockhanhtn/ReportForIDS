using System.Windows.Controls;

namespace ReportForIDS.UC
{
   /// <summary>
   /// Interaction logic for UCTitleBar.xaml
   /// </summary>
   public partial class UCTitleBar : UserControl
   {
      public UCTitleBar()
      {
         InitializeComponent();
         this.DataContext = new ViewModel.UCTitleBarViewModel();
      }
   }
}
