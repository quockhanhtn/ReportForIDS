using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCSetConditionViewModel : UCBaseViewModel
   {
      public ICommand AddCommand { get; set; }
      public ICommand SaveCommand { get; set; }
      public ICommand MoveTopCommand { get; set; }
      public ICommand MoveUpCommand { get; set; }
      public ICommand MoveDownCommand { get; set; }
      public ICommand MoveBotCommand { get; set; }
      public ICommand DeleteCommand { get; set; }
      public ICommand DeleteAllCommand { get; set; }

      public MyCondition ConditionSelected
      {
         get => conditionSelected;
         set
         {
            conditionSelected = value;
            if (value != null)
            {
               InputConditionOrder = value.Order;
               InputConditionField = value.Field;
               InputConditionType = value.ConditionType;
               InputConditionValue = value.Value;

               BtnAddEnable = true;
            }
            OnPropertyChanged();
         }
      }

      public List<string> ListConditionType { get => Cons.LIST_CONDITION_TYPE; }
      public ObservableCollection<MyCondition> ListConditions { get => listConditions; set { listConditions = value; OnPropertyChanged(); } }
      public ObservableCollection<MyField> ListFields { get => listFields; set { listFields = value; OnPropertyChanged(); } }

      public int InputConditionOrder { get => conditionOrder; set { conditionOrder = value; OnPropertyChanged(); } }
      public MyField InputConditionField { get => conditionField; set { conditionField = value; OnPropertyChanged(); } }
      public string InputConditionType { get => conditionType; set { conditionType = value; OnPropertyChanged(); } }
      public string InputConditionValue { get => conditionValue; set { conditionValue = value; OnPropertyChanged(); } }

      public bool BtnAddEnable { get => btnAddEnable; set { btnAddEnable = value; OnPropertyChanged(); } }

      public UCSetConditionViewModel(Action prevAction, Action nextAction)
      {
         AddCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            if (InputConditionOrder == MyCondition.LastOrder + 1 && p == null)
            {
               SaveCommand.Execute(null);
            }
            else
            {
               BtnAddEnable = false;
               ConditionSelected = null;

               InputConditionOrder = MyCondition.LastOrder + 1;
               InputConditionField = null;
               InputConditionType = null;
               InputConditionValue = "";
            }
         });

         SaveCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            if (InputConditionField == null)
            {
               CustomMessageBox.Show("Please select \"Field name\"", Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
               return;
            }

            if (string.IsNullOrEmpty(InputConditionType))
            {
               CustomMessageBox.Show("Please select \"ConditionType\"", Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
               return;
            }

            if (string.IsNullOrEmpty(InputConditionValue))
            {
               CustomMessageBox.Show("Please input \"Value\"", Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
               return;
            }

            if (InputConditionOrder == MyCondition.LastOrder + 1)   //Is add new
            {
               var newCondition = new MyCondition()
               {
                  Field = InputConditionField,
                  ConditionType = InputConditionType,
                  Value = InputConditionValue,
               };
               ListConditions.Add(newCondition);
               BtnAddEnable = true;
            }
            else                 //update exitst
            {
               var condition = ListConditions.Where(c => c.Order == InputConditionOrder).FirstOrDefault();
               if (condition == null) { return; }

               condition.Field = InputConditionField;
               condition.ConditionType = InputConditionType;
               condition.Value = InputConditionValue;

               RefreshListConditions();
            }
         });

         #region Define MoveCommand

         MoveTopCommand = new RelayCommand<object>((p) => { return ConditionSelected != null && ConditionSelected.Order > 1; }, (p) =>
         {
            ListConditions.Where(x => x.Order < ConditionSelected.Order).ToList().ForEach(x => x.Order += 1);
            ConditionSelected.Order = 1;
            SortListConditions();
         });

         MoveUpCommand = new RelayCommand<object>((p) => { return ConditionSelected != null && ConditionSelected.Order > 1; }, (p) =>
         {
            ListConditions.FirstOrDefault(x => x.Order == ConditionSelected.Order - 1).Order += 1;
            ConditionSelected.Order -= 1;
            SortListConditions();
         });

         MoveDownCommand = new RelayCommand<object>((p) => { return ConditionSelected != null && ConditionSelected.Order < ListConditions.Count; }, (p) =>
         {
            ListConditions.FirstOrDefault(x => x.Order == ConditionSelected.Order + 1).Order -= 1;
            ConditionSelected.Order += 1;
            SortListConditions();
         });

         MoveBotCommand = new RelayCommand<object>((p) => { return ConditionSelected != null && ConditionSelected.Order < ListConditions.Count; }, (p) =>
         {
            ListConditions.Where(x => x.Order > ConditionSelected.Order).ToList().ForEach(x => x.Order -= 1);
            ConditionSelected.Order = ListConditions.Count;
            SortListConditions();
         });

         #endregion Define MoveCommand

         DeleteCommand = new RelayCommand<object>((p) => { return ConditionSelected != null; }, (p) =>
         {
            ListConditions.Where(x => x.Order > ConditionSelected.Order).ToList().ForEach(x => x.Order -= 1);
            ListConditions.Remove(ConditionSelected);
            MyCondition.LastOrder -= 1;
            RefreshListConditions();
         });

         DeleteAllCommand = new RelayCommand<object>((p) => { return ListConditions.Count > 0; }, (p) =>
         {
            ReLoad();
         });

         PrevCommand = new RelayCommand<object>((p) => true, (p) => prevAction());

         NextCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            StepByStepData.ListCondition = ListConditions.ToList();
            nextAction();
         });
      }

      public override void ReLoad()
      {
         for (int i = ListConditions.Count - 1; i >= 0; i--)
         {
            if (!StepByStepData.ListFieldDisplay.Contains(ListConditions[i].Field))
            {
               ConditionSelected = ListConditions[i];
               DeleteCommand.Execute(null);
            }
         }

         //ListConditions.Clear();
         //MyCondition.LastOrder = 0;
         ListFields = StepByStepData.ListFieldDisplay.ToObservableCollection();

         AddCommand.Execute("first-time");
         BtnAddEnable = true;
      }

      private void SortListConditions()
      {
         CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListConditions);
         view.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
      }

      private void RefreshListConditions()
      {
         var refresh = new ObservableCollection<MyCondition>();
         foreach (var con in ListConditions) { refresh.Add(con); }

         ListConditions.Clear();
         ListConditions = refresh;
      }

      private MyCondition conditionSelected;
      private ObservableCollection<MyCondition> listConditions = new ObservableCollection<MyCondition>();
      private ObservableCollection<MyField> listFields = new ObservableCollection<MyField>();

      private int conditionOrder = MyCondition.LastOrder + 1;
      private MyField conditionField;
      private string conditionType;
      private string conditionValue;

      private bool btnAddEnable = true;
   }
}