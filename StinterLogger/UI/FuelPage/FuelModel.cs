using StinterLogger.RaceLogging.Iracing.Fuel;
using StinterLogger.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.UI.FuelPage
{
    public class FuelModel : ObservableObject
    {
        public FuelModel()
        { }

        public FuelModel(FuelData fuelData)
        {
            this.InTank = fuelData.FuelInTank;

            this.LapsCompleted = fuelData.LapsCompleted;

            this.LapsRemaining = fuelData.LapsRemaining;

            this.PerLap = fuelData.FuelUsagePerLap;

            this.TotalLapTime = fuelData.TotalRaceTime;

            this.TotalUsed = fuelData.TotalFuelUsed;

            this.AmountToAdd = fuelData.FuelToFinish;

            this.RemainingSessionTime = fuelData.RemainingRacetime;

            this.Liters = fuelData.Unit == FuelUnit.Liters ? true : false;

            this.Gallons = !this.Liters;
        }

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

        private float _remainingSessionTime;
        public float RemainingSessionTime
        {
            get
            {
                return this._remainingSessionTime;
            }

            set
            {
                if (value != this._remainingSessionTime)
                {
                    this._remainingSessionTime = value;
                    this.OnPropertyChanged("RemainingSessionTime");
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

        private int _lapsRemaining;
        public int LapsRemaining
        {
            get
            {
                return this._lapsRemaining;
            }

            set
            {
                if (value != this._lapsRemaining)
                {
                    this._lapsRemaining = value;
                    this.OnPropertyChanged("LapsRemaining");
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

        private bool _liters;
        public bool Liters
        {
            get
            {
                return this._liters;
            }

            set
            {
                if (value != this._liters)
                {
                    this._liters = value;
                    this.OnPropertyChanged("Liters");
                }
            }
        }

        private bool _gallons;
        public bool Gallons
        {
            get
            {
                return this._gallons;
            }

            set
            {
                if (value != this._gallons)
                {
                    this._gallons = value;
                    this.OnPropertyChanged("Gallons");
                }
            }
        }
    }
}
