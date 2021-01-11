using System;

namespace ReportForIDS.Utils
{
   public static class StringExtension
   {
      public static bool ToBool(this string str, bool defaultValue = false)
      {
         if (string.IsNullOrEmpty(str)) { return defaultValue; }

         if (str == bool.TrueString)
         {
            return true;
         }
         else if (str == bool.FalseString)
         {
            return false;
         }

         return defaultValue;
      }

      public static bool EqualsNotCaseSensitive(this string str1, string str2) =>  str1.ToLower().Equals(str2.ToLower());

      public static string EscapeSQL(this string sqlQuery)
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

      public static string AliasSQL(this string sqlQuery, string aliasName) => "(" + sqlQuery.EscapeSQL() + ") as " + aliasName;

      public static int Compare(this string str1, string str2)
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

      public static string RemoveNonDigit(this string str)
      {
         if (string.IsNullOrEmpty(str)) { return ""; }

         for (int i = str.Length - 1; i >= 0; i--)
         {
            if (!char.IsDigit(str, i)) { str = str.Remove(i, 1); }
         }
         return str;
      }

      public static string RemoveChar(this string str, char[] characters)
      {
         if (string.IsNullOrEmpty(str)) { return ""; }

         if (characters == null) { return str; }

         while (str.IndexOfAny(characters) >= 0)
         {
            str = str.Remove(str.IndexOfAny(characters));
         }

         return str;
      }

      public static int ToInt32(this string str)
      {
         Int32.TryParse(str, out int result);
         return result;
      }
   }
}