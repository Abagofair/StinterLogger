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
using System.Text;
using StinterLogger.RaceLogging.Iracing.Debug;

namespace StinterLogger.UI.MainApp
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private FuelManager _fuelManager;
        private IRaceLogger _raceLogger;
        private IDataLogger<DebugEventArgs> _debugLogger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this._debugLogger = new DebugLogger();
            this._debugLogger.Enable();

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

            mw.Closing += this.WindowClosing;
            mw.Show();

            this.MainWindow.Title = "Disconnected - Driver ID: ?";
        }

        private void WindowClosing(object sender, EventArgs eventArgs)
        {
            this._raceLogger.Stop();
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
            get
            {
                return this._raceLogger;
            }
        }

        public DebugLogger DebugLogger
        {
            get
            {
                return ((DebugLogger)this._debugLogger);
            }
        }

        public void OnRaceLoggerConnection(object sender, DriverConnectionEventArgs driverConnectionEventArgs)
        {
            var spacing = " - ";
            var displayString = new StringBuilder();
            displayString.Append("Connected");
            displayString.Append(spacing);
            displayString.Append("GlobalUserId: " + driverConnectionEventArgs.ActiveDriverInfo.GlobalUserId);
            displayString.Append(spacing);
            displayString.Append("UserName: " + driverConnectionEventArgs.ActiveDriverInfo.DriverName);
            this.MainWindow.Title = displayString.ToString();

            this.DebugLogger.CreateEventLog(displayString.ToString());
        }

        public void OnRaceLoggerDisconnection(object sender, EventArgs eventArgs)
        {
            var displayString = "Disconnected - Driver ID: ?";
            this.MainWindow.Title = displayString;

            this.DebugLogger.CreateEventLog(displayString);
        }
    }
}
