using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xakep.AspNetCore.Localization
{
    public class LocalRequestLocalizationOptions : RequestLocalizationOptions
    {
        public LocalRequestLocalizationOptions() : base()
        {
        }

        public bool SupportedAlias { get; set; } = true;

        public IList<KeyValuePair<string, string>> SupportedAliasUICultures { get; set; }

        public bool AcceptLanguage { get; set; } = true;

        public bool AcceptCookie { get; set; } = true;

        public static readonly string DefaultCookieName = ".AspNetCore.Culture";

        public string CookieName { get; set; } = DefaultCookieName;


        public bool SupportedDefaultAlias { get; set; } = false;
        public bool AcceptQueryString { get; set; } = true;
        public string QueryStringKey { get; set; } = "culture";
    }
}
