using ReportForIDS.Utils;
using System.Collections.Generic;

namespace ReportForIDS.Model
{
   public class MyField
   {
      public string FieldName { get; set; }
      public string TableName { get; set; }
      public bool IsSelected { get; set; }

      public MyField(string fieldName, string tableName)
      {
         this.FieldName = fieldName;
         this.TableName = tableName;
         this.IsSelected = false;
      }

      public MyField()
      {
      }

      public MyField Clone()
      {
         return new MyField(this.FieldName, this.TableName);
      }

      public string GetFullName()
      {
         return $"`{TableName}`.`{FieldName}`";
      }

      public override string ToString()
      {
         return this.TableName + "." + this.FieldName;
      }
   }

   public class MyFieldEqualityComparer : IEqualityComparer<MyField>
   {
      public bool Equals(MyField f1, MyField f2)
      {
         return f1.TableName.EqualsNotCaseSensitive(f2.TableName);
      }

      public int GetHashCode(MyField obj)
      {
         throw new System.NotImplementedException();
      }
   }
}