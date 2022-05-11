using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.DeviceNotify;

namespace ValvApp00
{
    public partial class ControlTab : UserControl
    {
        private ControlViewModel cvm;
        private byte[] readBuffer = new byte[512], readBuffer_stats = new byte[32];
        private int bytesRead;
        DateTime datetime = new DateTime();
        private System.Timers.Timer stat_timer;
        private bool deploy = false;

        public ControlTab()
        {
            
            InitializeComponent();
            cvm = new ControlViewModel();
            this.DataContext = cvm;

            UsbControl.MyUsbDeviceNotifier.Enabled = true;
            UsbControl.MyUsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;

            stat_timer = new System.Timers.Timer(1000);
            stat_timer.Elapsed += new ElapsedEventHandler(OnStatTimerElapsed);

            file_ListView.ItemsSource = cvm.Files;
        }

        private void scanbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UsbRegDeviceList devicelist = UsbDevice.AllDevices;
                if (devicelist.Find(UsbControl.MyUsbFinder) != null)
                {
                    devicelist.Find(UsbControl.MyUsbFinder).DeviceProperties.TryGetValue("FriendlyName", out object name);
                    cvm.AppendStatus("Found device: " + name);
                }
                else
                {
                    cvm.AppendStatus("No devices available.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void connectbutton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // Find and open the usb device.
                UsbControl.MyUsbDevice = UsbDevice.OpenUsbDevice(UsbControl.MyUsbFinder);

                if (UsbControl.MyUsbDevice == null) throw new Exception("No Device Available.");

                // Open read endpoint.
                UsbControl.reader = UsbControl.MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01, 256, EndpointType.Interrupt);

                // Open write endpoint.
                UsbControl.writer = UsbControl.MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Interrupt);

                if ((UsbControl.reader == null) || (UsbControl.writer == null)) throw new Exception("Error opening endpoints");

                cvm.AppendStatus("Device Connected: " + UsbControl.MyUsbDevice.Info.ProductString);


            }
            catch (Exception ex)
            {
                cvm.AppendStatus(ex.ToString());
            }

            
            if(UsbControl.MyUsbDevice != null)
            {
                if(deploy)
                    UsbControl.SendString("dpl1");

                //status box update
                stat_timer.Start();

            }



        }

        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            if (e.EventType == EventType.DeviceArrival)
            {
                cvm.AppendStatus("New device available");

            }
            else if (e.EventType == EventType.DeviceRemoveComplete)
            {
                if (UsbControl.MyUsbDevice != null)
                {
                    if (UsbControl.MyUsbDevice.IsOpen)
                    {
                        UsbControl.MyUsbDevice.Close();
                    }
                    UsbControl.MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();
                }
                stat_timer.Stop();
                cvm.AppendStatus("Device disconnected");
                cvm.Btime = "";
                cvm.Vbat = "";
                cvm.Files.Clear();
                


            }
        }

        private void sync_button_Click(object sender, RoutedEventArgs e)
        {
            datetime = DateTime.Now;
            string datestring = datetime.ToString("HHmmss-ddMMyy");
            UsbControl.SendString("tmst" + datestring);
        }

        private void refresh_button_Click(object sender, RoutedEventArgs e)
        {
          stat_timer.Stop();
          RefreshList();
          stat_timer.Start();
        }

        private void RefreshList()
        {
            byte[] listBuffer = new byte[64];

            cvm.Files.Clear();

            UsbControl.reader.Flush();
            Array.Clear(listBuffer, 0, listBuffer.Length);
            UsbControl.SendString("list");
            while (true)
            {
                UsbControl.reader.Read(listBuffer, 1000, out bytesRead);

                if (bytesRead > 0)
                {
                    cvm.AppendStatus(Encoding.Default.GetString(listBuffer, 0, bytesRead));
                    var listString = System.Text.Encoding.ASCII.GetString(listBuffer).Trim('\0');
                    cvm.AddFile(listString);

                }
                else
                    break;
            }

        }

        private void led_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("led");
        }

        private void startlog_button_Click(object sender, RoutedEventArgs e)
        {
            stat_timer.Stop();
            UsbControl.SendString("start");
            cvm.AppendStatus("Logging started...");
        }

        private void stoplog_button_Click(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("stop");
            cvm.AppendStatus("Logging stopped...");
            stat_timer.Start();
        }

        

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            statusBox.ScrollToEnd();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (UsbControl.MyUsbDevice != null)
            {
                UsbControl.reader.Flush();
                UsbControl.writer.Flush();
                stat_timer.Start();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            stat_timer.Stop();
        }

        private void format_button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Format SD card",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                UsbControl.SendString("format");
            }
        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
            string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ("data\\" + cvm.SelectedFile.name));
            string out_str = "rd " + cvm.SelectedFile.name;
            int index = 0;

            stat_timer.Stop();
            cvm.AppendStatus("Downloading to " + docPath);

            using (FileStream fs = new FileStream(docPath, FileMode.OpenOrCreate,FileAccess.Write))
            {
                UsbControl.SendString(out_str);
            
                do
                {
                    UsbControl.reader.Read(readBuffer, 1000, out bytesRead);
                    index = Array.IndexOf(readBuffer, (byte)0x00);

                    if (index >= 0)
                        fs.Write(readBuffer, 0, index);
                    else
                        fs.Write(readBuffer, 0, readBuffer.Length);
                                                            
                    Array.Clear(readBuffer, 0, readBuffer.Length);
                    
                } while (bytesRead > 0);
            }

            cvm.AppendStatus("Done");
            UsbControl.reader.Flush();
            stat_timer.Start();
        }

        private void delete_menuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(("Are you sure you want to delete "+ cvm.SelectedFile.name+"?"), 
                "Delete File ",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                UsbControl.SendString("de " + cvm.SelectedFile.name);
                cvm.Files.Remove(cvm.SelectedFile);
            }
           
        }

        private void deploy_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            stat_timer.Stop();
            deploy = true;
            UsbControl.SendString("dpl1");
            cvm.AppendStatus("Ready to deploy...");
            stat_timer.Start();
        }

        private void deploy_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            stat_timer.Stop();
            deploy = false;
            UsbControl.SendString("dpl0");
            cvm.AppendStatus("Deployment cancelled.");
            stat_timer.Start();
        }

        private void OnStatTimerElapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                if (UsbControl.reader != null)
                {
                    
                    Array.Clear(readBuffer_stats, 0, readBuffer_stats.Length);
                    UsbControl.reader.Flush();
                    UsbControl.SendString("vb");

                    UsbControl.Read(readBuffer_stats, 2000, out bytesRead);

                    if (bytesRead > 0)
                        cvm.Vbat = "Battery Voltage: " + Encoding.Default.GetString(readBuffer_stats, 0, bytesRead - 4) + "V";

                    Array.Clear(readBuffer_stats, 0, readBuffer_stats.Length);
                    UsbControl.SendString("tmgt");
                    UsbControl.Read(readBuffer_stats, 2000, out bytesRead);

                    if (bytesRead > 0)
                        cvm.Btime = "SysTime: " + Encoding.Default.GetString(readBuffer_stats, 0, bytesRead);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
    }
}
