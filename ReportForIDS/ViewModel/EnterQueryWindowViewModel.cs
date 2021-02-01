using ReportForIDS.Model;
using ReportForIDS.Utils;
using System;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class EnterQueryWindowViewModel : BaseViewModel
   {
      public ICommand LoadListFieldCommand { get; set; }
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

            try
            {
               var rowCount = Convert.ToInt32(DatabaseUtils.ExecuteScalar("select count(*) from " + SqlQueryStatement.AliasSQL("DT")));
               QueryItem.ExecResult = rowCount.ToString() + " row(s) return";
            }
            catch (Exception)
            {
               QueryItem.ExecResult = "Error while executing query";
               QueryItem.ListFeilds.Clear();
               QueryItem.CompareField = null;
            }
            OnPropertyChanged(nameof(QueryItem));
         }
      }

      public EnterQueryWindowViewModel(MyQuery updateQueryItem = null)
      {
         QueryItem = updateQueryItem ?? new MyQuery();
         SqlQueryStatement = QueryItem.SQLQuery;

         LoadListFieldCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            QueryItem.ListFeilds = DatabaseUtils.GetListField(SqlQueryStatement, "DT" + QueryItem.DisplayOrder);
            OnPropertyChanged(nameof(QueryItem));
         });

         OKCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            if (QueryItem.CompareField == null)
            {
               MessageBox.Show("Please select field to commpare", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Warning);
               return;
            }

            p.DialogResult = true;
            p.Close();
         });

         CancelCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            if (updateQueryItem == null)
            {
               MyQuery.LastOrder--;
            }

            p.DialogResult = false;
            p.Close();
         });
      }

      private MyQuery queryItem;
      private string sqlQueryStatement;
   }
}
