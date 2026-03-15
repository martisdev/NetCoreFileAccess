using NetCoreFileAccess.ModulesAccess.Dialogs;
using NetCoreFileAccess.SourceAccess;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NetCoreFileAccess
{
    public class ProgressInfo :IDisposable
    {
        #region FIELDS

        private ProgressWindow? _progressWindow;
        private readonly string _title;                

        #endregion

        public ProgressInfo(string title)
        {
            _title = title ?? string.Empty;
            _progressWindow = new ProgressWindow(_title);
            
        }

        /// <summary>
        /// Synchronous convenience wrapper that invokes ShowAsync on the UI thread.
        /// </summary>
        public void Show()
        {       
            if(_progressWindow != null)
                _progressWindow.Show();
        }

        /// <summary>
        /// Synchronous wrapper for UpdateMessage (keeps existing API).
        /// </summary>
        public void UpdateMessage(string message)
        {
            if (_progressWindow != null)
                _progressWindow.SetMessage(message);
        }
       
        /// <summary>
        /// Legacy synchronous wrapper (previous code used Thread.Sleep).
        /// </summary>
        public void ErrorMessage(string message)
        {
            if (_progressWindow != null)
                _progressWindow.ErrorMessage(message);
        }

        /// <summary>
        /// IDisposable synchronous close (keeps compatibility).
        /// </summary>
        public void Dispose()
        {
            if (_progressWindow != null)
            {
                if (_progressWindow.IsVisible)
                    _progressWindow.Close();

                _progressWindow = null;                
            }
            GC.SuppressFinalize(this);
        }        
    }
}
