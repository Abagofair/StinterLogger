using StinterLogger.RaceLogging.Iracing.Fuel;
using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;
using System.Windows;
using System.Windows.Input;

namespace StinterLogger.UI.FuelPage
{
    public class FuelCalculatorViewModel : ObservableObject, IPageViewModel
    {
        private FuelManager _fuelCalculator;
        private ICommand _enableFuelCalculator;

        public FuelCalculatorViewModel()
        {
            this.Name = "Fuel Calculator";
            this._fuelCalculator = ((App)Application.Current).FuelManager;
        }

        public string Name { get; set; }

        public FuelModel FuelModel
        {
            get
            {
                return this._fuelCalculator.FuelModel;
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

        private void OnFuelModelChange()
        {
            OnPropertyChanged("FuelModel");
        }

        private void EnableControl(string enable)
        {
            if (enable == "Enable")
            {
                this._fuelCalculator.Enable();
            }
            else if (enable == "Disable")
            {
                this._fuelCalculator.Disable();
            }

            OnPropertyChanged("FuelModel");
        }
    }
}
