﻿using StinterLogger.RaceLogging;
using System;
using System.Collections.Generic;
using System.Windows;
using StinterLogger.UI.DetailedDebug;
using StinterLogger.UI.Configuration;
using StinterLogger.RaceLogging.General.Fuel;
using StinterLogger.RaceLogging.General;
using StinterLogger.RaceLogging.General.Program;
using StinterLogger.RaceLogging.General.Debug;
using StinterLogger.RaceLogging.Simulations.Iracing;
using StinterLogger.RaceLogging.General.SimEventArgs;
using System.Text;

namespace StinterLogger.UI.MainApp
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private FuelManager _fuelManager;

        //private IRaceLogger _raceLogger;

        private DebugManager _debugLogger;

        private ProgramLoader _programConfigurator;

        private IAppSettings _appSettings;

        private ISimLogger _simLogger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this._appSettings = new AppSettings();

            this._debugLogger = new DebugManager();
            this._debugLogger.Enable();

            this._simLogger = new iRacingLogger(4);
            this._simLogger.DriverConnected += this.OnRaceLoggerConnection;
            this._simLogger.DriverDisconnected += this.OnRaceLoggerDisconnection;

            this._fuelManager = new FuelManager(this._simLogger, this._debugLogger, 1);

            this._simLogger.StartListening();

            var vm = new ApplicationViewModel();
            var mw = new MainWindow
            {
                DataContext = vm
            };

            mw.Closing += this.WindowClosing;
            mw.Show();

            this._programConfigurator = new ProgramLoader(this._appSettings.GetValue("ProgramConfigs"), null);

            var pm = new ProgramManager(this._simLogger, this._programConfigurator);
            //pm.Load("default");
            //pm.StartProgram();

            this.MainWindow.Title = "Disconnected - Driver ID: ?";
        }

        private void WindowClosing(object sender, EventArgs eventArgs)
        {
            this._simLogger.StopListening();
        }

        public FuelManager FuelManager
        {
            get
            {
                return this._fuelManager;
            }
        }

        public ISimLogger RaceLogger
        {
            get
            {
                return this._simLogger;
            }
        }

        public DebugManager DebugLogger
        {
            get
            {
                return ((DebugManager)this._debugLogger);
            }
        }

        public ProgramLoader AppSettings
        {
            get
            {
                return this._programConfigurator;
            }
        }

        public void OpenDetailedDebugWindow(List<string> detailedContent)
        {
            var detailWindow = new DetailedDebugWindow(detailedContent)
            {
                ResizeMode = ResizeMode.NoResize
            };
            detailWindow.ShowDialog();
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
