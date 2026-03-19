using MahApps.Metro.Controls;
using System.Windows;

namespace NetCoreFileAccess.ModulesAccess.Dialogs
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : MetroWindow
    {
 
        public ProgressWindow(string Title)
        {
            InitializeComponent();
            this.Title = Title;         
        }

        
        public void SetMessage(string message)
        {
            this.LbProgessInfo.Content = message;
            
        }

        public void ErrorMessage(string message)
        {
            this.LbProgessInfo.Content = message;
            Progress.Visibility= Visibility.Hidden;             
        }

    }
}
