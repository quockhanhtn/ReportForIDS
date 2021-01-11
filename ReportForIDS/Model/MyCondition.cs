namespace ReportForIDS.Model
{
   public class MyCondition
   {
      public int Order { get; set; }
      public MyField Field { get; set; }
      public string ConditionType { get; set; }
      public string Value { get; set; }

      public MyCondition()
      {
         Order = ++LastOrder;
      }

      public static int LastOrder = 0;

      public string ToQuery()
      {
         var query = Field.GetFullName();

         switch (Cons.ListConditionType.IndexOf(ConditionType))
         {
            case 0:
               query += $" > '{Value}'";
               break;

            case 1:
               query += $" >= '{Value}'";
               break;

            case 2:
               query += $" < '{Value}'";
               break;

            case 3:
               query += $" <= '{Value}'";
               break;

            case 4:
               query += $" = '{Value}'";
               break;

            case 5:
               query += $" <> '{Value}'";
               break;

            case 6:
               query += $"LIKE'%{Value}%'";
               break;

            case 7:
               query += $"NOT LIKE'%{Value}%'";
               break;
         }

         return query;
      }
   }
}