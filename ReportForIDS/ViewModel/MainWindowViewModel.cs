using ReportForIDS.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class MainWindowViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand LoadListDatabaseNameCommand { get; set; }
      public ICommand TestConnectionCommand { get; set; }
      public ICommand ViewRecentReportCommand { get; set; }
      public ICommand StepByStepCommand { get; set; }
      public ICommand BySQLCommand { get; set; }

      public string Server
      {
         get => server;
         set
         {
            server = value;
            DatabaseConnection.Server = value;
            OnPropertyChanged();
         }
      }
      public string Port
      {
         get => port;
         set
         {
            port = value;
            DatabaseConnection.Port = value;
            OnPropertyChanged();
         }
      }
      public string Username
      {
         get => username;
         set
         {
            username = value;
            DatabaseConnection.Uid = value;
            OnPropertyChanged();
         }
      }
      public string Password
      {
         get => password;
         set
         {
            password = value;
            DatabaseConnection.Pwd = value;
            OnPropertyChanged();
         }
      }

      public string DatabaseName
      {
         get => databaseName;
         set
         {
            databaseName = value;
            DatabaseConnection.DatabaseName = value;
            OnPropertyChanged();
         }
      }

      public ObservableCollection<string> ListDatabaseNames { get => listDatabaseNames; set { listDatabaseNames = value; OnPropertyChanged(); } }
      
      public MainWindowViewModel()
      {
         LoadedCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            LoadDatabaseConnectConfig();
            DatabaseUtils.TestConnection(out var connectError);
         });

         LoadListDatabaseNameCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            ListDatabaseNames = DatabaseUtils.GetListSchema();
         });

         TestConnectionCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            if (DatabaseUtils.TestConnection(out var connectError)) { MessageBox.Show("Test connection succeeded", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Information); }
            else { MessageBox.Show("Cannot connect to database.\n\r" + connectError, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error); }
         });

         ViewRecentReportCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            ListReportWindow listReportWindow = new ListReportWindow()
            {
               DataContext = new ListReportWindowViewModel()
            };
            listReportWindow.Show();
         });

         StepByStepCommand = new RelayCommand<object>((p) => { return !string.IsNullOrEmpty(DatabaseName) && DatabaseUtils.TestConnection(out var connectError); }, (p) =>
         {
            SaveDatabaseConnectConfig();
            StepByStepWindow stepByStepWindow = new StepByStepWindow()
            {
               DataContext = new StepByStepWindowViewModel(),
            };
            stepByStepWindow.ShowDialog();
         });

         BySQLCommand = new RelayCommand<object>((p) => { return !string.IsNullOrEmpty(DatabaseName) && DatabaseUtils.TestConnection(out var connectError); }, (p) =>
         {
            SaveDatabaseConnectConfig();
            ReportFromQueryWindow reportFromQueryWindow = new ReportFromQueryWindow()
            {
               DataContext = new ReportFromQueryWindowViewModel(),
            };
            reportFromQueryWindow.ShowDialog();
         });
      }

      private void LoadDatabaseConnectConfig()
      {
         IniFile iniFile = new IniFile(Cons.GetDataDirectory + "Config.ini", "DatabaseConnectConfig");

         Server = iniFile.Read(nameof(Server));
         Port = iniFile.Read(nameof(Port));
         Username = iniFile.Read(nameof(Username));
         Password = iniFile.Read(nameof(Password));

         #region Set default value if null or empty
         Server = string.IsNullOrEmpty(Server) ? "localhost" : Server;
         Port = string.IsNullOrEmpty(Port) ? "3306" : Port;
         Username = string.IsNullOrEmpty(Username) ? "root" : Username;
         Password = string.IsNullOrEmpty(Password) ? "" : Password;
         #endregion
      }

      private void SaveDatabaseConnectConfig()
      {
         IniFile iniFile = new IniFile(Cons.GetDataDirectory + "Config.ini", "DatabaseConnectConfig");

         iniFile.Write(nameof(Server), Server);
         iniFile.Write(nameof(Port), Port);
         iniFile.Write(nameof(Username), Username);
         iniFile.Write(nameof(Password), Password);
      }

      private string server, port, username, password, databaseName;
      private ObservableCollection<string> listDatabaseNames = new ObservableCollection<string>();
   }
}
