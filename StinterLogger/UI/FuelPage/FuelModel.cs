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

        public FuelModel(FuelManagerData fuelData)
        {
            this.SetValues(fuelData);
        }

        public void SetValues(FuelManagerData fuelData)
        {
            this.InTank = fuelData.FuelInTank;

            this.LapsCompleted = fuelData.LapsCompleted;

            this.LapsRemaining = fuelData.LapsRemaining;

            this.PerLap = fuelData.FuelUsagePerLap;

            this.TotalLapTime = fuelData.TotalRaceTime;

            this.TotalUsed = fuelData.TotalFuelUsed;

            this.AmountToAdd = fuelData.FuelToFinish;

            this.RemainingSessionTime = fuelData.RemainingRacetime;

            this.Liters = false;
            this.Gallons = false;
            if (fuelData.Unit == FuelUnit.Liters)
            {
                this.Liters = true;
                this.Gallons = false;
            }
            else if (fuelData.Unit == FuelUnit.Gallons)
            {
                this.Liters = false;
                this.Gallons = true;
            }

            this.GraceLaps = false;
            this.GracePercent = false;
            this.GraceValue = fuelData.GraceOption.Value;
            if (fuelData.GraceOption.Mode == GraceMode.Lap)
            {
                this.GraceLaps = true;
                this.GracePercent = false;
            }
            else if (fuelData.GraceOption.Mode == GraceMode.Percent)
            {
                this.GraceLaps = false;
                this.GracePercent = true;
            }
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

        private bool _gracePercent;
        public bool GracePercent
        {
            get
            {
                return this._gracePercent;
            }

            set
            {
                if (value != this._gracePercent)
                {
                    this._gracePercent = value;
                    this.OnPropertyChanged("GracePercent");
                }
            }
        }

        private bool _graceLaps;
        public bool GraceLaps
        {
            get
            {
                return this._graceLaps;
            }

            set
            {
                if (value != this._graceLaps)
                {
                    this._graceLaps = value;
                    this.OnPropertyChanged("GraceLaps");
                }
            }
        }

        private float _graceValue;
        public float GraceValue
        {
            get
            {
                return this._graceValue;
            }

            set
            {
                if (value != this._graceValue)
                {
                    this._graceValue = value;
                    this.OnPropertyChanged("GraceValue");
                }
            }
        }
    }
}
