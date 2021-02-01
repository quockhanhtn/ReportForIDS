using ReportForIDS.Model;

namespace ReportForIDS.ViewModel
{
   public class EditReportInfoWindowViewModel : BaseViewModel
   {
      public MyReport EditReport { get => editReport; set { editReport = value; OnPropertyChanged(); } }

      public EditReportInfoWindowViewModel(MyReport myReport)
      {
         editReport = myReport;
      }

      private MyReport editReport;
   }
}