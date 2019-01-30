using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.FuelCalculator
{
    public class FuelModel : ObservableObject
    {
        private float _inTank;
        public float InTank
        {
            get
            {
                return this._inTank;
            }

            set
            {
                if (value != this._inTank)
                {
                    this._inTank = value;
                    this.OnPropertyChanged("InTank");
                }
            }
        }

        private float _perLap;
        public float PerLap
        {
            get
            {
                return this._perLap;
            }

            set
            {
                if (value != _perLap)
                {
                    value = _perLap;
                    this.OnPropertyChanged("PerLap");
                }
            }
        }

        private float _perHour;
        public float PerHour
        {
            get
            {
                return this._perHour;
            }

            set
            {
                if (value != this._perHour)
                {
                    this._perHour = value;
                    this.OnPropertyChanged("PerHour");
                }
            }
        }
    }
}
