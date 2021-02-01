using ReportForIDS.Model;
using ReportForIDS.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class EnterQueryWindowViewModel : BaseViewModel
   {
      public ICommand LoadListFieldCommand { get; set; }
      public ICommand ExecuteQueryCommand { get; set; }
      public ICommand OKCommand { get; set; }
      public ICommand CancelCommand { get; set; }
      public MyQuery QueryItem { get => queryItem; set { queryItem = value; OnPropertyChanged(); } }

      public string SqlQueryStatement
      {
         get => sqlQueryStatement;
         set
         {
            sqlQueryStatement = value;
            QueryItem.SQLQuery = value;
            OnPropertyChanged(nameof(QueryItem));
            isChangedQuery = true;
         }
      }

      public EnterQueryWindowViewModel(MyQuery updateQueryItem = null)
      {
         QueryItem = updateQueryItem ?? new MyQuery();
         SqlQueryStatement = QueryItem.SQLQuery;

         LoadListFieldCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            Thread thread = new Thread(() =>
            {
               try
               {
                  var rowCount = Convert.ToInt32(DatabaseUtils.ExecuteScalar("select count(*) from " + SqlQueryStatement.AliasSQL("DT")));
                  Application.Current.Dispatcher.Invoke(new Action(delegate ()
                  {
                     QueryItem.ExecResult = rowCount.ToString() + " row(s) return";
                     OnPropertyChanged(nameof(QueryItem));
                  }));
               }
               catch (Exception)
               {
                  Application.Current.Dispatcher.Invoke(new Action(delegate ()
                  {
                     QueryItem.ExecResult = "Error while executing query";
                     QueryItem.ListFeilds.Clear();
                     QueryItem.CompareField = null;
                     OnPropertyChanged(nameof(QueryItem));
                  }));
               }
            })
            {
               IsBackground = true
            };
            thread.Start();

            QueryItem.ListFeilds = DatabaseUtils.GetListField(SqlQueryStatement, updateQueryItem?.AliasTableName ?? "");
            OnPropertyChanged(nameof(QueryItem));
            isChangedQuery = false;
         });

         ExecuteQueryCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
         });

         OKCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            if (isChangedQuery)
            {
               string feildName = QueryItem.CompareField.FieldName;
               LoadListFieldCommand.Execute(null);

               if (!string.IsNullOrEmpty(feildName))
               {
                  QueryItem.CompareField = QueryItem.ListFeilds.Where(x => x.FieldName.Equals(feildName)).FirstOrDefault();
               }
            }

            if (QueryItem.CompareField == null)
            {
               CustomMessageBox.Show("Please select field to commpare", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
               return;
            }

            p.DialogResult = true;
            p.Close();
         });

         CancelCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            if (updateQueryItem == null) { MyQuery.LastOrder--; }

            p.DialogResult = false;
            p.Close();
         });
      }

      private MyQuery queryItem;
      private string sqlQueryStatement;
      bool isChangedQuery = false;
   }
}