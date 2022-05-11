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
        public PlotModel PlotModel { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private OxyColor[] plotColours = { OxyColors.SaddleBrown, OxyColors.Red, OxyColors.Orange, OxyColors.Yellow,
                                          OxyColors.Green, OxyColors.Blue, OxyColors.Purple, OxyColors.DimGray,
                                          OxyColors.SaddleBrown, OxyColors.Red, OxyColors.Orange, OxyColors.Yellow,
                                          OxyColors.Green, OxyColors.Blue, OxyColors.Purple, OxyColors.DimGray, }; 

        public PlotViewModel()
        {
            
            this.SetupModel();
            this.watch.Start();
        }

        private void SetupModel()
        {
            PlotModel = new PlotModel();
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 45,
                MinimumRange = 45,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineStyle = LineStyle.DashDotDot
            });
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineStyle = LineStyle.DashDotDot
            });

            for (int i = 0; i < 16; i++)
            {
                if (i < 8)
                    PlotModel.Series.Add(new LineSeries { LineStyle = LineStyle.Solid, Color = plotColours[i] });
                else
                    PlotModel.Series.Add(new LineSeries { LineStyle = LineStyle.Dash, Color = plotColours[i] });
            }

            this.RaisePropertyChanged("PlotModel");
            this.PlotModel.InvalidatePlot(true);
        }

        public void AddPoint(double d, int ser)
        {
            double t = (this.watch.ElapsedMilliseconds * 0.001);
            var s = (LineSeries)PlotModel.Series[ser];
            s.Points.Add(new DataPoint(t, d));

            this.PlotModel.InvalidatePlot(true);
        }

        public void HideSeries(int ser)
        {
            PlotModel.Series[ser].IsVisible = false;
            this.PlotModel.InvalidatePlot(true);
        }

        public void ShowSeries(int ser)
        {
            PlotModel.Series[ser].IsVisible = true;
            this.PlotModel.InvalidatePlot(true);
        }

        public string[] Disp { get; } = new string[16];

        internal void UpdateDisp(double d, int ser)
        {
            Disp[ser] = d.ToString();
            RaisePropertyChanged("Disp");
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
