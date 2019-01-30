using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.Start
{
    public class StartViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get; set; }

        public StartViewModel()
        {
            this.Name = "Start";
        }
    }
}
