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
using System.Windows.Threading;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Windows.Forms;
using MahApps.Metro.Controls;

namespace Scrivener.View
{
    /// <summary>
    /// Interaction logic for Break_Clock.xaml
    /// </summary>
    public partial class Break_Clock : MetroWindow
    {
        public Break_Clock()
        {
            InitializeComponent();
            this.Left = Properties.Settings.Default.BreakClockLeftPosition;
            this.Top = Properties.Settings.Default.BreakClockTopPosition;
            ProfileCellNumber.Text = Properties.Settings.Default.CellNumber;
            ProfileCellProvider.Text = Properties.Settings.Default.CellProviderDropDown;
            BCProfileWarning.Text = Properties.Settings.Default.BreakWarning.ToString();
            if (Properties.Settings.Default.TxTWarning == "True")
            {
                EnableWarning.IsChecked = true;
            }

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                CurrentTime.Content = DateTime.Now.ToString("hh:mm:ss");
                TimeSpan x = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                if (BreakClockReturnTime.Content.ToString() != "")
                {
                    if (x.ToString() == Properties.Settings.Default.BreakReturnWarning.ToString())
                    {
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            if (Properties.Settings.Default.TxTWarning == "True")
                            {
                                // Create the Outlook application.
                                var oApp = new Outlook.Application();
                                // Create a new mail item.
                                var oMsg = oApp.CreateItem(Outlook.OlItemType.olMailItem);
                                // Add a recipient.
                                oMsg.To = Properties.Settings.Default.CellNumber + Properties.Settings.Default.CellProvider;
                                //oMsg.CC = "Jerry.Cain@ecotoh.org" + ";" + " dylan.batt@ecotoh.net";
                                //Subject line
                                oMsg.Subject = "";
                                //add the body of the email

                                if (Properties.Settings.Default.BreakWarning != 0)
                                {
                                    oMsg.Body = "You have " + Properties.Settings.Default.BreakWarning + " minute left on " + Properties.Settings.Default.BreakLunch + ".";
                                }
                                else
                                {
                                    oMsg.Body = "Please return from " + Properties.Settings.Default.BreakLunch;
                                }
                                

                                if (Properties.Settings.Default.BreakWarning != 0)
                                {
                                    oMsg.Send();
                                    Form f = new Form();
                                    f.TopMost = true;
                                    System.Windows.Forms.MessageBox.Show(f, "This is your " + Properties.Settings.Default.BreakWarning + " minute warning");
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                if (Properties.Settings.Default.BreakWarning == 0)
                                {

                                }
                                else
                                {
                                    Form f = new Form();
                                    f.TopMost = true;
                                    System.Windows.Forms.MessageBox.Show(f, "This is your " + Properties.Settings.Default.BreakWarning + " minute warning");
                                }
                            }
                        });
                    }
                }


                if (CurrentTime.ToString() == BreakClockReturnTime.ToString())
                {
                    System.Windows.Forms.MessageBox.Show(new Form() { TopMost = true }, "Please return from break.");
                    BreakClockReturnTime.Content = "";
                    Properties.Settings.Default.BreakReturnWarning = new TimeSpan(0, 0, 0);
                }
            }, this.Dispatcher);
        }

        // **************************** Expand / Collapse ****************************
        private void ExpandedCellProfile(object sender, RoutedEventArgs e)
        {
            BreakClock.Height = 320;
        }

        private void CollapsedCellProfile(object sender, RoutedEventArgs e)
        {
            BreakClock.Height = 169;
        }

        // **************************** Button ADD time ****************************
        public void AddBreakTime(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.TxTWarning == "True")
            {
                if (ProfileCellNumber.Text != "")
                {
                    if (_15min == sender)
                    {
                        TimeSpan BreakReturnTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute + 15, DateTime.Now.Second);
                        BreakClockReturnTime.Content = DateTime.Now.AddMinutes(15).ToString("hh:mm:ss");
                        Properties.Settings.Default.BreakReturnWarning = BreakReturnTime + new TimeSpan(0, -(int.Parse(BCProfileWarning.Text)), 0);
                        Properties.Settings.Default.BreakLunch = "break";
                    }
                    else if (_30min == sender)
                    {
                        TimeSpan BreakReturnTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute + 30, DateTime.Now.Second);
                        BreakClockReturnTime.Content = DateTime.Now.AddMinutes(30).ToString("hh:mm:ss");
                        Properties.Settings.Default.BreakReturnWarning = BreakReturnTime + new TimeSpan(0, -(int.Parse(BCProfileWarning.Text)), 0);
                        Properties.Settings.Default.BreakLunch = "lunch";
                    }
                    else if (_1hr == sender)
                    {
                        TimeSpan BreakReturnTime = new TimeSpan(DateTime.Now.Hour + 1, DateTime.Now.Minute, DateTime.Now.Second);
                        BreakClockReturnTime.Content = DateTime.Now.AddHours(1).ToString("hh:mm:ss");
                        Properties.Settings.Default.BreakReturnWarning = BreakReturnTime + new TimeSpan(0, -(int.Parse(BCProfileWarning.Text)), 0);
                        Properties.Settings.Default.BreakLunch = "lunch";
                    }
                }
                else
                {
                    Form f = new Form();
                    f.TopMost = true;
                    System.Windows.Forms.MessageBox.Show("Please fill out the cell profile.");
                }
            }
            else
            {
                if (_15min == sender)
                {
                    TimeSpan BreakReturnTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute + 15, DateTime.Now.Second);
                    BreakClockReturnTime.Content = DateTime.Now.AddMinutes(15).ToString("hh:mm:ss");
                    Properties.Settings.Default.BreakReturnWarning = BreakReturnTime + new TimeSpan(0, -(int.Parse(BCProfileWarning.Text)), 0);
                    Properties.Settings.Default.BreakLunch = "break";
                }
                else if (_30min == sender)
                {
                    TimeSpan BreakReturnTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute + 30, DateTime.Now.Second);
                    BreakClockReturnTime.Content = DateTime.Now.AddMinutes(30).ToString("hh:mm:ss");
                    Properties.Settings.Default.BreakReturnWarning = BreakReturnTime + new TimeSpan(0, -(int.Parse(BCProfileWarning.Text)), 0);
                    Properties.Settings.Default.BreakLunch = "lunch";
                }
                else if (_1hr == sender)
                {
                    TimeSpan BreakReturnTime = new TimeSpan(DateTime.Now.Hour + 1, DateTime.Now.Minute, DateTime.Now.Second);
                    BreakClockReturnTime.Content = DateTime.Now.AddHours(1).ToString("hh:mm:ss");
                    Properties.Settings.Default.BreakReturnWarning = BreakReturnTime + new TimeSpan(0, -(int.Parse(BCProfileWarning.Text)), 0);
                    Properties.Settings.Default.BreakLunch = "lunch";
                }
            }
        }

        // **************************** Clear button ****************************
        private void ClearReturnTime(object sender, RoutedEventArgs e)
        {
            BreakClockReturnTime.Content = "";
            Properties.Settings.Default.BreakReturnWarning = new TimeSpan(0, 0, 0);
        }

        // **************************** Profile ****************************
        private void ProfileCellNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            string[] split = ProfileCellNumber.Text.Split(new char[] { '-', '(', ')', ' ' });// remove all old format,if your phone number is like(001)123-456-789
            //ProfileCellNumber.Text = CleanInput(ProfileCellNumber.Text);
            StringBuilder sb = new StringBuilder();
            foreach (string s in split)
            {
                if (s.Trim() != "")
                {
                    sb.Append(s);
                }
            }
            if (ProfileCellNumber.Text != "" && sb.Length < 10 | sb.Length >= 11)
            {
                Form f = new Form();
                f.TopMost = true;
                System.Windows.Forms.MessageBox.Show("Please enter a valid phone number with area code.");
            }
            else
            {
                ProfileCellNumber.Text = sb.ToString();
            }
        }

        private void SaveProfile(object sender, RoutedEventArgs e)
        {
            if (ProfileCellNumber.Text.Length != 10 & EnableWarning.IsChecked == true)
            {
                Form f = new Form();
                f.TopMost = true;
                System.Windows.Forms.MessageBox.Show("Please enter a valid phone number with area code.");
            }
            else if (EnableWarning.IsChecked == false & ProfileCellNumber.Text != "" & ProfileCellNumber.Text.Length != 10)
            {
                Form f = new Form();
                f.TopMost = true;
                System.Windows.Forms.MessageBox.Show("Please enter a valid phone number with area code.");
            }
            else
            {
                Properties.Settings.Default.CellNumber = ProfileCellNumber.Text;
                Properties.Settings.Default.BreakWarning = int.Parse(BCProfileWarning.Text);
                Properties.Settings.Default.CellProviderDropDown = ProfileCellProvider.Text;

                // **************************** TxT warning ****************************
                if (EnableWarning.IsChecked == true)
                {
                    Properties.Settings.Default.TxTWarning = "True";
                }
                else
                {
                    Properties.Settings.Default.TxTWarning = "False";
                }

                // **************************** Cel Provider ****************************
                if (ProfileCellProvider.Text == "Alltel")
                {
                    Properties.Settings.Default.CellProvider = "@message.alltel.com";
                }
                else if (ProfileCellProvider.Text == "ATT")
                {
                    Properties.Settings.Default.CellProvider = "@txt.att.net";
                }
                else if (ProfileCellProvider.Text == "Boost Mobile")
                {
                    Properties.Settings.Default.CellProvider = "@myboostmobile.com";
                }
                else if (ProfileCellProvider.Text == "Sprint")
                {
                    Properties.Settings.Default.CellProvider = "@messaging.sprintpcs.com";
                }
                else if (ProfileCellProvider.Text == "Verizon")
                {
                    Properties.Settings.Default.CellProvider = "@vtext.com";
                }
                else if (ProfileCellProvider.Text == "Virgin Mobile")
                {
                    Properties.Settings.Default.CellProvider = "@vmobl.com";
                }
                else if (ProfileCellProvider.Text == "T-Mobile")
                {
                    Properties.Settings.Default.CellProvider = "@tmomail.net";
                }

                Cell_Profile.IsExpanded = false;
                Properties.Settings.Default.Save();
            }
        }

        private void SaveLocation(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.BreakClockTopPosition = this.Top;
            Properties.Settings.Default.BreakClockLeftPosition = this.Left;
            Properties.Settings.Default.Save();
        }
    }
}
