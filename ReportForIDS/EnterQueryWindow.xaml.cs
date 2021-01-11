using System.Windows;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for EnterQueryWindow.xaml
   /// </summary>
   public partial class EnterQueryWindow : Window
   {
      public EnterQueryWindow()
      {
         InitializeComponent();
         txtQuery.Focus();
      }
   }
}