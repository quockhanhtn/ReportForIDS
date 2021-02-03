using ReportForIDS.SessionData;
using System;

namespace ReportForIDS.ViewModel
{
   public class UCGroupOptionViewModel : UCBaseViewModel
   {
      private static readonly string MULTI_COLUMN_IMAGE_SRC = "/Resources/Images/multi-column.png";
      private static readonly string SEPERATE_COMMA_IMAGE_SRC = "/Resources/Images/separate-by-comma.png";

      public bool IsCreateMultiColumn
      {
         get => isCreateMultiColumn;
         set
         {
            isCreateMultiColumn = value;
            ExampleImageSrc = value ? MULTI_COLUMN_IMAGE_SRC : SEPERATE_COMMA_IMAGE_SRC;
            OnPropertyChanged(nameof(IsCreateMultiColumn));
            OnPropertyChanged(nameof(IsSepareteByComma));
         }
      }

      public bool IsSepareteByComma { get => !IsCreateMultiColumn; set { IsCreateMultiColumn = !value; } }

      public string ExampleImageSrc { get => exampleImageSrc; set { exampleImageSrc = value; OnPropertyChanged(nameof(ExampleImageSrc)); } }

      public UCGroupOptionViewModel(Action prevAction, Action nextAction, bool isReportFormQuery)
      {
         IsSepareteByComma = true;

         PrevCommand = new RelayCommand<object>((p) => true, (p) => prevAction());
         NextCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            if (isReportFormQuery)
            {
               ReportFromQueryData.GroupToMultiColumn = IsCreateMultiColumn;
            }
            else
            {
               StepByStepData.GroupToMultiColumn = IsCreateMultiColumn;
            }
            nextAction();
         });
      }

      public override void ReLoad() {  }

      private bool isCreateMultiColumn = true;
      private string exampleImageSrc;
   }
}