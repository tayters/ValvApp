using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using MathNet;
using MathNet.Numerics;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Accord.Statistics;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Kernels;
using Accord.Math;
using Accord.Math.Optimization;

namespace ValvApp00
{
    public class CalViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool disposed;

        public PlotModel ScatModel { get; private set; }
        public ScatterSeries scatterSeries { get; private set; }
        public LineSeries trendSeries { get; private set; }

        public CalViewModel()
        {
            ScatModel = new PlotModel { Title = "Calibration Curve" };
            scatterSeries = new ScatterSeries { MarkerType = MarkerType.Diamond, MarkerFill = OxyColors.Blue };
            trendSeries = new LineSeries { };

            ScatModel.Series.Add(scatterSeries);

            ScatModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 2000 });
            ScatModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 8, Maximum = 30 });

            this.RaisePropertyChanged("ScatModel");
            this.ScatModel.InvalidatePlot(true);

            positionmm = 5;
            positionSteps = 0;
        }

        //Array to store last 16 adc readings
        public int[] Temp_cal_array { get; set; } = new int[16];

        //median of Tempcal_array - updated by GetMedian()
        private int temp_cal_median;
        public int Temp_cal_median
        {
            get { return temp_cal_median; }
            set
            {
                temp_cal_median = value;
                RaisePropertyChanged("Temp_cal_median");
            }
        }
                
        //Diplacement in mm
        public int Cal_disp { get; set; }

        //Channel
        private int cal_chan = 0;
        public int Cal_chan
        {
            get { return cal_chan; }
            set
            {
                cal_chan = value;
                Temp_cal_median = 0;
                RaisePropertyChanged("Temp_cal_median");
            }
        }

        //Class to hold median reading and disp
        public class CalRd : INotifyPropertyChanged
        {
            //Displacement
            private int disp;
            public int Disp
            {
                get { return this.disp; }
                set
                {
                    if (this.disp != value)
                    {
                        this.disp = value;
                        this.NotifyPropertyChanged("Disp");
                    }
                }
            }

            private int readings_median;
            public int Readings_median
            {
                get { return this.readings_median; }
                set
                {
                    if (this.readings_median != value)
                    {
                        this.readings_median = value;
                        this.NotifyPropertyChanged("Readings_median");
                    }
                }
            }

            //Constructor
            public CalRd()
            {

            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyPropertyChanged(string propName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        private string coefficients;
        public String Coefficients
        {
            get { return coefficients; }
            set 
            {
                coefficients = value;
                RaisePropertyChanged("Coefficients");
            }

        }

        private double rsquared;
        public double Rsquared
        {
            get { return rsquared; }
            set 
            {
                rsquared = value;
                RaisePropertyChanged("Rsquared");
            }
        }


        private int steps;
        public int Steps
        {
            get { return steps; }
            set
            {
                steps = value;
                RaisePropertyChanged("Steps");
            }
        }

        private int positionSteps;
        public int PositionSteps
        {
            get { return positionSteps; }
            set
            {
                positionSteps = value;
                RaisePropertyChanged("PositionSteps");
            }
        }

        private double positionmm;
        public double Positionmm
        {
            get { return positionmm; }
            set
            {
                positionmm = value;
                RaisePropertyChanged("Positionmm");
            }
        }


        public ObservableCollection<CalRd> calReadings = new ObservableCollection<CalRd>();

        internal void UpdateCalReadings(string a, int rep)
        {
            Temp_cal_array[rep] = int.Parse(a);
        }

        public void AddCalReading()
        {
            calReadings.Add(new CalRd() { Disp = Cal_disp, Readings_median = Temp_cal_median });
            scatterSeries.Points.Add(new ScatterPoint(Cal_disp, Temp_cal_median));
            this.ScatModel.InvalidatePlot(true);

        }

        public void FitCurve()
        {
            int len = calReadings.Count;
            double[] d = new double[len]; //Displacement 
            double[] v = new double[len]; //adc values

            double[,] data = new double[len,2];

            for (int i = 0; i < len; i++)
            {
                d[i] = (double)calReadings[i].Disp;
                v[i] = (double)calReadings[i].Readings_median;
                data[i, 0] = d[i];
                data[i, 1] = v[i];
            }
           

            double[][] inputs = data.GetColumn(0).ToJagged();
            double[] outputs = data.GetColumn(1);
                        

            var nls = new NonlinearLeastSquares()
            {
                NumberOfParameters = 3,

                // Initialize to some random values
                StartValues = new[] { 500000, (double) -1, 1000 },

                // Let's assume a quadratic model function: ax² + bx + c
                Function = (w, x) => w[0] * Math.Pow(x[0], w[1]) + w[2],

                // Derivative in respect to the weights:
                Gradient = (w, x, r) =>
                {
                    r[0] = Math.Pow(x[0], w[1]);                          // w.r.t a 
                    r[1] = Math.Log(x[0]) * w[0] * Math.Pow(x[0], w[1]);    // w.r.t b
                    r[2] = 1;                                             // w.r.t c 
                },

                Algorithm = new LevenbergMarquardt()
                {
                    MaxIterations = 1000,
                    Tolerance = 1e-5,
                }
            };


            var regression = nls.Learn(inputs, outputs);

            var c = regression.Coefficients;
            Coefficients = c[0].ToString("F0") + ", " + c[1].ToString("F4") + ", "+ c[2].ToString("F0");
            // Use the function to compute the input values
            double[] predict = regression.Transform(inputs);

            //How Good?
            List<double> trendList = new List<double>(predict.ToList());
            List<double> dataList = new List<double>(data.GetColumn(1).ToList());
            Rsquared = GoodnessOfFit.RSquared(trendList, dataList);


            for(int i = 0; i<predict.Length; i++)
            {
                trendSeries.Points.Add(new DataPoint(d[i], predict[i]));
            }

            if (ScatModel.Series.Count < 2)
            {
                ScatModel.Series.Add(trendSeries);
            }

            this.RaisePropertyChanged("ScatModel");
            this.ScatModel.InvalidatePlot(true);


        }

        public void UpdateMedian()
        {
            Temp_cal_median = GetMedian(Temp_cal_array);
        }

        public int GetMedian(int[] values)
        {
            Array.Sort(values);

            return values[values.Length / 2];
        }

        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //this.timer.Dispose();
                }
            }

            disposed = true;
        }
    }
}
