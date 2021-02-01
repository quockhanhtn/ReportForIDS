using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public enum SelectFieldType
   {
      TO_DISPLAY,
      TO_GROUP,
      TO_GROUP_FROM_QUERY,
      TO_HIDE,
   }

   public class UCSelectFieldViewModel : UCViewModel
   {
      public ICommand SelectAllFieldCommand { get; set; }
      public ICommand PrevCommand { get; set; }
      public ICommand NextCommand { get; set; }

      public SelectFieldType Type { get; set; }

      public string Title
      {
         get
         {
            string title = "";
            switch (Type)
            {
               case SelectFieldType.TO_DISPLAY:
                  title = "Select field to display".ToUpper();
                  break;

               case SelectFieldType.TO_GROUP:
               case SelectFieldType.TO_GROUP_FROM_QUERY:
                  title = "Select field to group".ToUpper();
                  break;

               case SelectFieldType.TO_HIDE:
                  title = "Select field to hide".ToUpper();
                  break;

               default:
                  break;
            }
            return title;
         }
      }

      public int NoOfSelectedField { get { return listFieldsWithoutFilter.Count(f => f.IsSelected); } }
      public int NoOfAllField { get { return listFieldsWithoutFilter.Count; } }

      public string KeySearchFieldName
      {
         get => keySearchFieldName;
         set
         {
            keySearchFieldName = value;
            if (value == "")
            {
               ListFields = listFieldsWithoutFilter;
            }
            else
            {
               ListFields = listFieldsWithoutFilter.Where(f => f.FieldName.ToLower().Contains(value.ToLower())).ToObservableCollection();
            }
            OnPropertyChanged();
         }
      }

      public ObservableCollection<MyField> ListFields { get => listFields; set { listFields = value; OnPropertyChanged(); } }

      public MyField SelectedField
      {
         get => selectedField;
         set
         {
            selectedField = value;
            //if (value != null)
            //{
            //   if (Type == SelectFieldType.TO_GROUP)
            //   {
            //      var select = !value.IsSelected;
            //      value.IsSelected = select;
            //      refreshList();
            //   }
            //}
            OnPropertyChanged();
         }
      }

      public UCSelectFieldViewModel(SelectFieldType type, Action prevAction, Action nextAction)
      {
         Type = type;

         #region SelectAllFieldCommand

         //SelectAllFieldCommand = new RelayCommand<TextBlock>((p) =>
         //{
         //   if (NoOfSelectedField < NoOfAllField)
         //   {
         //      p.Text = "Select all";
         //   }
         //   else
         //   {
         //      p.Text = "Unselect all";
         //   }

         //   return p != null;
         //}, (p) =>
         //{
         //   if (NoOfSelectedField < NoOfAllField)
         //   {
         //      for (int i = 0; i < listFieldsWithoutFilter.Count; i++)
         //      {
         //         listFieldsWithoutFilter[i].IsSelected = true;
         //      }
         //   }
         //   else
         //   {
         //      for (int i = 0; i < listFieldsWithoutFilter.Count; i++)
         //      {
         //         listFieldsWithoutFilter[i].IsSelected = false;
         //      }
         //   }
         //   OnPropertyChanged();
         //});

         #endregion SelectAllFieldCommand

         PrevCommand = new RelayCommand<object>((p) => { return true; }, (p) => prevAction());

         NextCommand = new RelayCommand<object>((p) =>
         {
            OnPropertyChanged(nameof(NoOfSelectedField));
            return Type != 0 || listFieldsWithoutFilter.Count(f => f.IsSelected) > 0;
         }, (p) =>
         {
            ListFields = listFieldsWithoutFilter;
            switch (Type)
            {
               case SelectFieldType.TO_DISPLAY:
                  StepByStepData.ListFieldDisplay.Clear();
                  foreach (var field in ListFields)
                  {
                     if (field.IsSelected) { StepByStepData.ListFieldDisplay.Add(field); }
                  }
                  break;

               case SelectFieldType.TO_GROUP:
                  StepByStepData.ListFieldGroup.Clear();
                  foreach (var field in ListFields)
                  {
                     if (field.IsSelected) { StepByStepData.ListFieldGroup.Add(field); }
                  }
                  break;

               case SelectFieldType.TO_GROUP_FROM_QUERY:
                  ReportFromQueryData.ListFieldToGroup.Clear();
                  ReportFromQueryData.ListFieldToGroup.AddRange(ListFields.Where(f => f.IsSelected));
                  break;

               case SelectFieldType.TO_HIDE:
                  ReportFromQueryData.ListFieldToHide.Clear();
                  ReportFromQueryData.ListFieldToHide.AddRange(ListFields.Where(f => f.IsSelected));
                  break;
            }
            nextAction();
         });
      }

      public override void ReLoad()
      {
         ListFields.Clear();
         listFieldsWithoutFilter.Clear();

         switch (Type)
         {
            case SelectFieldType.TO_DISPLAY:
               var fieldsName = new HashSet<string>();
               var myfields = new ObservableCollection<MyField>();
               foreach (var table in StepByStepData.ListTable)
               {
                  table.Fields.ForEach(f =>
                  {
                     fieldsName.Add(f.FieldName);
                     myfields.Add(f);
                  });
               }
               ListFields = myfields;
               break;

            case SelectFieldType.TO_GROUP:
               foreach (var f in StepByStepData.ListFieldDisplay) { ListFields.Add(f.Clone()); }
               break;

            case SelectFieldType.TO_GROUP_FROM_QUERY:
               ListFields = ReportFromQueryData.ExecuteDataTableOnlyHeader.GetListColumnName();
               //ListFields = ReportFromQueryData.ListQueries[0].ListFeilds;
               break;

            case SelectFieldType.TO_HIDE:
               //ListFields.AddRange(ReportFromQueryData.ListField1);
               //ListFields.AddRange(ReportFromQueryData.ListField2);
               //foreach (var item in ReportFromQueryData.ListFieldToGroup)
               //{
               //   ListFields.Remove(item);
               //}
               break;

            default:
               break;
         }

         listFieldsWithoutFilter = ListFields;
         OnPropertyChanged(nameof(NoOfAllField));
      }

      public override bool LoadPreviosData()
      {
         ReLoad();
         if (Type == SelectFieldType.TO_DISPLAY)
         {
            foreach (var item in StepByStepData.ListFieldDisplay)
            {
               ListFields[ListFields.IndexOf(item)].IsSelected = true;
            }
         }
         else if (Type == SelectFieldType.TO_GROUP)
         {
            foreach (var item in StepByStepData.ListFieldGroup)
            {
               ListFields[ListFields.IndexOf(item)].IsSelected = true;
            }
         }
         return true;
      }

      private void RefreshList()
      {
         var newList = new ObservableCollection<MyField>();
         foreach (var f in ListFields)
         {
            newList.Add(f);
         }
         ListFields.Clear();
         ListFields = newList;
      }

      private ObservableCollection<MyField> listFieldsWithoutFilter = new ObservableCollection<MyField>();
      private ObservableCollection<MyField> listFields = new ObservableCollection<MyField>();

      private MyField selectedField;
      private string keySearchFieldName;
   }

   public class UCSelectFieldToGroupViewModel : UCSelectFieldViewModel
   {
      public UCSelectFieldToGroupViewModel() { }
   }
}