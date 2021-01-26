using MaterialDesignThemes.Wpf;
using ReportForIDS.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class MainWindowViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand PasswordChangedCommand { get; set; }
      public ICommand ShowPasswordCommand { get; set; }
      public ICommand LoadListDatabaseNameCommand { get; set; }
      public ICommand TestConnectionCommand { get; set; }
      public ICommand ViewRecentReportCommand { get; set; }
      public ICommand StepByStepCommand { get; set; }
      public ICommand BySQLCommand { get; set; }

      public PasswordBox PwbPassword { get; set; }
      public TextBox TxtPassword { get; set; }

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

            TxtPassword = p.FindName("txtPassword") as TextBox;
            PwbPassword = p.FindName("pwbPassword") as PasswordBox;

            PwbPassword.Password = Password;
         });

         PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return p != null; }, (p) => { Password = p.Password; });

         ShowPasswordCommand = new RelayCommand<PackIcon>((p) => { return p != null; }, (p) =>
         {
            if (PwbPassword.Visibility == Visibility.Visible)
            {
               p.Kind = PackIconKind.VisibilityOff;
               PwbPassword.Visibility = Visibility.Collapsed;
               TxtPassword.Visibility = Visibility.Visible;
            }
            else
            {
               p.Kind = PackIconKind.Visibility;
               PwbPassword.Visibility = Visibility.Visible;
               TxtPassword.Visibility = Visibility.Collapsed;
            }
            PwbPassword.Password = Password;
         });

         LoadListDatabaseNameCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            ListDatabaseNames = DatabaseUtils.GetListSchema();
         });

         TestConnectionCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            if (DatabaseUtils.TestConnection(out var connectError)) { CustomMessageBox.Show("Test connection succeeded", Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Information); }
            else { CustomMessageBox.Show("Cannot connect to database.\n\r" + connectError, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error); }
         });

         ViewRecentReportCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            //ListReportWindow listReportWindow = new ListReportWindow();
            //listReportWindow.Show();

            ListTemplateWindow templateWindow = new ListTemplateWindow();
            templateWindow.ShowDialog();
         });

         StepByStepCommand = new RelayCommand<object>((p) => { return !string.IsNullOrEmpty(DatabaseName) && DatabaseUtils.TestConnection(out var connectError); }, (p) =>
         {
            try
            {
               SaveDatabaseConnectConfig();
               StepByStepWindow stepByStepWindow = new StepByStepWindow();
               stepByStepWindow.ShowDialog();
            }
            catch (System.Exception e)
            {
               CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
         });

         BySQLCommand = new RelayCommand<object>((p) => { return !string.IsNullOrEmpty(DatabaseName) && DatabaseUtils.TestConnection(out var connectError); }, (p) =>
         {
            try
            {
               SaveDatabaseConnectConfig();
               ReportFromQueryWindow reportFromQueryWindow = new ReportFromQueryWindow();
               reportFromQueryWindow.ShowDialog();
            }
            catch (System.Exception e)
            {
               CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

         #endregion Set default value if null or empty
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