using iRacingSdkWrapper;
using Ninject;
using StinterLogger.FuelCalculator;
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
        private SdkWrapper _sdkWrapper;
        private ModelManager _modelManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this._modelManager = new ModelManager();

            _sdkWrapper = new SdkWrapper();
            _sdkWrapper.TelemetryUpdateFrequency = 4;

            var vm = new ApplicationViewModel();
            var mw = new MainWindow(_sdkWrapper);

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
    }
}
