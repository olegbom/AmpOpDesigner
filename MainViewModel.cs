using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using AmpOpDesigner.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AmpOpDesigner
{
    public class MainViewModel:INotifyPropertyChanged
    {

        public DifferentialVoltage Supply { get; private set; } = new DifferentialVoltage(0,10);
        public DifferentialVoltage Input { get; private set; } = new DifferentialVoltage(2.635,7.365);
        public double Voltage2 { get; set; } = 10;
        public DifferentialVoltage Output { get; private set; } = new DifferentialVoltage(0.5,3.2);

        public ObservableCollection<SchemeSolution> Solutions { get; }= new ObservableCollection<SchemeSolution>();
        public SchemeSolution IdealSolution { get; private set; }
        public SchemeSolution SelectedSolution { get; set; }

        public void OnSelectedSolutionChanged()
        {
            TolerancePlotViewModel.Series.Clear();

            if (SelectedSolution == null)
            {
                TolerancePlotViewModel.InvalidatePlot(true);
                return;
            }
            
          
            var areaSeries1 = new AreaSeries();
            areaSeries1.DataFieldX2 = "Uin";
            areaSeries1.DataFieldY2 = "Uout Minimum";
            areaSeries1.DataFieldX = "Uin";
            areaSeries1.DataFieldY = "Uout Maximum";
            areaSeries1.Title = "Maximum/Minimum";
            areaSeries1.Fill = OxyColors.LightBlue;
            areaSeries1.MarkerFill = OxyColors.Transparent;
            areaSeries1.StrokeThickness = 0;
            areaSeries1.Points.Add(new DataPoint(Input.Negative, SelectedSolution.UOutExtremePositive(Input.Negative, Voltage2)));
            areaSeries1.Points.Add(new DataPoint(Input.Positive, SelectedSolution.UOutExtremePositive(Input.Positive, Voltage2)));
            areaSeries1.Points2.Add(new DataPoint(Input.Negative, SelectedSolution.UOutExtremeNegative(Input.Negative, Voltage2)));
            areaSeries1.Points2.Add(new DataPoint(Input.Positive, SelectedSolution.UOutExtremeNegative(Input.Positive, Voltage2)));
            TolerancePlotViewModel.Series.Add(areaSeries1);
            
            var lineSeriesSelected = new LineSeries();
            lineSeriesSelected.Title = "Selected";

            double uOut1 = Input.Negative * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1;
            double uOut2 = Input.Positive * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1;

            lineSeriesSelected.Points.Add(new DataPoint(Input.Negative, uOut1));
            lineSeriesSelected.Points.Add(new DataPoint(Input.Positive, uOut2));
            TolerancePlotViewModel.Series.Add(lineSeriesSelected);

            var lineSeriesIdeal = new LineSeries();
            lineSeriesIdeal.Title = "Ideal";

            uOut1 = Input.Negative * IdealSolution.K2 - Voltage2 * IdealSolution.K1;
            uOut2 = Input.Positive * IdealSolution.K2 - Voltage2 * IdealSolution.K1;

            lineSeriesIdeal.Points.Add(new DataPoint(Input.Negative, uOut1));
            lineSeriesIdeal.Points.Add(new DataPoint(Input.Positive, uOut2));
            TolerancePlotViewModel.Series.Add(lineSeriesIdeal);
            



            TolerancePlotViewModel.InvalidatePlot(true);
        }


        public PlotModel TolerancePlotViewModel { get; private set; }


        public MainViewModel()
        {
            TolerancePlotViewModel = new PlotModel(){Title = "Передаточная функция"};
            var linearAxis1 = new LinearAxis();
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.AbsoluteMaximum = Input.Positive * 1.2;
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.Maximum = Input.Positive*1.2;
            linearAxis1.Minimum = 0;
            TolerancePlotViewModel.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            linearAxis2.AbsoluteMaximum = Output.Positive * 1.2;
            linearAxis2.AbsoluteMinimum = 0;
            linearAxis2.Maximum = Output.Positive * 1.2;
            linearAxis2.Minimum = 0;
            TolerancePlotViewModel.Axes.Add(linearAxis2);
        }


        #region StartCommand

        private RelayCommand _startCommand;
        public RelayCommand StartCommand => _startCommand ?? (_startCommand = new RelayCommand(Start));

        private void Start(object o)
        {
            Solutions.Clear();
            
            double dIn = Input.Delta;
            double dOut = Output.Delta;


            double k1 = (Input.Negative * dOut / dIn - Output.Negative)/ Voltage2;
            double k2 = dOut / dIn;
            double r3 = 10_000;
            double r6 = r3;
            double r4 = r3/k1;
            double r5 = (r3 + r4) * r6 / k2 / r4 - r6;

            IdealSolution = new SchemeSolution(){R3 = r3, R4=r4,R5=r5,R6=r6};
            List<SchemeSolution> solutions = new List<SchemeSolution>();

          
            for (int i = 0; i < Helper.E24.Count; i++)
            {
                r3 = Helper.E24[i] * 10_000;
                
                for (int j = 0; j < Helper.E24.Count; j++)
                {
                    r6 = Helper.E24[j] * 10_000;
                    r4 = r3 / k1;
                    
                    r5 = (r3 + r4) * r6 / k2 / r4 - r6;
                    //var solution = new SchemeSolution() { R3 = r3, R4 = Helper.Round(r4), R5 = Helper.Round(r5), R6 = r6 };
                   // solutions.Add(solution);
                    var solution = new SchemeSolution() { R3 = r3, R4 = Helper.RoundUp(r4), R5 = Helper.RoundUp(r5), R6 = r6 };
                    solutions.Add(solution);
                    solution = new SchemeSolution() { R3 = r3, R4 = Helper.RoundDown(r4), R5 = Helper.RoundUp(r5), R6 = r6 };
                    solutions.Add(solution);
                    solution = new SchemeSolution() { R3 = r3, R4 = Helper.RoundUp(r4), R5 = Helper.RoundDown(r5), R6 = r6 };
                    solutions.Add(solution);
                    solution = new SchemeSolution() { R3 = r3, R4 = Helper.RoundDown(r4), R5 = Helper.RoundDown(r5), R6 = r6 };
                    solutions.Add(solution);
                }
            } 
            var sorted = solutions.Where(s =>
            {
                var outNeg = s.UOut(Input.Negative, Voltage2);
                var outPos = s.UOut(Input.Positive, Voltage2);
                return outNeg > 0 && outPos > 0 && 
                       Math.Abs(outNeg - Output.Negative) < 0.2 &&
                       Math.Abs(outPos - Output.Positive) < 0.2;
            }).OrderBy(s => Math.Abs(s.UOut(Input.Negative, Voltage2) - Output.Negative) +
                                                Math.Abs(s.UOut(Input.Positive, Voltage2) - Output.Positive));

            foreach (var solution in sorted)
            {
                Solutions.Add(solution);
            }
            
            SelectedSolution = Solutions.First();
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
    public class SchemeSolution:INotifyPropertyChanged
    {
        public static readonly double Tolerance = 1;

        public static double MinPos(double x) => x * (100 - Tolerance) / 100;
        public static double MaxPos(double x) => x * (100 + Tolerance) / 100;


        public double R3 { get; set; } = 10000;
        public double R4 { get; set; } = 10000;
        public double R5 { get; set; } = 10000;
        public double R6 { get; set; } = 10000;

        public double K1 => CalcK1(R3, R4);

        public double K2 => CalcK2(R3,R4,R5,R6);

        public static double CalcK1(double r3, double r4)
        {
            return r3 / r4;
        }

        public static double CalcK2(double r3, double r4, double r5, double r6)
        {
            return (r3 + r4) * r6 / (r6 + r5) / r4;
        }


        public double UOutExtremePositive(double u2, double u1)
        {
            double r3 = MinPos(R3);
            double r4 = MaxPos(R4);
            double r5 = MinPos(R5);
            double r6 = MaxPos(R6);

            double k1 = CalcK1(r3, r4);
            double k2 = CalcK2(r3, r4, r5, r6);
            return u2 * k2 - u1 * k1;
        }


        public double UOutExtremeNegative(double u2, double u1)
        {
            double r3 = MaxPos(R3);
            double r4 = MinPos(R4);
            double r5 = MaxPos(R5);
            double r6 = MinPos(R6);

            double k1 = CalcK1(r3, r4);
            double k2 = CalcK2(r3, r4, r5, r6);
            return u2 * k2 - u1 * k1;
        }

        public double UOut(double u2, double u1) => u2 * K2 - u1 * K1;


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
