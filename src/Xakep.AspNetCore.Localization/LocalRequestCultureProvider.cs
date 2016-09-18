using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Xakep.AspNetCore.Localization
{
    public class LocalRequestCultureProvider : RequestCultureProvider
    {
        private static readonly char[] _cookieSeparator = new[] { '|' };
        private static readonly string _culturePrefix = "c=";
        private static readonly string _uiCulturePrefix = "uic=";
        private int MaximumAcceptLanguageHeaderValuesToTry { get; set; } = 3;
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var request = httpContext.Request;
            if (!request.Path.HasValue || request.Path.Value == "/")
            {
                return Task.FromResult((ProviderCultureResult)null);
            }
            var XOptions = httpContext.Features.Get<ILocalRequestCultureFeature>();
          
            if (XOptions == null)
            {
                return Task.FromResult((ProviderCultureResult)null);
            }
            var segments = request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (XOptions.SupportedAliasUICultures!=null && segments.Length > 0)
            {
                var vAlias = XOptions.SupportedAliasUICultures.Where(w => w.Key.Equals(segments[0], StringComparison.OrdinalIgnoreCase));
                if (vAlias.Count() > 0)
                {
                    var UrlCulture = vAlias.First();
                    httpContext.Request.Path = segments.Length == 1 ? "/" : "/" + string.Join("/", segments, 1, segments.Length - 1);
                    return Task.FromResult(new ProviderCultureResult(UrlCulture.Value, UrlCulture.Value));
                }
            }

            if (XOptions.AcceptQueryString && !httpContext.Request.QueryString.HasValue)
            {
                var QSResultCulture = ParseQueryString(httpContext.Request, XOptions.QueryStringKey);
                if (QSResultCulture != null)
                    return Task.FromResult(QSResultCulture);
            }
            

            if (XOptions.AcceptCookie && !string.IsNullOrEmpty(httpContext.Request.Cookies[XOptions.CookieName]))
            {
                var CookieResultCulture = ParseCookieValue(httpContext.Request.Cookies[XOptions.CookieName]);
                if (CookieResultCulture != null)
                    return Task.FromResult(CookieResultCulture);
            }

            if (XOptions.AcceptLanguage)
            {
                var LanguageHeadResultCulture = ParseLanguageHeader(httpContext.Request);
                if (LanguageHeadResultCulture != null)
                    return Task.FromResult(LanguageHeadResultCulture);
            }


            return Task.FromResult((ProviderCultureResult)null);
        }
        private ProviderCultureResult ParseCookieValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var parts = value.Split(_cookieSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;

            var potentialCultureName = parts[0];
            var potentialUICultureName = parts[1];
            if (!potentialCultureName.StartsWith(_culturePrefix) || !potentialUICultureName.StartsWith(_uiCulturePrefix))
                return null;

            var cultureName = potentialCultureName.Substring(_culturePrefix.Length);
            var uiCultureName = potentialUICultureName.Substring(_uiCulturePrefix.Length);

            if (cultureName == null && uiCultureName == null)
                return null;

            if (cultureName != null && uiCultureName == null)
                uiCultureName = cultureName;

            if (cultureName == null && uiCultureName != null)
                cultureName = uiCultureName;

            return new ProviderCultureResult(cultureName, uiCultureName);
        }

        private ProviderCultureResult ParseQueryString(HttpRequest request, string QueryStringKey)
        {
            string queryUICulture = null;

            if (!string.IsNullOrWhiteSpace(QueryStringKey))
                queryUICulture = request.Query[QueryStringKey];

            if (queryUICulture == null)
                return null;
            return new ProviderCultureResult(queryUICulture, queryUICulture);
        }

        private ProviderCultureResult ParseLanguageHeader(HttpRequest request)
        {
            var acceptLanguageHeader = request.GetTypedHeaders().AcceptLanguage;
            if (acceptLanguageHeader == null || acceptLanguageHeader.Count == 0)
                return null;

            var languages = acceptLanguageHeader.AsEnumerable();
            if (MaximumAcceptLanguageHeaderValuesToTry > 0)
                languages = languages.Take(MaximumAcceptLanguageHeaderValuesToTry);

            var orderedLanguages = languages.OrderByDescending(h => h, StringWithQualityHeaderValueComparer.QualityComparer)
                .Select(x => x.Value).ToList();

            if (orderedLanguages.Any())
                return new ProviderCultureResult(orderedLanguages);

            return null;
        }

    }
}
