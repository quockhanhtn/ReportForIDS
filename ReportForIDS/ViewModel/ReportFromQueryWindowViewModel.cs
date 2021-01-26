using ReportForIDS.UC;

namespace ReportForIDS.ViewModel
{
   public class ReportFromQueryWindowViewModel : ReportWindowBaseViewModel
   {
      public string TemplateFilePath { get; set; }

      public ReportFromQueryWindowViewModel() : base()
      {
      }

      public ReportFromQueryWindowViewModel(string templateFilePath) : base()
      {
         TemplateFilePath = templateFilePath;
      }

      protected override void InitUC()
      {
         var insertQueryDtContxt = new UCInsertSqlQueryViewModel(PrevStep, NextStep);
         ListUC.Add(new UCInsertSqlQuery() { DataContext = insertQueryDtContxt });
         ListUC.Add(new UCSelectField() { DataContext = new UCSelectFieldToGroupFromQueryVM(PrevStep, NextStep) });
         //ListUC.Add(new UCSelectField() { DataContext = new UCSelectFieldViewModel(SelectFieldType.TO_HIDE, PrevStep, NextStep) });
         ListUC.Add(new UCGroupOption() { DataContext = new UCGroupOptionViewModel(PrevStep, NextStep, true) });
         ListUC.Add(new UCSaveReport() { DataContext = new UCSaveReportFromQueryVM(PrevStep) });

         if (!string.IsNullOrEmpty(TemplateFilePath))
         {
            insertQueryDtContxt.LoadedCommand.Execute(ListUC[0]);
            insertQueryDtContxt.LoadQueryCommand.Execute(TemplateFilePath);
         }
      }
   }
}