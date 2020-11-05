using System.ComponentModel;
using System.Runtime.CompilerServices;
using AmpOpDesigner.Annotations;

namespace AmpOpDesigner
{
    public class DifferentialVoltage: INotifyPropertyChanged 
    {
        public double Negative { get; set; }
        public double Positive { get; set; }

        public double Delta => Positive - Negative;
        public DifferentialVoltage()
        {
        }

        public DifferentialVoltage(double negative,double positive)
        {
            Positive = positive;
            Negative = negative;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}