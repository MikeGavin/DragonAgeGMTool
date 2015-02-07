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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scrivener.UserControls
{
    /// <summary>
    /// Interaction logic for Note.xaml
    /// </summary>
    public partial class NoteViewUC : UserControl
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public NoteViewUC()
        {                       
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

    }
}
