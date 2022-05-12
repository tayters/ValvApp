using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace ValvApp00
{
    public partial class CalTab : UserControl
    {
        private CalViewModel cvm;

        public CalTab()
        {
            InitializeComponent();
            cvm = new CalViewModel();
            this.DataContext = cvm;

            calGrid.ItemsSource = cvm.calReadings;
        }

        private void chan_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = chan_comboBox.SelectedIndex;
            if (cvm != null)
            {
                cvm.Cal_chan = c;
                UsbControl.SendString("sel" + (cvm.Cal_chan + 1).ToString());
            }
        }

        private void channel_test_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("ct1");
        }

        private void channel_test_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("ct0");
        }


        private void get_button_Click(object sender, RoutedEventArgs e)
        {
            CalSingle();
                        
        }

        private void adc_convert_checkbox_Toggle(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;

            if ((bool)cb.IsChecked)
                UsbControl.SendString("ac1");
            else
                UsbControl.SendString("ac0");

        }

        private void run_button_Click(object sender, RoutedEventArgs e)
        {
            //cvm.calReadings.Clear();
            CalRoutine();
        }

        private void CalSingle()
        {
            byte[] readBuffer = new byte[256];
            int bytesRead;

            UsbControl.SendString("cm1");   //cal mode
            UsbControl.SendString("sel" + (cvm.Cal_chan + 1).ToString());

            UsbControl.SendString("ol");
            UsbControl.Read(readBuffer, 2000, out bytesRead);
            var rd_str_arr = Encoding.Default.GetString(readBuffer).Split(" ", StringSplitOptions.None);

            for (int i = 0; i < 16; i++)
            {
                cvm.UpdateCalReadings(rd_str_arr[i], i);
            }

            //generate median
            cvm.UpdateMedian();
            cvm.AddCalReading();
            

            UsbControl.SendString("cm0");  //Exit cal mode
        }

        private void CalRoutine()
        {
            byte[] readBuffer = new byte[256];
            int bytesRead;

            UsbControl.SendString("cm1"); //Single channel mode
            UsbControl.SendString("sel" + (cvm.Cal_chan + 1).ToString());

            Thread.Sleep(1000);
            for (int d = 9; d <= 25; d++)
            {
                Thread.Sleep(1000);
                cvm.Cal_disp = d;

                Stepmm(d);

                Thread.Sleep(1000);
                /*
                if (MessageBox.Show("Set distance to: " + d.ToString() + "mm",
                    "Confirmation",
                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    break;
                */


                UsbControl.SendString("ol");
                UsbControl.Read(readBuffer, 2000, out bytesRead);
                var rd_str_arr = Encoding.Default.GetString(readBuffer).Split(" ", StringSplitOptions.None);
                
                for (int j = 0; j < 16; j++)
                {
                    cvm.UpdateCalReadings(rd_str_arr[j], j);
                    //cvm.UpdateCalReadings(data[d-9,1], j);
                }
                

                //Generate median
                cvm.UpdateMedian();
                cvm.AddCalReading();

                

            }

            cvm.FitCurve();

            UsbControl.SendString("cm0");
        }


        private void Stepmm(int disp)
        {
            var newpos = (disp-5) * 105;
            
            var stp = newpos - cvm.PositionSteps;

            if (stp > 0)
                UsbControl.SendString("st_r " + stp.ToString());
            else if (stp < 0)
                UsbControl.SendString("st_l " + (stp * -1).ToString());


            cvm.PositionSteps += stp;
            cvm.Positionmm = (((double)cvm.PositionSteps) / 105) + 5;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((bool)adc_convert_checkBox.IsChecked)
            {
                UsbControl.SendString("ac1");
            }
            else 
            {
                UsbControl.SendString("ac0");
            }
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            cvm.calReadings.Clear();
            cvm.trendSeries.Points.Clear();
            cvm.scatterSeries.Points.Clear();
            cvm.ScatModel.InvalidatePlot(true);
        }

        private void step_button_Click(object sender, RoutedEventArgs e)
        {
            if (cvm.Steps > 0)
                UsbControl.SendString("st_r " + cvm.Steps.ToString());
            else if (cvm.Steps < 0)
                UsbControl.SendString("st_l " + (cvm.Steps * -1).ToString());

            cvm.PositionSteps += cvm.Steps;
            cvm.Positionmm = (((double)cvm.PositionSteps) / 105)+5;
        }

        private void home_button_Click(object sender, RoutedEventArgs e)
        {
            if (cvm.PositionSteps > 0)
                UsbControl.SendString("st_l " + cvm.PositionSteps.ToString());
            else if (cvm.PositionSteps < 0)
                UsbControl.SendString("st_r " + (cvm.PositionSteps).ToString());

            cvm.PositionSteps = 0;
            cvm.Positionmm = 5;
        }

        private void zero_button_Click(object sender, RoutedEventArgs e)
        {
            cvm.PositionSteps = 0;
            cvm.Positionmm = 5;
        }

        private void write_button_Click(object sender, RoutedEventArgs e)
        {
            UsbControl.SendString("cal");
        }


    }
}
