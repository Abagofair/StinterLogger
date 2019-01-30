using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.FuelCalculator
{
    public class FuelCalculatorViewModel : IPageViewModel
    {
        private FuelModel _fuelModel;

        public string Name { get; set; }

        public FuelCalculatorViewModel()
        {
            this.Name = "Fuel Calculator";
        }
    }
}
