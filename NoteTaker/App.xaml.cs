using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Threading;
using System;

namespace Scrivener
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected static NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();
        static App()
        {
            DispatcherHelper.Initialize();
            //Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        public App()
            : base()
        {
            //this.Dispatcher.UnhandledException += Current_DispatcherUnhandledException;
            //Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //this.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            //Dispatcher.UnhandledException += DispatcherOnUnhandledException;
        }

        //private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        //{
        //    MessageBox.Show("TEST 3");
        //}

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            nlog.Fatal((ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
            MessageBox.Show(ex.Message, "FATAL ERROR: Program will terminate. Uncaught Thread Exception.",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Model.ExceptionReporting.Email(ex);
            }
            Application.Current.Shutdown();
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {            
            nlog.Fatal(e.Exception.Message);
            MessageBox.Show(e.Exception.Message, "FATAL ERROR: Uncaught Thread Exception Program MAY terminate.",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Model.ExceptionReporting.Email(e.Exception);
            }
            e.Handled = true;
        }


        //void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
            
        //    Exception ex = e.ExceptionObject as Exception;
        //    nlog.Fatal((ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
        //    MessageBox.Show(ex.Message, "FATAL ERROR: Uncaught Thread Exception",
        //                    MessageBoxButton.OK, MessageBoxImage.Error);
        //    Model.ExceptionReporting.Email(ex);
        //}


    }
}
    

