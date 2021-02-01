using ReportForIDS.Model;
using ReportForIDS.ViewModel;
using System.Windows;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for EditReportInfoWindow.xaml
   /// </summary>
   public partial class EditReportInfoWindow : Window
   {
      public EditReportInfoWindow()
      {
         InitializeComponent();
      }

      public static MyReport Show(MyReport myReport)
      {
         var windowDataContext = new EditReportInfoWindowViewModel(myReport);
         EditReportInfoWindow editReportInfoWindow = new EditReportInfoWindow() { DataContext = windowDataContext };
         var result = editReportInfoWindow.ShowDialog();
         if (result == true)
         {
            myReport = windowDataContext.EditReport;
         }
         return myReport;
      }
   }
}