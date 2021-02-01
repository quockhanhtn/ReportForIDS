using ReportForIDS.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ReportForIDS.Utils
{
   static public class ExtendsionUtils
   {
      public static bool EqualsNotCaseSensitive(this string str1, string str2)
      {
         return str1.ToLower().Equals(str2.ToLower());
      }

      static public string EscapeSQL(this string sqlQuery)
      {
         if (string.IsNullOrEmpty(sqlQuery)) { return ""; }
         //sqlQuery = sqlQuery.Replace("''", "'").Replace("'", "''").Replace(@"\", @"\\").Trim();
         // replace last char if = ','
         while (sqlQuery[sqlQuery.Length - 1] == ',')
         {
            sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
         }
         // replace last char if = ';'
         while (sqlQuery[sqlQuery.Length - 1] == ';')
         {
            sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
         }
         return sqlQuery;
      }
      
      static public string AliasSQL(this string sqlQuery, string aliasName)
      {
         return "(" + sqlQuery.EscapeSQL() + ") as " + aliasName;
      }

      static public int Compare(this string str1, string str2)
      {
         if (str1.Length < str2.Length)
         {
            return -1;
         }
         else if (str1.Length > str2.Length)
         {
            return 1;
         }
         else
         {
            return string.Compare(str1, str2);
         }
      }

      static public void AddUnique(this List<MyField> myFields, MyField newField)
      {
         if (myFields.Count(f => f.FieldName.EqualsNotCaseSensitive(newField.FieldName)) == 0)
         {
            myFields.Add(newField);
         }
      }

      static public void AddUniqueRange(this List<MyField> myFields, IEnumerable<MyField> range)
      {
         foreach (var f in range)
         {
            myFields.AddUnique(f);
         }
      }

      static public ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
      {
         var result = new ObservableCollection<T>();
         foreach (var item in collection)
         {
            result.Add(item);
         }
         return result;
      }

      static public void AddRange<T>(this ObservableCollection<T> thisObCollection, ObservableCollection<T> collection)
      {
         foreach (var item in collection)
         {
            thisObCollection.Add(item);
         }
      }
      static public void AddRange<T>(this ObservableCollection<T> thisObCollection, IEnumerable<T> collection)
      {
         foreach (var item in collection)
         {
            thisObCollection.Add(item);
         }
      }
   }
}
