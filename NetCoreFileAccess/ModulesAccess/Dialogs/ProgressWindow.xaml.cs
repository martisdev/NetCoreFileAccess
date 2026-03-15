using MahApps.Metro.Controls;
using NetCoreFileAccess.SourceAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
