using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCSelectFieldViewModel : UCBaseViewModel
   {
      public ICommand SelectedFieldChangedCommand { get; set; }
      public ICommand CheckAllCommand { get; set; }
      public ICommand UncheckAllCommand { get; set; }

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
            CollectionViewSource.GetDefaultView(ListFields).Refresh();
            OnPropertyChanged();
         }
      }

      public ObservableCollection<MyField> ListFields { get => listFields; set { listFields = value; OnPropertyChanged(); } }

      public MyField SelectedField { get => selectedField; set { selectedField = value; OnPropertyChanged(); } }

      protected UCSelectFieldViewModel(Action prevAction, Action nextAction)
      {
         CheckAllCommand = new RelayCommand<object>((p) => NoOfSelectedField < NoOfAllField, (p) =>
         {
            foreach (MyField field in listFieldsWithoutFilter)
            {
               field.IsSelected = true;
            }
            OnPropertyChanged(nameof(NoOfSelectedField));
            CollectionViewSource.GetDefaultView(ListFields).Refresh();
         });

         UncheckAllCommand = new RelayCommand<object>((p) => NoOfSelectedField > 0, (p) =>
         {
            foreach (MyField field in listFieldsWithoutFilter)
            {
               field.IsSelected = false;
            }
            OnPropertyChanged(nameof(NoOfSelectedField));
            CollectionViewSource.GetDefaultView(ListFields).Refresh();
         });

         SelectedFieldChangedCommand = new RelayCommand<object>((p) => true, (p) => OnPropertyChanged(nameof(NoOfSelectedField)));

         PrevCommand = new RelayCommand<object>((p) => true, (p) => prevAction());

         NextCommand = new RelayCommand<object>((p) =>
         {
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

         listFieldsWithoutFilter = ListFields;
         OnPropertyChanged(nameof(NoOfAllField));
      }

      public virtual void LoadListField()
      {
      }

      public virtual void SaveSelectedFeild()
      {
      }

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
         ListFields = new ObservableCollection<MyField>()
         {
            ReportFromQueryData.ListQueries[0].CompareField
         };

         ReportFromQueryData.ListQueries.ForEach(q =>
         {
            ListFields.AddRange(q.ListFeilds);
            ListFields.Remove(q.CompareField);
         });
      }

      public override void SaveSelectedFeild()
         => ReportFromQueryData.SetListFieldToGroup(ListFields.Where(f => f.IsSelected));
   }

   public class UCSelectFieldToHideVM : UCSelectFieldViewModel
   {
      public UCSelectFieldToHideVM(Action prevAction, Action nextAction) : base(prevAction, nextAction)
      {
         Title = "Select field to hide".ToUpper();
      }

      public override void LoadListField()
      {
         ListFields = new ObservableCollection<MyField>() { };

         foreach (MyField f in ReportFromQueryData.ListFieldToGroup)
         {
            ListFields.Add(f.Clone());
         }
      }

      public override void SaveSelectedFeild()
         => ReportFromQueryData.SetListFieldToHide(ListFields.Where(f => f.IsSelected));
   }
}