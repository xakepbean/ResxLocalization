using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xakep.AspNetCore.Localization
{
    public interface ILocalRequestCultureFeature
    {
        /// <summary>
        /// The <see cref="Localization.RequestCulture"/> of the request.
        /// </summary>
        RequestCulture DefaultRequestCulture { get; set; }

        IList<KeyValuePair<string, string>> SupportedAliasUICultures { get; set; }

        bool AcceptLanguage { get; set; }

        bool AcceptCookie { get; set; }

        bool AcceptQueryString { get; set; }
        string QueryStringKey { get; set; }

        string CookieName { get; set; }

        bool SupportedDefaultAlias { get; set; } 
    }
    public class LocalRequestCultureFeature : ILocalRequestCultureFeature
    {
        /// <summary>
        /// Creates a new <see cref="RequestCultureFeature"/> with the specified <see cref="Localization.RequestCulture"/>.
        /// </summary>
        /// <param name="requestCulture">The <see cref="Localization.RequestCulture"/>.</param>
        /// <param name="provider">The <see cref="IRequestCultureProvider"/>.</param>
        public LocalRequestCultureFeature(LocalRequestLocalizationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            DefaultRequestCulture = options.DefaultRequestCulture;
            SupportedAliasUICultures = options.SupportedAliasUICultures;
            AcceptLanguage = options.AcceptLanguage;
            AcceptQueryString = options.AcceptQueryString;
            QueryStringKey = options.QueryStringKey;
            AcceptCookie = options.AcceptCookie;
            CookieName = options.CookieName;
            SupportedDefaultAlias = options.SupportedDefaultAlias;
        }

        /// <inheritdoc />
        public RequestCulture DefaultRequestCulture { get; set; }

        public IList<KeyValuePair<string, string>> SupportedAliasUICultures { get; set; }

        public bool AcceptLanguage { get; set; }

        public bool AcceptCookie { get; set; }

        public string CookieName { get; set; }
        public bool SupportedDefaultAlias { get; set; }
        public bool AcceptQueryString { get; set; }
        public string QueryStringKey { get; set; }


    }
}
