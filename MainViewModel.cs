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

        public SchemeSolution SelectedSolution { get; set; }

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
           /* linearAxis2.AbsoluteMaximum = Output.Positive * 1.2;
            linearAxis2.AbsoluteMinimum = 0;
            linearAxis2.Maximum = Output.Positive * 1.2;
            linearAxis2.Minimum = 0;*/
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
            double r4 = r3/k1;
            double r6 = r3;
            double r5 = (r3 + r4) * r6 / k2 / r4 - r6;

            var solution = new SchemeSolution(){R3 = r3, R4=r4,R5=r5,R6=r6};
         
            Solutions.Add(solution);
            SelectedSolution = Solutions.Last();

            TolerancePlotViewModel.Series.Clear();

             var lineSeriesIdeal = new LineSeries();
            lineSeriesIdeal.Title = "Ideal";
            lineSeriesIdeal.Points.Add(new DataPoint(Input.Negative, Input.Negative * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1));
            lineSeriesIdeal.Points.Add(new DataPoint(Input.Positive, Input.Positive * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1));
            lineSeriesIdeal.MarkerSize = 4;
            lineSeriesIdeal.MarkerStroke = OxyColors.Black;
            lineSeriesIdeal.MarkerStrokeThickness = 1.5;
            lineSeriesIdeal.MarkerType = MarkerType.Circle;
           // TolerancePlotViewModel.Series.Add(lineSeriesIdeal);
            var lineSeriesMin = new LineSeries();
            lineSeriesMin.Title = "Min";
            lineSeriesMin.Points.Add(new DataPoint(Input.Negative, Input.Negative * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1 -
                                                                         Input.Negative * SelectedSolution.MinK2 + Voltage2 * SelectedSolution.MinK1));
            lineSeriesMin.Points.Add(new DataPoint(Input.Positive, Input.Positive * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1 -
                                                                     Input.Positive * SelectedSolution.MinK2 + Voltage2 * SelectedSolution.MinK1));
            lineSeriesMin.MarkerSize = 4;
            lineSeriesMin.MarkerStroke = OxyColors.Black;
            lineSeriesMin.MarkerStrokeThickness = 1.5;
            lineSeriesMin.MarkerType = MarkerType.Circle;
            TolerancePlotViewModel.Series.Add(lineSeriesMin);
            var lineSeriesMax = new LineSeries();
            lineSeriesMax.Title = "Max";
            lineSeriesMax.Points.Add(new DataPoint(Input.Negative, Input.Negative * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1 - 
                                                                   Input.Negative * SelectedSolution.MaxK2 + Voltage2 * SelectedSolution.MaxK1));
            lineSeriesMax.Points.Add(new DataPoint(Input.Positive, Input.Positive * SelectedSolution.K2 - Voltage2 * SelectedSolution.K1 -
                                                                        Input.Positive * SelectedSolution.MaxK2 + Voltage2 * SelectedSolution.MaxK1));
            lineSeriesMax.MarkerSize = 4;
            lineSeriesMax.MarkerStroke = OxyColors.Black;
            lineSeriesMax.MarkerStrokeThickness = 1.5;
            lineSeriesMax.MarkerType = MarkerType.Circle;
            TolerancePlotViewModel.Series.Add(lineSeriesMax);
            TolerancePlotViewModel.InvalidatePlot(true);
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

        public double MinK1 => CalcK1(MinPos(R3), MaxPos(R4));
        public double MaxK1 => CalcK1(MaxPos(R3), MinPos(R4));
        public double K2 => CalcK2(R3,R4,R5,R6);
        public double MinK2 => CalcK2(MinPos(R3), MaxPos(R4), MaxPos(R5), MinPos(R6));
        public double MaxK2 => CalcK2(MaxPos(R3), MinPos(R4), MinPos(R5), MaxPos(R6));

        public static double CalcK1(double r3, double r4)
        {
            return r3 / r4;
        }

        public static double CalcK2(double r3, double r4, double r5, double r6)
        {
            return (r3 + r4) * r6 / (r6 + r5) / r4;
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
