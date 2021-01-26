using MySql.Data.MySqlClient;

namespace ReportForIDS.Utils
{
   public class DatabaseConnection
   {
      private static string server = "", port = "", databaseName = "", uid = "", pwd = "";
      private static MySqlConnection connection = null;

      private static string ConnectionString
      {
         get => $"Server={Server};Port={Port};Database={DatabaseName};Uid={Uid};Pwd={Pwd};" +
            "CharSet=utf8mb4;Convert Zero Datetime=True;default command timeout=360;Pooling=true;";
      }

      public static string Server { get => server; set { server = value; RefreshConnection(); } }
      public static string Port { get => port; set { port = value; RefreshConnection(); } }
      public static string DatabaseName { get => databaseName; set { databaseName = value; RefreshConnection(); } }
      public static string Uid { get => uid; set { uid = value; RefreshConnection(); } }
      public static string Pwd { get => pwd; set { pwd = value; RefreshConnection(); } }

      public static MySqlConnection GetInstance()
      {
         if (connection != null)
         {
            connection = new MySqlConnection(ConnectionString);
         }
         return connection;
      }

      private static void RefreshConnection()
      {
         try
         {
            connection = new MySqlConnection(ConnectionString);
         }
         catch (System.Exception) { }
      }
   }
}