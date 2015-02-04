using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Scrivener.View
{
    /// <summary>
    /// Interaction logic for logging.xaml
    /// </summary>
    public partial class logging : Window
    {
        public logging()
        {
            InitializeComponent();
            var test = (MemoryTarget)Helpers.LoggingHelper.ReturnTarget("memory");
            foreach (string log in test.Logs)
            {
                testbox.AppendText(log + "\n");
            }
        }
    }
}
