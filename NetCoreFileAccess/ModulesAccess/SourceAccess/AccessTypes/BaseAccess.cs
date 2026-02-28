using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreFileAccess.SourceAccess
{
    public class BaseAccess : ISourceAccess
    {

        #region PROPERTIES        
        public string PathFile { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsInicializing { get; set; }
        #endregion

        #region CONSTRUCTORS
        public BaseAccess()
        {            
            UserName = string.Empty;
            Password = string.Empty;
            PathFile = string.Empty;
        }
        #endregion

        #region PUBLIC METHODS
        public virtual bool TryLogin(params object[] Options)
        {
            // Set the password pattern for validation in the login window
            CredentialsUtils.Pattern = Options != null && Options.Length > 0 && Options[0] is string pattern ? pattern : string.Empty; ;

            LoginWindows Login;
            bool? result;
            bool Finalize = false;
            string message = string.Empty;
            int attempt = 0;

            do
            {
                attempt++;
                Login = new LoginWindows(this.IsInicializing);
                Login.Finalize = Finalize;

                if (this.IsInicializing && Login.Finalize == false)
                    message = "Please enter your credentials to initialize the source access.";
                else if (message.Length == 0)
                    message = "Please enter your credentials to login.";

                result = Login.ShowDialog(SourceType.Local, message);

                if (Login.Finalize)
                    break;

                if (result == true)
                {
                    bool resultLogin = this.Login(Login.user, Login.password);
                    if (resultLogin == false)
                    {
                        message = string.Format("Login attempt {0} failed. Please enter your credentials to login again.", attempt);
                        result = false;
                    }
                    else
                    {                        
                        //Login successful, set the user and password for future use
                        UserName = Login.user;
                        Password = Login.password;
                    }
                }
                else
                {
                    message = "Login cancelled by user.";
                    Finalize = true;
                    result = false;
                    UserName = string.Empty;
                    Password = string.Empty;
                }

                if (attempt >= 3)
                {
                    message = "Maximum login attempts exceeded.";                    
                    Finalize = true;
                    result = false;
                    UserName = string.Empty;
                    Password = string.Empty;
                }
            }
            while ((bool)!result);

            return result ?? false;
        }

        #endregion

        #region PRIVATE METHODS
        protected virtual bool Login(string User, string Password)
        {
            return false;
        }
        #endregion

        public virtual bool SaveFile(MemoryStream content)
        {
            return false;
        }

        public virtual MemoryStream GetFileData()
        {
            return new MemoryStream();
        }

    }
}
