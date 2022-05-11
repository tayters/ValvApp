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

namespace ValvApp00
{
    public class PlotViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool disposed;
        private readonly Stopwatch watch = new Stopwatch();
        public DateTime datetime = new DateTime();
        public Plot[] PlotsA { get; set; }
        public Plot[] PlotsB { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public OxyColor[] plotColours = { OxyColors.SaddleBrown, OxyColors.Red, OxyColors.Orange, OxyColors.Yellow,
                                          OxyColors.Green, OxyColors.Blue, OxyColors.Purple, OxyColors.DimGray,
                                          OxyColors.SaddleBrown, OxyColors.Red, OxyColors.Orange, OxyColors.Yellow,
                                          OxyColors.Green, OxyColors.Blue, OxyColors.Purple, OxyColors.DimGray, }; 

        public PlotViewModel()
        {
            this.SetupModel();
            this.watch.Start();
        }
                
        public void SetupModel()
        {
            PlotsA = new Plot[7];
            PlotsB = new Plot[7];
            for (int i = 0; i < 7; i++)
            {
                PlotsA[i] = new Plot(i);
                PlotsB[i] = new Plot(i);
                string str1 = string.Format("PlotsA[{0}].PlotModel", i);
                string str2 = string.Format("PlotsB[{0}].PlotModel", i);
                this.RaisePropertyChanged(str1);
                this.RaisePropertyChanged(str2);
            }


            PlotsA[0].PlotModel.Title = "Activity for Previous Minute";
            PlotsB[0].PlotModel.Title = "Activity for Previous Hour";

            this.UpdateAll();
        }

        public class Plot
        {
            public PlotModel PlotModel { get; private set; }

            public Plot(int index)
            {
                PlotModel = new PlotModel();
                PlotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Minimum = 0,
                    Maximum = 30,
                    MinimumRange = 30,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineStyle = LineStyle.DashDotDot
                });
                PlotModel.Axes.Add(new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineStyle = LineStyle.DashDotDot
                });

                PlotModel.Series.Add(new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness=2, Color = plotColours[index] });
            }

            public OxyColor[] plotColours = { OxyColors.SaddleBrown, OxyColors.Red, OxyColors.Orange, OxyColors.Yellow,
                                          OxyColors.Green, OxyColors.Blue, OxyColors.Purple, OxyColors.DimGray,
                                          OxyColors.SaddleBrown, OxyColors.Red, OxyColors.Orange, OxyColors.Yellow,
                                          OxyColors.Green, OxyColors.Blue, OxyColors.Purple, OxyColors.DimGray, };
                
        
        }

        public void AddPoint(double d, int ser)
        {
            double t = (this.watch.ElapsedMilliseconds * 0.001);
            datetime = DateTime.Now;

            var sA = (LineSeries)PlotsA[ser].PlotModel.Series[0];
            sA.Points.Add(new DataPoint(DateTimeAxis.ToDouble(datetime), d));

            if (sA.Points.Count > 30)
                sA.Points.RemoveAt(0);

            var sB = (LineSeries)PlotsB[ser].PlotModel.Series[0];
            sB.Points.Add(new DataPoint(DateTimeAxis.ToDouble(datetime), d));

            if (sB.Points.Count > 1800)
                sB.Points.RemoveAt(0);

            this.UpdateAll();
        }

        public void ShowSeries()
        {
            for (int i = 0; i < 7; i++)
            {
                PlotsA[i].PlotModel.Series[0].IsVisible = true;
                PlotsB[i].PlotModel.Series[0].IsVisible = true;
            }
            this.UpdateAll();
        }

        public string[] Disp { get; } = new string[16];

        internal void UpdateDisp(double d, int ser)
        {
            Disp[ser] = d.ToString();
            RaisePropertyChanged("Disp");
        }

        public void UpdateAll()
        {
            for (int i = 0; i < 7; i++)
            {
                this.PlotsA[i].PlotModel.InvalidatePlot(true);
                this.PlotsB[i].PlotModel.InvalidatePlot(true);
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
