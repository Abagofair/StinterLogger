using StinterLogger.RaceLogging.Iracing;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;
using System.Windows;
using System.Windows.Input;

namespace StinterLogger.UI.FuelPage
{
    public class FuelCalculatorViewModel : ObservableObject, IPageViewModel
    {
        private IDataLogger<FuelDataEventArgs> _fuelCalculator;

        private ICommand _enableFuelCalculator;

        private FuelModel _fuelModel;

        public FuelCalculatorViewModel()
        {
            this.Name = "Fuel Calculator";
            this._fuelCalculator = ((App)Application.Current).FuelManager;
            this._fuelCalculator.OnDataModelChange += this.OnFuelModelChange;
        }

        public string Name { get; set; }

        public FuelModel FuelModel
        {
            get
            {
                return this._fuelModel;
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

        private void OnFuelModelChange(object sender, FuelDataEventArgs fuelDataEventArgs)
        {
            this._fuelModel = new FuelModel(fuelDataEventArgs.FuelData);
            OnPropertyChanged("FuelModel");
        }

        private void EnableControl(string enable)
        {
            if (enable == "Enable")
            {
                this._fuelCalculator.Enable();
                this._fuelModel = new FuelModel((FuelData)this._fuelCalculator.DataModel);
                this._fuelModel.Enabled = true;

            }
            else if (enable == "Disable")
            {
                this._fuelCalculator.Disable();
                this._fuelModel = null;
            }

            OnPropertyChanged("FuelModel");
        }
    }
}
