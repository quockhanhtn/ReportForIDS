using ReportForIDS.SessionData;
using System;
using System.Windows.Input;

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
            if (isCreateMultiColumn)
            {
               IsSepareteByComma = false;
               ExampleImageSrc = MULTI_COLUMN_IMAGE_SRC;
            }
            OnPropertyChanged();
         }
      }

      public bool IsSepareteByComma
      {
         get => isSepareteByComma;
         set
         {
            isSepareteByComma = value;
            if (isSepareteByComma)
            {
               IsCreateMultiColumn = false;
               ExampleImageSrc = SEPERATE_COMMA_IMAGE_SRC;
            }
            OnPropertyChanged();
         }
      }

      public string ExampleImageSrc { get => exampleImageSrc; set { exampleImageSrc = value; OnPropertyChanged(nameof(ExampleImageSrc)); } }

      public UCGroupOptionViewModel(Action prevAction, Action nextAction, bool isReportFormQuery)
      {
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

      public override void ReLoad()
      {
         IsSepareteByComma = true;
      }

      private bool isCreateMultiColumn = true;
      private bool isSepareteByComma = false;
      string exampleImageSrc;
   }
}