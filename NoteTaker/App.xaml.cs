using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Threading;

namespace Scrivener
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
            //Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        public App()
            : base()
        {
            this.Dispatcher.UnhandledException += Current_DispatcherUnhandledException;
        }

        static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = false;
            }
            else
            {
                Model.ExceptionReporting.Email(e.Exception);
                ShowUnhandeledException(e);
            }

        }

        static void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string errorMessage = string.Format("An application error occurred and EDTECH has been emailed a error report.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",
            e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));
            
            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
    

