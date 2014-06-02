using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NoteConnector
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //Error if no arguments are supplied
                MessageBox.Show("No arguments supplied.");
            }
            else if (args.Length > 1)
            {
                MessageBox.Show("too many arguments supplied.");
            }
            else
            {
                string notespath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Notes\";
                bool isExists = System.IO.Directory.Exists(notespath);
                if (!isExists)
                    System.IO.Directory.CreateDirectory(notespath);

                string file = notespath + args[0] + @".txt";
                //MessageBox.Show(file);
                using (StreamWriter sw = File.CreateText(file))
                {
                    foreach (string i in args)
                        sw.WriteLine(i);
                }
            }
        }
    }
}
