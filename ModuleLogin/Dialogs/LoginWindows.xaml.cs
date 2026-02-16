
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NetCoreFileAccess.SourceAccess;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NetCoreFileAccess
{
    /// <summary>
    /// Interaction logic for LoginWindows.xaml
    /// </summary>
    public partial class LoginWindows : MetroWindow
    {
        #region FIELDS
        
        string _OldInfo = string.Empty;
        bool _Inicializing = false;
        
        #endregion
        
        #region PROPERTIES
        public string user { get { return this.TxtName.Text; } }
        
        public string password { get { return this.txtPsw.Password; } }

        public bool Finalize { get; set;}    

        #endregion

        #region CONTRUCTOR
        public LoginWindows( bool Inicializing)
        {
            InitializeComponent();
            
            LoadICon();

            //Add textbox for check password
            if (Inicializing)
            {
                _Inicializing = true;
                var Pw = new PasswordBox();
                Pw.Name = "txtRepeadPsw";
                Pw.Width = 264;
                Pw.Height = 40;
                Pw.Margin = new Thickness(36,145,0,0);
                Pw.HorizontalAlignment = HorizontalAlignment.Left;
                Pw.VerticalAlignment = VerticalAlignment.Top;
                Pw.PasswordChanged += TxtPsw_PasswordChanged;                
                Pw.SetValue(TextBoxHelper.UseFloatingWatermarkProperty, true);
                Pw.SetValue(TextBoxHelper.WatermarkProperty, "Repead password");                
                Pw.Style = (Style)FindResource("MahApps.Styles.PasswordBox.Button.Revealed");
                
                MainGrid.Children.Add(Pw);
                this.btnOK.IsEnabled = false;
                LoginWin.Height = 370;
            }
        }

        #endregion

        #region EVENTS

        private void TxtPsw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox? txtRepeadPsw = sender as PasswordBox;
            if(txtRepeadPsw == null)
                return;

            bool OK= (this.txtPsw.Password.ToString() == txtRepeadPsw.Password.ToString());
            if (!OK)
                this.lblMessage.Text = "The passwords do not match";
            else
            {
                //check reg
                if(_Inicializing)
                    OK = CredentialsUtils.CheckPassword(this.txtPsw.Password.ToString());

                if (OK)
                    this.lblMessage.Text = _OldInfo;
                else
                    this.lblMessage.Text = "The password must be between 4 and 20 characters long and contain at least one number, one uppercase letter, one lowercase letter and at least one special character (@$!%*#?&) .";
            }

            this.btnOK.IsEnabled = OK;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {            
            base.DialogResult = true;
        }

        private void btnKO_Click(object sender, RoutedEventArgs e)
        {            
            base.DialogResult = false;
        }

        #endregion

        #region OVERRIDES
        
        public bool? ShowDialog(SourceType sourceType, string message)
        {
            if(this.Finalize)
            {
                // Ensure the window is shown/loaded so MahApps Metro dialog can attach to it.
                // Attach a Loaded handler that will display the message and then close the window.
                this.Loaded += async (s, e) =>
                {
                    // detach to avoid re-running
                    //this.Loaded -= null!;
                    await ShowFinalize(message);
                    this.Close();
                };

                // Show the window modally so the Loaded handler runs and the metro dialog can display.
                base.ShowDialog();
                return false;
            }
            //Customoze the dialog based on source type
            switch (sourceType)
            {
                case SourceType.Local:
                    this.Title = "Local Login";
                    this.TxtName.SetValue(TextBoxHelper.WatermarkProperty, "Local user Name");
                    this.txtPsw.SetValue(TextBoxHelper.WatermarkProperty, "Local password");                    
                    break;
                case SourceType.GoogleDrive:
                    this.Title = "Google Drive Login";
                    this.TxtName.SetValue(TextBoxHelper.WatermarkProperty, "Google User Name");
                    this.txtPsw.SetValue(TextBoxHelper.WatermarkProperty, "Google Password");
                    
                    break;
                case SourceType.Ftp:
                    this.Title = "FTP Login";
                    this.TxtName.SetValue(TextBoxHelper.WatermarkProperty, "FTP User Name");
                    this.txtPsw.SetValue(TextBoxHelper.WatermarkProperty, "FTP Password");                    
                    break;
            }
            
            _OldInfo = message;
            this.lblMessage.Text = message;
            return base.ShowDialog();
        }

        #endregion

        #region PRIVATE

        private async Task ShowFinalize(string message)
        {
            var mySettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Quit",
                AnimateShow = true,
                AnimateHide = false
            };

            await this.ShowMessageAsync("attention", message, MessageDialogStyle.Affirmative, mySettings);
        }

        private void LoadICon()
        {
            // If the icon is in Properties.Resources (resx) as a System.Drawing.Icon named "bunker":
            var resIcon = NetCoreFileAccess.Properties.Resources.Cedentials; // check Resources.resx for exact name
            if (resIcon != null)
            {
                using (var ms = new MemoryStream(resIcon))
                {
                    var decoder = new IconBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    this.Icon = decoder.Frames[0];
                }
            }
        }
        #endregion

    }
}
