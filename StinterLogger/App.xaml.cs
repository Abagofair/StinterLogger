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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this._modelManager = new ModelManager();

            this.raceLogger = new RaceLogger(4);
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

        public IRaceLogger raceLogger
        {
            get;
            set;
        }
    }
}
