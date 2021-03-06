﻿using RaceLogging.General;
using RaceLogging.General.Fuel;
using RaceLogging.General.SimEventArgs;
using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;
using System.Windows;
using System.Windows.Input;

namespace StinterLogger.UI.FuelPage
{
    public class FuelCalculatorViewModel : ObservableObject, IPageViewModel
    {
        private IManager<FuelDataEventArgs> _fuelCalculator;

        private ICommand _enableFuelCalculator;

        private FuelModel _fuelModel;

        public FuelCalculatorViewModel()
        {
            this.Name = "Fuel Calculator";
            this._fuelCalculator = ((App)Application.Current).FuelManager;
            this._fuelCalculator.OnEntityChange += this.OnFuelModelChange;
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
            if (fuelDataEventArgs.FuelData.GraceOption.Value != this._fuelModel.GraceValue)
            {
                ((FuelManager)_fuelCalculator).SetGraceValue(this._fuelModel.GraceValue);
            }

            this._fuelModel.SetValues(fuelDataEventArgs.FuelData);
            OnPropertyChanged("FuelModel");
        }

        private void EnableControl(string enable)
        {
            if (enable == "Enable")
            {
                this._fuelCalculator.Enable();
                this._fuelModel = new FuelModel((FuelManagerData)this._fuelCalculator.Entity);
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
