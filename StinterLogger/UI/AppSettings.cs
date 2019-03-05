using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.UI.Configuration
{
    public class AppSettings : IAppSettings
    {
        private NameValueCollection _options;

        public AppSettings()
        {
            LoadAll();
        }

        public void LoadAll()
        {
            if (this._options == null)
            {
                this._options = ConfigurationManager.AppSettings;
            }
        }

        public string GetValue(string setting)
        {
            return this._options.Get(setting);
        }
    }
}
