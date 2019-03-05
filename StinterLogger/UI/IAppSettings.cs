using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.UI
{
    public interface IAppSettings
    {
        void LoadAll();
        string GetValue(string setting);
    }
}
