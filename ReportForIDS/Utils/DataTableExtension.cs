using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ReportForIDS.Utils
{
   public static class DataTableExtension
   {
      public static void SortByColumns(this DataTable dataTable, params string[] columns)
      {
         List<string> listColumn = columns.ToList();

         for (int i = 0; i < dataTable.Columns.Count; i++)
         {
            if (listColumn.IndexOf(dataTable.Columns[i].ColumnName) < 0)
            {
               listColumn.Add(dataTable.Columns[i].ColumnName);
            }
         }

         dataTable.DefaultView.Sort = string.Join(", ", listColumn.Select(x => x + " ASC"));
         _ = dataTable.DefaultView.ToTable();
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

      public static void AddDatatable(this DataTable primaryDatatable, DataTable dt)
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

         #endregion Add columns to primary datatable

         int primaryRows = primaryDatatable.Rows.Count;
         int dtRows = dt.Rows.Count;
         int m = 0, n = 0;

         while (m < primaryRows && n < dtRows)
         {
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

      public static DataTable CloneAndConvertToString(this DataTable dataTable)
      {
         DataTable rs = dataTable.Clone();
         foreach (DataColumn dataColumn in rs.Columns)
         {
            dataColumn.DataType = typeof(string);
         }
         return rs;
      }

      public static DataTable CopyAndConvertToString(this DataTable dataTable)
      {
         DataTable rs = dataTable.Clone();
         foreach (DataColumn dataColumn in rs.Columns)
         {
            dataColumn.DataType = typeof(string);
         }

         foreach (DataRow row in dataTable.Rows)
         {
            rs.ImportRow(row);
         }

         return rs;
      }

      public static void RemoveColumns(this DataTable dataTable, List<string> columns)
      {
         if (columns != null && columns.Count > 0)
         {
            for (int i = 0; i < columns.Count; i++)
            {
               int index = dataTable.Columns.IndexOf(columns[i]);
               if (index > -1)
               {
                  dataTable.Columns.RemoveAt(index);
               }
            }
         }
      }
   }
}