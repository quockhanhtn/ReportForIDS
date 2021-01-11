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
   public class UCSelectFieldViewModel : UCViewModel
   {
      public ICommand SelectAllFieldCommand { get; set; }
      public ICommand PrevCommand { get; set; }
      public ICommand NextCommand { get; set; }

      public string Title { get; set; }
      public int NoOfSelectedField { get { return listFieldsWithoutFilter.Count(f => f.IsSelected); } }
      public int NoOfAllField { get { return listFieldsWithoutFilter.Count; } }

      public string KeySearchFieldName
      {
         get => keySearchFieldName;
         set
         {
            keySearchFieldName = value;
            if (value == "") { ListFields = listFieldsWithoutFilter; }
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
            OnPropertyChanged();
         }
      }

      protected UCSelectFieldViewModel(Action prevAction, Action nextAction)
      {
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
            return !(this is UCSelectFieldToDisplayVM) || listFieldsWithoutFilter.Count(f => f.IsSelected) > 0;
         }, (p) =>
         {
            ListFields = listFieldsWithoutFilter;
            SaveSelectedFeild();
            nextAction();
         });
      }

      public override void ReLoad()
      {
         ListFields.Clear();
         listFieldsWithoutFilter.Clear();

         LoadListField();

         #region Old code

         //switch (Type)
         //{
         //   case SelectFieldType.TO_DISPLAY:

         //      break;

         //   case SelectFieldType.TO_GROUP:

         //      break;

         //   case SelectFieldType.TO_GROUP_FROM_QUERY:
         //      ListFields = ReportFromQueryData.ExecuteDataTableOnlyHeader.GetListColumnName();
         //      //ListFields = ReportFromQueryData.ListQueries[0].ListFeilds;
         //      break;

         //   case SelectFieldType.TO_HIDE:
         //      //ListFields.AddRange(ReportFromQueryData.ListField1);
         //      //ListFields.AddRange(ReportFromQueryData.ListField2);
         //      //foreach (var item in ReportFromQueryData.ListFieldToGroup)
         //      //{
         //      //   ListFields.Remove(item);
         //      //}
         //      break;

         //   default:
         //      break;
         //}         //switch (Type)
         //{
         //   case SelectFieldType.TO_DISPLAY:

         //      break;

         //   case SelectFieldType.TO_GROUP:

         //      break;

         //   case SelectFieldType.TO_GROUP_FROM_QUERY:
         //      ListFields = ReportFromQueryData.ExecuteDataTableOnlyHeader.GetListColumnName();
         //      //ListFields = ReportFromQueryData.ListQueries[0].ListFeilds;
         //      break;

         //   case SelectFieldType.TO_HIDE:
         //      //ListFields.AddRange(ReportFromQueryData.ListField1);
         //      //ListFields.AddRange(ReportFromQueryData.ListField2);
         //      //foreach (var item in ReportFromQueryData.ListFieldToGroup)
         //      //{
         //      //   ListFields.Remove(item);
         //      //}
         //      break;

         //   default:
         //      break;
         //}

         #endregion Old code

         listFieldsWithoutFilter = ListFields;
         OnPropertyChanged(nameof(NoOfAllField));
      }

      public virtual void LoadListField()
      {
      }

      public virtual void SaveSelectedFeild()
      {
      }

      #region not use

      //public override bool LoadPreviosData()
      //{
      //   ReLoad();
      //   if (Type == SelectFieldType.TO_DISPLAY)
      //   {
      //      foreach (var item in StepByStepData.ListFieldDisplay)
      //      {
      //         ListFields[ListFields.IndexOf(item)].IsSelected = true;
      //      }
      //   }
      //   else if (Type == SelectFieldType.TO_GROUP)
      //   {
      //      foreach (var item in StepByStepData.ListFieldGroup)
      //      {
      //         ListFields[ListFields.IndexOf(item)].IsSelected = true;
      //      }
      //   }
      //   return true;
      //}

      //private void RefreshList()
      //{
      //   var newList = new ObservableCollection<MyField>();
      //   foreach (var f in ListFields)
      //   {
      //      newList.Add(f);
      //   }
      //   ListFields.Clear();
      //   ListFields = newList;
      //}

      #endregion not use

      protected ObservableCollection<MyField> listFieldsWithoutFilter = new ObservableCollection<MyField>();
      private ObservableCollection<MyField> listFields = new ObservableCollection<MyField>();

      private MyField selectedField;
      private string keySearchFieldName;
   }

   public class UCSelectFieldToDisplayVM : UCSelectFieldViewModel
   {
      public UCSelectFieldToDisplayVM(Action prevAction, Action nextAction) : base(prevAction, nextAction)
      {
         Title = "Select field to display".ToUpper();
      }

      public override void LoadListField()
      {
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
      }

      public override void SaveSelectedFeild()
      {
         StepByStepData.ListFieldDisplay.Clear();
         foreach (var field in ListFields)
         {
            if (field.IsSelected) { StepByStepData.ListFieldDisplay.Add(field); }
         }
      }
   }

   public class UCSelectFieldToGroupVM : UCSelectFieldViewModel
   {
      public UCSelectFieldToGroupVM(Action prevAction, Action nextAction) : base(prevAction, nextAction)
      {
         Title = "Select field to group".ToUpper();
      }

      public override void LoadListField()
      {
         foreach (var f in StepByStepData.ListFieldDisplay) { ListFields.Add(f.Clone()); }
      }

      public override void SaveSelectedFeild()
      {
         StepByStepData.ListFieldGroup.Clear();
         foreach (var field in ListFields)
         {
            if (field.IsSelected) { StepByStepData.ListFieldGroup.Add(field); }
         }
      }
   }

   public class UCSelectFieldToGroupFromQueryVM : UCSelectFieldViewModel
   {
      public UCSelectFieldToGroupFromQueryVM(Action prevAction, Action nextAction) : base(prevAction, nextAction)
      {
         Title = "Select field to group".ToUpper();
      }

      public override void LoadListField()
      {
         ListFields = new ObservableCollection<MyField>();
         ReportFromQueryData.ListQueries.ForEach(q => ListFields.AddRange(q.ListFeilds));
         //ListFields = ReportFromQueryData.ExecuteDataTableOnlyHeader.GetListColumnName();
      }

      public override void SaveSelectedFeild()
      {
         ReportFromQueryData.ListFieldToGroup.Clear();
         ReportFromQueryData.ListFieldToGroup.AddRange(ListFields.Where(f => f.IsSelected));
      }
   }

   public class UCSelectFieldToHideVM : UCSelectFieldViewModel
   {
      public UCSelectFieldToHideVM(Action prevAction, Action nextAction) : base(prevAction, nextAction)
      {
         Title = "Select field to hide".ToUpper();
      }

      public override void LoadListField()
      {
      }

      public override void SaveSelectedFeild()
      {
         ReportFromQueryData.ListFieldToHide.Clear();
         ReportFromQueryData.ListFieldToHide.AddRange(ListFields.Where(f => f.IsSelected));
      }
   }
}