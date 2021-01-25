using ReportForIDS.Utils;
using System.Collections.Generic;

namespace ReportForIDS.Model
{
   public class MyField
   {
      public string FieldName { get; set; }
      public string TableName { get; set; }
      public bool IsSelected { get; set; }
      public bool CanSelected { get; set; }

      public MyField(string fieldName, string tableName)
      {
         this.FieldName = fieldName;
         this.TableName = tableName;
         this.IsSelected = false;
         this.CanSelected = true;
      }

      public MyField()
      {
      }

      public MyField Clone() => new MyField(this.FieldName, this.TableName);

      public string GetFullName() => $"`{TableName}`.`{FieldName}`";

      public override string ToString() => this.TableName + "." + this.FieldName;
   }

   public class MyFieldEqualityComparer : IEqualityComparer<MyField>
   {
      public bool Equals(MyField f1, MyField f2) => f1.TableName.EqualsNotCaseSensitive(f2.TableName);

      public int GetHashCode(MyField obj) => obj.GetHashCode();
   }
}