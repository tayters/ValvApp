using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.DeviceNotify;

namespace ValvApp00
{
    public static class UsbControl
    {
        static int Vend_Id = 0x03EB;
        static int Prod_Id = 0x2423;

        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(Vend_Id, Prod_Id);
        public static UsbEndpointWriter writer;
        public static UsbEndpointReader reader;
        public static IDeviceNotifier MyUsbDeviceNotifier = new WindowsDeviceNotifier();
        //private static byte[] readBuffer = new byte[256], readBuffer_stats = new byte[32];
        //private static int bytesRead;

        static UsbControl()
        {
            
        }

        public static void SendString(string out_string)
        {
            UsbTransfer usbWriteTransfer;
            byte[] bytesToSend = Encoding.Default.GetBytes(out_string);
            ErrorCode ecWrite;
            int transferredOut;

            if (MyUsbDevice != null)
            {
                if (MyUsbDevice.IsOpen)
                {
                    try
                    {
                        ecWrite = writer.SubmitAsyncTransfer(bytesToSend, 0, bytesToSend.Length, 100, out usbWriteTransfer);
                        if (usbWriteTransfer != null)
                        {
                            ecWrite = usbWriteTransfer.Wait(out transferredOut);
                            usbWriteTransfer.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }
            }
        }

        public static void Read(byte[] readBuffer, int timeout, out int bytesRead)
        {
            reader.Read(readBuffer, timeout, out bytesRead);
        }



    }
}
