using iRacingSdkWrapper;
using StinterLogger.UI;
using StinterLogger.RaceLogging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StinterLogger.RaceLogging.Iracing.Fuel;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing;

namespace StinterLogger.UI.MainApp
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private FuelManager _fuelManager;
        private IracingLogger _raceLogger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this._raceLogger = new IracingLogger(4);

            this._raceLogger.Connected += this.OnRaceLoggerConnection;

            this._raceLogger.Disconnected += this.OnRaceLoggerDisconnection;

            this._fuelManager = new FuelManager(this._raceLogger, 1);

            this._raceLogger.Start();

            var vm = new ApplicationViewModel();
            var mw = new MainWindow
            {
                DataContext = vm
            };

            mw.Show();

            this.MainWindow.Title = "Disconnected - Driver ID: ?";
        }

        public FuelManager FuelManager
        {
            get
            {
                return this._fuelManager;
            }
        }

        public IRaceLogger RaceLogger
        {
            get;
            set;
        }

        public void OnRaceLoggerConnection(object sender, DriverConnectionEventArgs driverConnectionEventArgs)
        {
            this.MainWindow.Title = "Connected to iRacing - Driver ID: " + this._raceLogger.DriverId;
        }

        public void OnRaceLoggerDisconnection(object sender, EventArgs eventArgs)
        {
            this.MainWindow.Title = "Disconnected - Driver ID: ?";
        }
    }
}
