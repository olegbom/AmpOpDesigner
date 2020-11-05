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
