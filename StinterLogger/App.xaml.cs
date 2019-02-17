using iRacingSdkWrapper;
using StinterLogger.FuelCalculator;
using StinterLogger.RaceLogging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StinterLogger
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ModelManager _modelManager;
        private FuelManager _fuelManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.raceLogger = new RaceLogger(4);

            this._modelManager = new ModelManager();

            this._fuelManager = new FuelManager(this.raceLogger, 1);

            this.raceLogger.Start();

            var vm = new ApplicationViewModel();
            var mw = new MainWindow();

            mw.DataContext = vm;
            mw.Show();
        }

        public ModelManager ModelManager
        {
            get
            {
                return this._modelManager;
            }
        }

        public FuelManager FuelManager
        {
            get
            {
                return this._fuelManager;
            }
        }

        public IRaceLogger raceLogger
        {
            get;
            set;
        }
    }
}
