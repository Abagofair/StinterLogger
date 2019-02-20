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

            this.raceLogger = new IracingLogger(4);

            this.raceLogger.Connected += this.OnRaceLoggerConnection;

            this.raceLogger.Disconnected += this.OnRaceLoggerDisconnection;

            this._modelManager = new ModelManager();

            this._fuelManager = new FuelManager(this.raceLogger, 1);

            this.raceLogger.Start();

            var vm = new ApplicationViewModel();
            var mw = new MainWindow();

            mw.DataContext = vm;
            mw.Show();

            this.MainWindow.Title = "Disconnected - Driver ID: ?";
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

        public void OnRaceLoggerConnection(object sender, DriverConnectionEventArgs driverConnectionEventArgs)
        {
            this.MainWindow.Title = "Connected to iRacing - Driver ID: " + this.raceLogger.DriverId;
        }

        public void OnRaceLoggerDisconnection(object sender, EventArgs eventArgs)
        {
            this.MainWindow.Title = "Disconnected - Driver ID: ?";
        }
    }
}
