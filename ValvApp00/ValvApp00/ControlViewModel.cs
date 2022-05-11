using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Markup;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using MathNet.Numerics;
using System.IO;
using System.Data;

namespace ValvApp00
{
    public class ControlViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Stopwatch watch = new Stopwatch();
        public PlotModel PlotModel { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private bool disposed;

        public ObservableCollection<SdFile> Files;

        private SdFile selectedFile;
        public SdFile SelectedFile 
        {
            get { return selectedFile; }
            set 
            {
                selectedFile = value;
                RaisePropertyChanged("SelectedFile");
            }
        }
      

        public ControlViewModel() 
        {
            this.watch.Start();
            Files = new ObservableCollection<SdFile>();
            SdFile SelectedFile = new SdFile(0x00A1, 0x00A1);
        }

        public string Stat_str { get; set; } = String.Empty;

        internal void AppendStatus(string text)
        {
            Stat_str += DateTime.Now.ToLongTimeString() + " - " + text + "\n";
            RaisePropertyChanged("Stat_str");
        }

        private string btime = String.Empty;
        public string Btime
        {
            get { return btime; }
            set
            {
                btime = value;
                RaisePropertyChanged("Btime");
            }
        }

        private string vbat = String.Empty;
        public string Vbat
        {
            get { return vbat; }
            set
            {
                vbat = value;
                RaisePropertyChanged("Vbat");
            }
        }

        internal void AddFile(string fileinfo)
        {
            var sr = new StringReader(fileinfo);
            string[] n = new string[5];
            n = fileinfo.Split(" ", 5, 0);
            Files.Add(new SdFile(Int32.Parse(n[2]), Int32.Parse(n[3])) { name = n[0], size = Int32.Parse(n[1]) });

            /*
            string l;
            string[] n = new string[64];

            while ((l=sr.ReadLine()) != null)
            {
                if (l.Length > 10)
                {
                    n = l.Split(" ", 5, 0);
                    Files.Add(new SdFile(Int32.Parse(n[2]), Int32.Parse(n[3])) { name = n[0], size = Int32.Parse(n[1])});
                }
            }
            */
        }




        public class SdFile 
        { 
            public string name { get; set; }
            public int size { get; set; }
            //public int date { get; set; }
            //public int time { get; set; }

            public DateTime dateTime { get; set; }

            public SdFile(Int32 date, Int32 time)
            {
                if ((date != 0) && (time != 0))
                { 
                var h = ((0xF800 & time) >> 11);
                var m = ((0x07E0 & time) >> 5);
                var s = (0x001F & time);
                dateTime = new DateTime((((0xFE00 & date) >> 9) + 1980), //Year
                                        ((0x01E0 & date) >> 5),          //Month
                                        (0x001F & date),             //Day
                                        h,
                                        m,
                                        s);
                }
            }
        }

        protected void RaisePropertyChanged(string property)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //this.timer.Dispose();
                }
            }

            this.disposed = true;
        }

    }
}
