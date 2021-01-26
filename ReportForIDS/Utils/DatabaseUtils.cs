using MySql.Data.MySqlClient;
using ReportForIDS.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace ReportForIDS.Utils
{
   public class DatabaseUtils
   {
      /// <summary>
      /// Test connection to database
      /// </summary>
      /// 
      /// <param name="error">Exception.Message</param>
      /// 
      /// <returns>
      /// true if 'Test connection succeeded'
      /// </returns>
      public static bool TestConnection(out string error)
      {
         bool result;
         try
         {
            MySqlConnection db = DatabaseConnection.GetInstance();
            db.Open();
            result = true;
            db.Close();
         }
         catch (System.Exception e)
         {
            error = e.Message;
            return false;
         }

         error = null;
         return result;
      }

      /// <summary>
      /// Get list Field from Sql query
      /// </summary>
      /// 
      /// <param name="sqlQuery"></param>
      /// <param name="tableName"></param>
      /// 
      /// <returns></returns>
      public static ObservableCollection<MyField> GetListField(string sqlQuery, string tableName)
      {
         var result = new ObservableCollection<MyField>();
         var dataTable = ExecuteQuery(sqlQuery.EscapeSQL() + " limit 0;");

         foreach (DataColumn column in dataTable.Columns)
         {
            result.Add(new MyField(column.ColumnName.ToString().ToUpper(), tableName));
         }
         return result;
      }

      /// <summary>
      /// Get list field from table
      /// </summary>
      /// 
      /// <param name="tableName">name of table in database</param>
      /// 
      /// <returns>List&#60;MyField&#62;</returns>
      public static List<MyField> GetListField(string tableName)
      {
         var result = new List<MyField>();
         using (MySqlConnection db = DatabaseConnection.GetInstance())
         {
            db.Open();

            MySqlCommand command = db.CreateCommand();
            command.CommandText = $"show columns from `{tableName}`;";
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
               var fieldName = reader.GetValue(0).ToString().Trim();
               // just get field not in list hidden
               if (Cons.ListHiddenFields.Count(x => x.EqualsNotCaseSensitive(fieldName)) == 0)
               {
                  result.Add(new MyField(fieldName, tableName));
               }
            }

            db.Close();
         }
         return result;
      }

      public static ObservableCollection<MyTable> GetListTable()
      {
         var result = new ObservableCollection<MyTable>();
         using (MySqlConnection db = DatabaseConnection.GetInstance())
         {
            db.Open();

            MySqlCommand command = db.CreateCommand();
            command.CommandText = "SHOW TABLES;";
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
               for (int i = 0; i < reader.FieldCount; i++)
               {
                  string tableName = reader.GetValue(i).ToString();
                  // just get table not in list hidden
                  if (Cons.ListHiddenTables.Count(x => x.EqualsNotCaseSensitive(tableName)) == 0)
                  {
                     result.Add(new MyTable(tableName));
                  }
               }
            }
            db.Close();
         }
         return result;
      }

      public static ObservableCollection<string> GetListSchema()
      {
         var result = new ObservableCollection<string>();
         using (MySqlConnection db = DatabaseConnection.GetInstance())
         {
            try
            {
               db.Open();

               MySqlCommand command = db.CreateCommand();
               command.CommandText = "SHOW DATABASES;";
               MySqlDataReader reader = command.ExecuteReader();
               while (reader.Read())
               {
                  for (int i = 0; i < reader.FieldCount; i++)
                  {
                     string schema = reader.GetValue(i).ToString();
                     // just get schema not in list hidden
                     if (Cons.ListHiddenSchemas.Count(x => x.EqualsNotCaseSensitive(schema)) == 0)
                     {
                        result.Add(schema);
                     }
                  }
               }
               db.Close();
            }
            catch (System.Exception) { }
         }
         return result;
      }

      public static DataTable ExecuteQuery(string query, int limit, List<object> parameters = null)
      {
         query = query.Trim();
         if (query.LastIndexOf(';') == query.Length - 1)
         {
            query = query.Substring(0, query.Length - 1);
         }
         query += $" limit {limit};";
         return ExecuteQuery(query, parameters);
      }

      public static DataTable ExecuteQuery(string query, List<object> parameters = null)
      {
         DataTable dataTable = new DataTable(); //store result
         try
         {
            using MySqlConnection db = DatabaseConnection.GetInstance();
            db.Open();

            MySqlCommand cmd = new MySqlCommand(query, db);
            SetCommandParameters(query, cmd, parameters);

            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(cmd);
            dataAdapter.Fill(dataTable);

            db.Close();
         }
         catch (System.Exception) { }
         return dataTable;
      }

      public static int ExecuteNonQuery(string query, List<object> parameters = null)
      {
         int data = 0;
         using (MySqlConnection db = DatabaseConnection.GetInstance())
         {
            db.Open();

            MySqlCommand cmd = new MySqlCommand(query, db);
            SetCommandParameters(query, cmd, parameters);
            data = cmd.ExecuteNonQuery();

            db.Close();
         }
         return data;
      }

      public static object ExecuteScalar(string query, List<object> parameters = null)
      {
         using MySqlConnection db = DatabaseConnection.GetInstance();
         db.Open();

         MySqlCommand cmd = new MySqlCommand(query, db);
         SetCommandParameters(query, cmd, parameters);
         object data = cmd.ExecuteScalar();
         db.Close();

         return data;
      }

      static void SetCommandParameters(string query, MySqlCommand cmd, List<object> parameters)
      {
         if (parameters != null)
         {
            string[] listPara = query.Split(' ');
            int i = 0;
            foreach (string item in listPara)
            {
               if (item.Contains('@'))
               {
                  cmd.Parameters.AddWithValue(item, parameters[i]);
                  i++;
               }
            }
         }
      }
   }
}