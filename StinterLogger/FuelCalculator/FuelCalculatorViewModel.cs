using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StinterLogger.FuelCalculator
{
    public class FuelCalculatorViewModel : ObservableObject, IPageViewModel
    {
        private FuelModel _fuelModel;
        private bool _isEnabled;
        private ICommand _enableFuelCalculator;

        public FuelCalculatorViewModel()
        {
            this.Name = "Fuel Calculator";
            ((App)Application.Current).SdkWrapper.TelemetryUpdated += this.OnTelemetryUpdate;
        }

        private float fuelAtLapStart = 0.0f;
        private void OnTelemetryUpdate(object sender, iRacingSdkWrapper.SdkWrapper.TelemetryUpdatedEventArgs e)
        { 
            var lapsCompleted = e.TelemetryInfo.LapCompleted.Value;
            this.FuelModel.InTank = e.TelemetryInfo.FuelLevel.Value;
            this.FuelModel.SessionLength = ((float)e.TelemetryInfo.SessionTimeRemain.Value) / 60.0f;
            
            if (fuelAtLapStart == 0.0f)
            {
                fuelAtLapStart = this.FuelModel.InTank;
            }

            if (this.FuelModel.LapsCompleted < lapsCompleted)
            {
                var fuelUsedThisLap = fuelAtLapStart - this.FuelModel.InTank;
                this.FuelModel.TotalUsed += fuelUsedThisLap;
                this.FuelModel.PerLap = this.FuelModel.TotalUsed / this.FuelModel.LapsCompleted;

                fuelAtLapStart = this.FuelModel.InTank;
                this.FuelModel.LapsCompleted = e.TelemetryInfo.LapCompleted.Value;
            }
        }

        public string Name { get; set; }

        public FuelModel FuelModel
        {
            get
            {
                return this._fuelModel;
            }

            set
            {
                this._fuelModel = value;
                OnPropertyChanged("FuelModel");
            }
        }

        public ICommand EnableFuelCalculator
        {
            get
            {
                if (this._enableFuelCalculator == null)
                {
                    this._enableFuelCalculator = new RelayCommand(
                        b => this.EnableControl((string)b)
                        );
                }
                return _enableFuelCalculator;
            }
        }

        private void EnableControl(string enable)
        {
            if (enable == "Enable")
            {
                this._isEnabled = true;
                try
                {
                    this.FuelModel = ((App)Application.Current).ModelManager.CreateModel(ModelType.FuelModel) as FuelModel;
                }
                catch (Exception e)
                {
                    //log
                }
            }
            else if (enable == "Disable")
            {
                this._isEnabled = false;
                try
                {
                    this.FuelModel = null;
                    ((App)Application.Current).ModelManager.DestroyModel(ModelType.FuelModel);
                }
                catch (Exception e)
                {
                    //log
                }
            }
        }
    }
}
