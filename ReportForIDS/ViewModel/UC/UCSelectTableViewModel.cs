﻿using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCSelectTableViewModel : UCBaseViewModel
   {
      public ICommand AddSelectedTableCommand { get; set; }
      public ICommand RemoveSelectedTableCommand { get; set; }
      public ICommand RemoveAllTableCommand { get; set; }

      public ObservableCollection<MyTable> ListTableQueues { get => listTableQueues; set { listTableQueues = value; OnPropertyChanged(); } }
      public MyTable TableQueue { get => tableQueue; set { tableQueue = value; OnPropertyChanged(); } }
      public ObservableCollection<MyTable> ListTableSelects { get => listTableSelects; set { listTableSelects = value; OnPropertyChanged(); } }
      public MyTable TableSelected { get => tableSelected; set { tableSelected = value; OnPropertyChanged(); } }

      public UCSelectTableViewModel(Action prevAction, Action nextAction)
      {
         AddSelectedTableCommand = new RelayCommand<object>((p) => { return TableQueue != null; }, (p) =>
         {
            ListTableSelects.Add(TableQueue);
            ListTableQueues.Remove(TableQueue);
         });

         RemoveSelectedTableCommand = new RelayCommand<object>((p) => { return TableSelected != null; }, (p) =>
         {
            ListTableQueues.Add(TableSelected);
            ListTableSelects.Remove(TableSelected);
            ListTableSelects_CollectionChanged(ListTableSelects, null);
         });

         RemoveAllTableCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            var messageBoxResult = CustomMessageBox.Show("This action will remove all item in list. Are you sure to continue ?",
                                                         Cons.TOOL_NAME,
                                                         MessageBoxButton.YesNo,
                                                         MessageBoxImage.Warning,
                                                         MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
               foreach (var item in ListTableSelects)
               {
                  ListTableQueues.Add(item);
               }
               ListTableSelects.Clear();
            }
         });

         PrevCommand = new RelayCommand<object>((p) => true, (p) => prevAction());

         NextCommand = new RelayCommand<object>((p) => { return ListTableSelects.Count > 0; }, (p) =>
         {
            StepByStepData.ListTable = ListTableSelects.ToList();
            nextAction();
         });

         ListTableSelects.CollectionChanged += ListTableSelects_CollectionChanged;
      }

      public override void ReLoad()
      {
         WaitWindow.Show(() =>
         {
            ListTableQueues = DatabaseUtils.GetListTable();
         }, null);

         ListTableSelects.Clear();
      }

      private void ListTableSelects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if (ListTableSelects.Count == 0)
         {
            ListTableQueues = DatabaseUtils.GetListTable();
            return;
         }

         ListTableQueues.Clear();
         ListTableQueues = FindRelatitionTable();
      }

      private ObservableCollection<MyTable> FindRelatitionTable()
      {
         var allTables = DatabaseUtils.GetListTable();

         var result = new ObservableCollection<MyTable>();

         var listRelatedTbName = new HashSet<string>();
         foreach (var item in ListTableSelects)
         {
            Cons.ListRelatedTables.FindAll(x => x.IsContain(item.TableName)).ForEach(tb =>
            {
               listRelatedTbName.Add(tb.Table1);
               listRelatedTbName.Add(tb.Table2);
            });
         }

         foreach (var item in allTables)
         {
            if (listRelatedTbName.Contains(item.TableName) && ListTableSelects.FirstOrDefault(x => x.TableName == item.TableName) == null)
            {
               result.Add(item);
            }
         }

         return result;
      }

      private ObservableCollection<MyTable> listTableQueues = new ObservableCollection<MyTable>();
      private MyTable tableQueue;
      private ObservableCollection<MyTable> listTableSelects = new ObservableCollection<MyTable>();
      private MyTable tableSelected;
   }
}