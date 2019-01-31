using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StinterLogger.FuelCalculator
{
    public class FuelCalculatorViewModel : IPageViewModel
    {
        private FuelModel _fuelModel;
        private bool _isEnabled;
        private ICommand _enableFuelCalculator;

        public FuelCalculatorViewModel()
        {
            this.Name = "Fuel Calculator";
            this.FuelModel = new FuelModel
            {
                InTank = 10,
                PerLap = 20,
                PerHour = 30
            };
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
            this.FuelModel.InTank += 10;
            if (enable == "Enable")
            {
                this._isEnabled = true;
            }
            else if (enable == "Disable")
            {
                this._isEnabled = false;
            }
        }
    }
}
