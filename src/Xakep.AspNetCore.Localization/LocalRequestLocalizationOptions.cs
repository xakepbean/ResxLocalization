using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Xakep.AspNetCore.Localization
{
    public class LocalRequestLocalizationOptions : RequestLocalizationOptions
    {
        private IConfiguration _config = null;

        private string _DefaultCulture = string.Empty;
        public string DefaultCulture
        {
            get { return _DefaultCulture; }
            set
            {
                _DefaultCulture = value;
                DefaultRequestCulture = new RequestCulture(value);
            }
        }

        public LocalRequestLocalizationOptions() : base()
        {
        }

        public List<AliasCulture> SupportedAlias { get; set; }

        public bool AcceptLanguage { get; set; } = true;

        public bool AcceptCookie { get; set; } = true;

        public string CookieName { get; set; } = ".AspNetCore.Culture";


        public bool SupportedDefaultAlias { get; set; } = false;
        public bool AcceptQueryString { get; set; } = true;
        public string QueryStringKey { get; set; } = "culture";

        public void FormatJson(IConfiguration Config)
        {
            _config = Config;
            FormatJson();
        }

        private void FormatJson()
        {
            if (_config != null)
            {
                lock (_config)
                {
                    if (SupportedAlias != null)
                        SupportedAlias.Clear();
                    ConfigurationBinder.Bind(_config, this);
                    SupportedCultures = SupportedAlias.Where(w => w.Enabled).Select(w => new CultureInfo(w.Name)).ToList();
                    SupportedUICultures = SupportedCultures;

                    var vProviders = RequestCultureProviders.Where(w => w is LocalRequestCultureProvider);
                    if (vProviders.Count() == 0)
                    {
                        RequestCultureProviders.Insert(0, new LocalRequestCultureProvider() { Options = this });
                    }
                }
            }
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                DefaultCulture = DefaultCulture,
                AcceptLanguage = AcceptLanguage,
                AcceptCookie = AcceptCookie,
                SupportedDefaultAlias = SupportedDefaultAlias,
                AcceptQueryString = AcceptQueryString,
                CookieName = CookieName,
                QueryStringKey = QueryStringKey,
                SupportedAlias = SupportedAlias
            });
        }

        public LocalRequestLocalizationOptions GetReload()
        {
            if (_config == null)
                return this;
            //if (_config.GetReloadToken().HasChanged)
            //{
            FormatJson();
            //}
            return this;
        }
    }

    public class AliasCulture
    {
        public string Name { get; set; }
        public string Alia { get; set; }
        public bool Enabled { get; set; }

    }


}
