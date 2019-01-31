using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.FuelCalculator
{
    public class FuelModel : ObservableObject, IEntityModel
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

        private float _totalUsed;
        public float TotalUsed
        {
            get
            {
                return this._totalUsed;
            }

            set
            {
                if (value != _totalUsed)
                {
                    _totalUsed = value;
                    OnPropertyChanged("TotalUsed");
                }
            }
        }

        private float _totalLapTime;
        public float TotalLapTime
        {
            get
            {
                return this._totalLapTime;
            }

            set
            {
                if (value != this._totalLapTime)
                {
                    this._totalLapTime = value;
                    OnPropertyChanged("TotalLapTime");
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
                    _perLap = value;
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

        private float _amountToAdd;
        public float AmountToAdd
        {
            get
            {
                return this._amountToAdd;
            }

            set
            {
                if (value != this._amountToAdd)
                {
                    this._amountToAdd = value;
                    this.OnPropertyChanged("AmountToAdd");
                }
            }
        }

        private float _sessionLength;
        public float SessionLength
        {
            get
            {
                return this._sessionLength;
            }

            set
            {
                if (value != this._sessionLength)
                {
                    this._sessionLength = value;
                    this.OnPropertyChanged("SessionLength");
                }
            }
        }

        private int _lapsCompleted;
        public int LapsCompleted
        {
            get
            {
                return this._lapsCompleted;
            }

            set
            {
                if (value != this._lapsCompleted)
                {
                    this._lapsCompleted = value;
                    this.OnPropertyChanged("LapsCompleted");
                }
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get
            {
                return this._enabled;
            }

            set
            {
                if (value != this._enabled)
                {
                    this._enabled = value;
                    this.OnPropertyChanged("Enabled");
                }
            }
        }

        private bool _units;
        public bool Units
        {
            get
            {
                return this._units;
            }

            set
            {
                if (value != this._units)
                {
                    this._units = value;
                    this.OnPropertyChanged("Units");
                }
            }
        }

        private bool _autoFuel;
        public bool AutoFuel
        {
            get
            {
                return this._autoFuel;
            }

            set
            {
                if (value != this._autoFuel)
                {
                    this._autoFuel = value;
                    this.OnPropertyChanged("AutoFuel");
                }
            }
        }

        private Guid _guid;
        public Guid Guid
        {
            get
            {
                if (this._guid == null)
                {
                    this._guid = Guid.NewGuid();
                }
                return this._guid;
            }
        }
    }
}
