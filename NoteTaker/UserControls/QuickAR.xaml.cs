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
    /// Interaction logic for QuickAR.xaml
    /// </summary>
    public partial class QuickAR : UserControl
    {
        public QuickAR()
        {
            InitializeComponent();
        }

                private void Phoneregex(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CompileBF(object sender, RoutedEventArgs e)
        {
            String replaceitems = "Please replace: ";
            String bfdate = "";
            String bftime = "";
            ComboBoxItem starttime = StartTime.SelectedItem as ComboBoxItem;
            ComboBoxItem stoptime = StopTime.SelectedItem as ComboBoxItem;
            String bfshipment = "";
            String bfnumber = "";
            String bfaddy = "";
            String bfasset = "";
            String bfmodel = "";
            String bfserial = "";

            String noitem = "";
            String nodate = "";
            String nostarttime = "";
            String nostoptime = "";
            String noshipment = "";
            String noaddy = "";
            String nonumber = "";
            String noasset = "";
            String nomodel = "";
            String noserial = "";

            if (bflistbox.SelectedItems.Count != 0)
            {
                foreach (ListBoxItem selecteditems in bflistbox.SelectedItems)
                {
                    replaceitems += selecteditems.Content.ToString() + ", ";
                }
                int i = replaceitems.LastIndexOf(",");
                if (i != -1)
                {
                    replaceitems = replaceitems.Remove(i);
                }
                replaceitems += Environment.NewLine;
            }


            bool ideliver = false;
            bool asset = false;
            bool model = false;
            bool serial = false;

            if (replaceitems.Contains("Tower"))
            {
                ideliver = true;
                asset = true;
                model = true;
                serial = true;
            }

            else if (replaceitems.Contains("Monitor") || replaceitems.Contains("Printer"))
            {
                ideliver = true;
            }

            if (BFDate.SelectedDate != null && BFDate.IsEnabled == true)
            {
                DateTime testdate = BFDate.SelectedDate.Value.Date;
                bfdate = "Scheduled for: " + testdate.ToString("dddd - MM/dd/yyyy");
            }


            if (starttime.Content.ToString() != "Start Time" && stoptime.Content.ToString() != "Stop Time" && StartTime.IsEnabled == true && StopTime.IsEnabled == true)
            {
                bftime = " between " + starttime.Content.ToString() + " and " + stoptime.Content.ToString() + Environment.NewLine;
            }
            else
            {
                bftime = "";
            }


            if (BFShipment.Text != "" && BFShipment.IsEnabled == true)
            {
                bfshipment = "Shipment ID: " + BFShipment.Text + Environment.NewLine;
            }
            else
            {
                bfshipment = "";
            }


            if (BFNumber.Text != "" && BFNumber.IsEnabled == true)
            {
                bfnumber = "Best contact number: " + BFNumber.Text + Environment.NewLine;
            }
            else
            {
                bfnumber = "";
            }


            if (BFAddy.Text != "" && BFAddy.IsEnabled == true)
            {
                bfaddy = "Address: " + BFAddy.Text + Environment.NewLine;
            }
            else
            {
                bfaddy = "";
            }

            if (BFAsset.Text != "" && BFAsset.IsEnabled == true)
            {
                bfasset = "Asset: " + BFAsset.Text + Environment.NewLine;
            }
            else
            {
                bfasset = "";
            }

            if (BFModel.Text != "" && BFModel.IsEnabled == true)
            {
                bfmodel = "Model: " + BFModel.Text + Environment.NewLine;
            }
            else
            {
                bfmodel = "";
            }

            if (BFSerial.Text != "" && BFSerial.IsEnabled == true)
            {
                bfserial = "Serial: " + BFSerial.Text + Environment.NewLine;
            }
            else
            {
                bfserial = "";
            }




            //error checking
            if (bflistbox.SelectedItems.Count == 0)
            { 
                noitem = "Please select an item(s) to be replaced" + Environment.NewLine + Environment.NewLine; 
            }
            else
            {
                noitem = "";
            }

            if (BFDate.SelectedDate == null && ideliver == true)
            {
                nodate = "Please select ''Scheduled for'' date" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nodate = "";
            }
            if (starttime.Content.ToString() == "Start Time" && ideliver == true)
            {
                nostarttime = "Please select ''Start Time''" + Environment.NewLine + Environment.NewLine;
            }   
            else
            {
                nostarttime = "";
            }

            if (stoptime.Content.ToString() == "Stop Time" && ideliver == true)
            {
                nostoptime = "Please select ''Stop Time''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nostoptime = "";
            }

            if (BFShipment.Text == "" && ideliver == true)
            {
                noshipment = "Please include ''Shipment ID''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                noshipment = "";
            }

            if (BFAddy.Text == "")
            {
                noaddy = "Please include ''Address''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                noaddy = "";
            }

            if (BFNumber.Text == "")
            {
                nonumber = "Please include ''Phone Number''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nonumber = "";
            }

            if (BFAsset.Text == "" && asset == true)
            {
                noasset = "Please include ''Asset Tag''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                noasset = "";
            }

            if (BFModel.Text == "" && model == true)
            {
                nomodel = "Please include ''Model''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nomodel = "";
            }

            if (BFSerial.Text == "" && serial == true)
            {
                noserial = "Please include ''Serial Number''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                noserial = "";
            }


            if (bflistbox.SelectedItems.Count == 0 || BFAddy.Text == "" || BFNumber.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (ideliver == true && bfdate == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (ideliver == true && starttime.Content.ToString() == "Start Time")
                {
                    MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
                }
            else if (ideliver == true && stoptime.Content.ToString() == "Stop Time")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (ideliver == true && BFNumber.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (ideliver == true && BFAddy.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (asset == true && BFAsset.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (model == true && BFModel.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else if (serial == true && BFSerial.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noshipment + noaddy + nonumber + noasset + nomodel + noserial);
            }
            else
            {
                BFFinal.IsEnabled = true;
                BFAdd.IsEnabled = true;
                BFFinal.Text = replaceitems + bfmodel + bfasset + bfserial + bfaddy + bfnumber + bfshipment + bfdate + bftime;

                if (replaceitems.Contains("Tower") || replaceitems.Contains("Monitor") || replaceitems.Contains("Printer"))
                {
                    idelbutton.IsEnabled = true;
                }
            }
        }

        private void iDeliverEnableClick(object sender, RoutedEventArgs e)
        {
            iDeliverSelected();
        }

        private void iDeliverSelected()
        {
            if (TowerLBI.IsSelected == true)
            {
                DisablespeakerLI();
                DisableTabletChargerRouterLI();
                BFAddy.IsEnabled = true;
                BFAsset.IsEnabled = true;
                BFDate.IsEnabled = true;
                BFModel.IsEnabled = true;
                BFNumber.IsEnabled = true;
                BFSerial.IsEnabled = true;
                StartTime.IsEnabled = true;
                StopTime.IsEnabled = true;
                BFShipment.IsEnabled = true;
            }
            else if (MonitorLBI.IsSelected == true || PrinterLBI.IsSelected == true)
            {
                DisablespeakerLI();
                DisableTabletChargerRouterLI();
                BFAddy.IsEnabled = true;
                BFDate.IsEnabled = true;
                BFNumber.IsEnabled = true;
                StartTime.IsEnabled = true;
                StopTime.IsEnabled = true;
                BFShipment.IsEnabled = true;
            }
        }

        private void iDeliverDeselected(object sender, RoutedEventArgs e)
        {
            if (TowerLBI.IsSelected == false && MonitorLBI.IsSelected == false && PrinterLBI.IsSelected == false)
            {
                EnablespeakerLI();
                EnableTabletChargerRouterLI();
                BFAddy.IsEnabled = false;
                BFAsset.IsEnabled = false;
                BFDate.IsEnabled = false;
                BFModel.IsEnabled = false;
                BFNumber.IsEnabled = false;
                BFSerial.IsEnabled = false;
                StartTime.IsEnabled = false;
                StopTime.IsEnabled = false;
                BFShipment.IsEnabled = false;
            }           

            else if (TowerLBI.IsSelected == false)
            {
                BFAsset.IsEnabled = false;
                BFModel.IsEnabled = false;
                BFSerial.IsEnabled = false;
            }
            PeripheralSelected();
        }

        public void DisablespeakerLI()
        {
            SpeakersLBI.IsEnabled = false;
        }

        public void EnablespeakerLI()
        {
            SpeakersLBI.IsEnabled = true;
        }

        public void DisableTabletChargerRouterLI()
        {
            TabletLBI.IsEnabled = false;
            ChargerLBI.IsEnabled = false;
            RouterLBI.IsEnabled = false;
        }

        public void EnableTabletChargerRouterLI()
        {
            TabletLBI.IsEnabled = true;
            ChargerLBI.IsEnabled = true;
            RouterLBI.IsEnabled = true;
        }

        private void EnabledPeripheralClick(object sender, RoutedEventArgs e)
        {
            PeripheralSelected();
        }

        private void PeripheralSelected()
        {
            if (KeyboardLBI.IsSelected == true || MouseLBI.IsSelected == true || MousePadLBI.IsSelected == true || HeadsetLBI.IsSelected == true || SpeakersLBI.IsSelected == true || EthernetLBI.IsSelected == true || USBLBI.IsSelected == true || VGALBI.IsSelected == true || PowerLBI.IsSelected == true || TabletLBI.IsSelected == true || ChargerLBI.IsSelected == true || RouterLBI.IsSelected == true)
            {
                DisablespeakerLI();
                DisableTabletChargerRouterLI();
                BFAddy.IsEnabled = true;
                BFNumber.IsEnabled = true;
            }
        }

        private void PeripheralDeselected(object sender, RoutedEventArgs e)
        {
            if (KeyboardLBI.IsSelected == false && MouseLBI.IsSelected == false && MousePadLBI.IsSelected == false && HeadsetLBI.IsSelected == false && SpeakersLBI.IsSelected == false && EthernetLBI.IsSelected == false && USBLBI.IsSelected == false && VGALBI.IsSelected == false && PowerLBI.IsSelected == false && TabletLBI.IsSelected == false && ChargerLBI.IsSelected == false && RouterLBI.IsSelected == false)
            {
                EnablespeakerLI();
                EnableTabletChargerRouterLI();
                BFAddy.IsEnabled = false;
                BFNumber.IsEnabled = false;
            }
            iDeliverSelected();
        }

        private void SpeakerSelected(object sender, RoutedEventArgs e)
        {
            TowerLBI.IsEnabled = false;
            MonitorLBI.IsEnabled = false;
            PrinterLBI.IsEnabled = false;
            KeyboardLBI.IsEnabled = false;
            MouseLBI.IsEnabled = false;
            MousePadLBI.IsEnabled = false;
            HeadsetLBI.IsEnabled = false;
            EthernetLBI.IsEnabled = false;
            USBLBI.IsEnabled = false;
            VGALBI.IsEnabled = false;
            PowerLBI.IsEnabled = false;
            TabletLBI.IsEnabled = false;
            ChargerLBI.IsEnabled = false;
            RouterLBI.IsEnabled = false;
            BFAddy.IsEnabled = true;
            BFNumber.IsEnabled = true;
        }

        private void SpeakerDeselected(object sender, RoutedEventArgs e)
        {
            TowerLBI.IsEnabled = true;
            MonitorLBI.IsEnabled = true;
            PrinterLBI.IsEnabled = true;
            KeyboardLBI.IsEnabled = true;
            MouseLBI.IsEnabled = true;
            MousePadLBI.IsEnabled = true;
            HeadsetLBI.IsEnabled = true;            
            EthernetLBI.IsEnabled = true;
            USBLBI.IsEnabled = true;
            VGALBI.IsEnabled = true;
            PowerLBI.IsEnabled = true;
            TabletLBI.IsEnabled = true;
            ChargerLBI.IsEnabled = true;
            RouterLBI.IsEnabled = true;
            BFAddy.IsEnabled = false;
            BFNumber.IsEnabled = false;
        }

        private void TabletChargerRouterSelected(object sender, RoutedEventArgs e)
        {
            if (TabletLBI.IsSelected == true || ChargerLBI.IsSelected == true || RouterLBI.IsSelected == true)
            {
                TowerLBI.IsEnabled = false;
                MonitorLBI.IsEnabled = false;
                PrinterLBI.IsEnabled = false;
                KeyboardLBI.IsEnabled = false;
                MouseLBI.IsEnabled = false;
                MousePadLBI.IsEnabled = false;
                HeadsetLBI.IsEnabled = false;
                SpeakersLBI.IsEnabled = false;
                EthernetLBI.IsEnabled = false;
                USBLBI.IsEnabled = false;
                VGALBI.IsEnabled = false;
                PowerLBI.IsEnabled = false;
                BFAddy.IsEnabled = true;
                BFNumber.IsEnabled = true;
            }
        }

        private void TabletChargerRouterDeselected(object sender, RoutedEventArgs e)
        {
            if (TabletLBI.IsSelected == false && ChargerLBI.IsSelected == false && RouterLBI.IsSelected == false)
            {
                TowerLBI.IsEnabled = true;
                MonitorLBI.IsEnabled = true;
                PrinterLBI.IsEnabled = true;
                KeyboardLBI.IsEnabled = true;
                MouseLBI.IsEnabled = true;
                MousePadLBI.IsEnabled = true;
                HeadsetLBI.IsEnabled = true;
                SpeakersLBI.IsEnabled = true;
                EthernetLBI.IsEnabled = true;
                USBLBI.IsEnabled = true;
                VGALBI.IsEnabled = true;
                PowerLBI.IsEnabled = true;
                BFAddy.IsEnabled = false;
                BFNumber.IsEnabled = false;
            }
        }

        private void ResetBF(object sender, RoutedEventArgs e)
        {
            TowerLBI.IsEnabled = true;
            MonitorLBI.IsEnabled = true;
            PrinterLBI.IsEnabled = true;
            KeyboardLBI.IsEnabled = true;
            MouseLBI.IsEnabled = true;
            MousePadLBI.IsEnabled = true;
            HeadsetLBI.IsEnabled = true;
            SpeakersLBI.IsEnabled = true;
            EthernetLBI.IsEnabled = true;
            USBLBI.IsEnabled = true;
            VGALBI.IsEnabled = true;
            PowerLBI.IsEnabled = true;
            TabletLBI.IsEnabled = true;
            ChargerLBI.IsEnabled = true;
            RouterLBI.IsEnabled = true;

            TowerLBI.IsSelected = false;
            MonitorLBI.IsSelected = false;
            PrinterLBI.IsSelected = false;
            KeyboardLBI.IsSelected = false;
            MouseLBI.IsSelected = false;
            MousePadLBI.IsSelected = false;
            HeadsetLBI.IsSelected = false;
            SpeakersLBI.IsSelected = false;
            EthernetLBI.IsSelected = false;
            USBLBI.IsSelected = false;
            VGALBI.IsSelected = false;
            PowerLBI.IsSelected = false;
            TabletLBI.IsSelected = false;
            ChargerLBI.IsSelected = false;
            RouterLBI.IsSelected = false;

            BFAddy.IsEnabled = false;
            BFAsset.IsEnabled = false;
            BFDate.IsEnabled = false;
            BFModel.IsEnabled = false;
            BFNumber.IsEnabled = false;
            BFSerial.IsEnabled = false;
            StartTime.IsEnabled = false;
            StopTime.IsEnabled = false;
            BFShipment.IsEnabled = false;

            BFAddy.Text = "";
            BFAsset.Text = "";
            BFDate.Text = "";
            BFModel.Text = "";
            BFNumber.Text = "";
            BFSerial.Text = "";
            StartTime.SelectedItem = Startdefault;
            StopTime.SelectedItem = Stopdefault;
            BFShipment.Text = "";
            BFFinal.Text = "";
            BFFinal.IsEnabled = false;
            BFAdd.IsEnabled = false;
            idelbutton.IsEnabled = false;
        }

        public void idelnotes(object sender, RoutedEventArgs e)
        {
            string idelitems = "";
            string idelasset = "";
            string idelserial = "";

            if (bflistbox.SelectedItems.Count != 0)
            {
                foreach (ListBoxItem selecteditems in bflistbox.SelectedItems)
                {
                    idelitems += selecteditems.Content.ToString() + ", ";
                }
                int i = idelitems.LastIndexOf(",");
                if (i != -1)
                {
                    idelitems = idelitems.Remove(i);
                }                
            }

            if (BFAsset.Text != "" && BFAsset.IsEnabled == true)
            {
                idelasset = "Asset: " + BFAsset.Text;
            }            

            if (BFSerial.Text != "" && BFSerial.IsEnabled == true)
            {
                idelserial = "Serial: " + BFSerial.Text;
            }
             
            string idel = idelitems + idelasset + idelserial;
            Clipboard.SetDataObject(idel);
        }
    }
}
