using ReportForIDS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace ReportForIDS.Utils
{
   public static class DataTableExtendsion
   {
      static public ObservableCollection<MyField> GetListColumnName(this DataTable dataTable, string tableName = "")
      {
         var result = new ObservableCollection<MyField>();
         foreach (DataColumn column in dataTable.Columns)
         {
            result.Add(new MyField(column.ColumnName.ToString().ToUpper(), tableName));
         }
         return result;
      }

      static public void SortByColumns(this DataTable dataTable, params string[] columns)
      {
         dataTable.DefaultView.Sort = string.Join(", ", columns);
         dataTable = dataTable.DefaultView.ToTable();
      }

      public static void SetColumnsOrder(this DataTable dataTable, params string[] columnNames)
      {
         int columnIndex = 0;
         foreach (var columnName in columnNames)
         {
            if (dataTable.Columns[columnName] != null)
            {
               dataTable.Columns[columnName].SetOrdinal(columnIndex++);
            }
         }
      }

      static public void AddDatatable(this DataTable primaryDatatable, DataTable dt)
      {
         int primaryColumns = primaryDatatable.Columns.Count;

         #region Add columns to primary datatable
         for (int j = 1; j < dt.Columns.Count; j++)
         {
            // if duplicate add '_'
            if (primaryDatatable.Columns.Contains(dt.Columns[j].ColumnName))
            {
               dt.Columns[j].ColumnName = dt.Columns[j].ColumnName + "_";
               j--;
            }
            else
            {
               primaryDatatable.Columns.Add(dt.Columns[j].ColumnName, dt.Columns[j].DataType);
            }
         }
         #endregion

         int primaryRows = primaryDatatable.Rows.Count;
         int dtRows = dt.Rows.Count;
         int m = 0, n = 0;

         while (m < primaryRows && n < dtRows)
         {
            if (primaryDatatable.Rows[m][0].ToString() == "453175")
            {
            }
            int compare = primaryDatatable.Rows[m][0].ToString().Compare(dt.Rows[n][0].ToString());
            if (compare < 0) { m++; }
            else if (compare > 0) { n++; }
            else
            {
               for (int k = 1; k < dt.Columns.Count; k++)
               {
                  primaryDatatable.Rows[m][primaryColumns + k - 1] = dt.Rows[n][k];
               }
               m++;
               //n++;
            }
         }
      }

      public static DataTable Join(DataTable primaryDatatable, List<MyQuery> queries)
      {
         for (int i = 0; i < queries.Count; i++)
         {
            var dt = queries[i].GetDatatable();
            int primaryColumns = primaryDatatable.Columns.Count;

            #region Add column to primary datatable
            for (int j = 1; j < dt.Columns.Count; j++)
            {
               // if duplicate add '_'
               if (primaryDatatable.Columns.Contains(dt.Columns[j].ColumnName))
               {
                  dt.Columns[j].ColumnName = dt.Columns[j].ColumnName + "_";
                  j--;
               }
               else
               {
                  primaryDatatable.Columns.Add(dt.Columns[j].ColumnName, dt.Columns[j].DataType);
               }
            }
            #endregion

            int primaryRows = primaryDatatable.Rows.Count;
            int dtRows = dt.Rows.Count;
            int m = 0, n = 0;

            while (m < primaryRows && n < dtRows)
            {
               int compare = primaryDatatable.Rows[m][0].ToString().Compare(dt.Rows[n][0].ToString());
               if (compare < 0)
               {
                  m++;
               }
               else if (compare > 0) { n++; }
               else
               {
                  for (int k = 1; k < dt.Columns.Count; k++)
                  {
                     primaryDatatable.Rows[m][primaryColumns + k - 1] = dt.Rows[n][k];
                  }
                  m++;
                  n++;
               }
            }
         }

         return primaryDatatable;
      }

      public static bool EqualWithBeforeRow(this DataTable dataTable, int rowIndex, string[] columns)
      {
         foreach (var column in columns)
         {
            var thisValue = dataTable.Rows[rowIndex][column].ToString();
            var beforeValue = dataTable.Rows[rowIndex - 1][column].ToString();
            if (!string.Equals(thisValue, beforeValue)) { return false; }
         }
         return true;
      }
   }
}
