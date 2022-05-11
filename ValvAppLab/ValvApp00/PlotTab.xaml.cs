using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Timers;

namespace ValvApp00
{
    public partial class PlotTab : UserControl
    {
        PlotViewModel pvm;
        private Timer plot_timer;

        public PlotTab()
        {
            InitializeComponent();
            pvm = new PlotViewModel();
            this.DataContext = pvm;

            plot_timer = new Timer(2000);
            plot_timer.Elapsed += new ElapsedEventHandler(OnPlotTimerElapsed);

            pvm.ShowSeries();
        }

        private void OnPlotTimerElapsed(object source, ElapsedEventArgs e)
        {
            byte[] readBuffer = new byte[256], rWord = new byte[5];
            double pnt;
            int bytesRead;

            UsbControl.SendString("ol");
            UsbControl.Read(readBuffer, 2000, out bytesRead);

            //Read the 16 values
            for (int i = 0; i < 7; i++)
            {
                Array.Copy(readBuffer, (6 * i + 1), rWord, 0, 5);
                if (rWord[0] >= 0x30)
                {
                    try
                    {
                        pnt = Math.Round(double.Parse(Encoding.ASCII.GetString(rWord)), 2);
                        pvm.AddPoint(pnt, i);
                        pvm.UpdateDisp(pnt, i);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("cm0");
            plot_timer.Start();
        }

        private void stop_button_Click(object sender, RoutedEventArgs e)
        {
            plot_timer.Stop();
            UsbControl.reader.Flush();
            UsbControl.writer.Flush();
        }

      
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            plot_timer.Stop();
            UsbControl.reader.Flush();
            UsbControl.writer.Flush();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("ac1");
        }
    }
}
