using ReportForIDS.UC;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ReportForIDS.ViewModel
{
   public class StepByStepWindowViewModel : ReportWindowBaseViewModel
   {
      public StepByStepWindowViewModel() : base()
      {
      }

      protected override void InitUC()
      {
         ListUC.Clear();
         ListUC = new List<UserControl>
         {
            new UCSelectTable() { DataContext = new UCSelectTableViewModel(PrevStep, NextStep) },
            new UCSelectField() { DataContext = new UCSelectFieldToDisplayVM(PrevStep, NextStep) },
            new UCSetCondition() { DataContext = new UCSetConditionViewModel(PrevStep, NextStep) },
            new UCSelectField() { DataContext = new UCSelectFieldToGroupVM(PrevStep, NextStep) },
            new UCGroupOption() { DataContext = new UCGroupOptionViewModel(PrevStep, NextStep, false) },
            new UCSaveReport() { DataContext = new UCSaveReportStepByStepVM(PrevStep) }
         };
      }
   }
}