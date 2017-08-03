using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Xakep.AspNetCore.Localization
{
    /// <summary>
    /// 重写UrlHelper的GenerateUrl 方法
    /// </summary>
    public class LocalUrlHelper : UrlHelper
    {
        internal void AppendPathAndFragment(StringBuilder builder, VirtualPathData pathData, string fragment)
        {
            var pathBase = HttpContext.Request.PathBase;

            if (!pathBase.HasValue)
            {
                if (pathData.VirtualPath.Length == 0)
                {
                    builder.Append("/");
                }
                else
                {
                    if (!pathData.VirtualPath.StartsWith("/", StringComparison.Ordinal))
                    {
                        builder.Append("/");
                    }

                    builder.Append(pathData.VirtualPath);
                }
            }
            else
            {
                if (pathData.VirtualPath.Length == 0)
                {
                    builder.Append(pathBase.Value);
                }
                else
                {
                    builder.Append(pathBase.Value);

                    if (pathBase.Value.EndsWith("/", StringComparison.Ordinal))
                    {
                        builder.Length--;
                    }

                    if (!pathData.VirtualPath.StartsWith("/", StringComparison.Ordinal))
                    {
                        builder.Append("/");
                    }

                    builder.Append(pathData.VirtualPath);
                }
            }

            if (!string.IsNullOrEmpty(fragment))
            {
                builder.Append("#").Append(fragment);
            }
        }

        public LocalUrlHelper(ActionContext actionContext) : base(actionContext) { }

        /// <summary>
        /// Generates the URL using the specified components.
        /// </summary>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <param name="host">The host name for the URL.</param>
        /// <param name="pathData">The <see cref="VirtualPathData"/>.</param>
        /// <param name="fragment">The fragment for the URL.</param>
        /// <returns>The generated URL.</returns>
        protected override string GenerateUrl(string protocol, string host, VirtualPathData pathData, string fragment)
        {
            if (pathData == null)
            {
                return null;
            }

            // VirtualPathData.VirtualPath returns string.Empty instead of null.
            Debug.Assert(pathData.VirtualPath != null);

            // Perf: In most of the common cases, GenerateUrl is called with a null protocol, host and fragment.
            // In such cases, we might not need to build any URL as the url generated is mostly same as the virtual path available in pathData.
            // For such common cases, this FastGenerateUrl method saves a string allocation per GenerateUrl call.
            string url;
            if (TryFastGenerateUrl(protocol, host, pathData, fragment, out url))
            {
                return url;
            }

            var builder = new StringBuilder();
            try
            {
                if (string.IsNullOrEmpty(protocol) && string.IsNullOrEmpty(host))
                {
                    AppendPathAndFragment(builder, pathData, fragment);
                    // We're returning a partial URL (just path + query + fragment), but we still want it to be rooted.
                    if (builder.Length == 0 || builder[0] != '/')
                    {
                        builder.Insert(0, '/');
                    }
                }
                else
                {
                    protocol = string.IsNullOrEmpty(protocol) ? "http" : protocol;
                    builder.Append(protocol);

                    builder.Append("://");

                    host = string.IsNullOrEmpty(host) ? HttpContext.Request.Host.Value : host;
                    builder.Append(host);
                    AppendPathAndFragment(builder, pathData, fragment);
                }

                var path = builder.ToString();
                return path;
            }
            finally
            {
                // Clear the StringBuilder so that it can reused for the next call.
                builder.Clear();
            }
        }

        private bool TryFastGenerateUrl(
            string protocol,
            string host,
            VirtualPathData pathData,
            string fragment,
            out string url)
        {
            var pathBase = HttpContext.Request.PathBase;
            url = null;

            if (string.IsNullOrEmpty(protocol)
                && string.IsNullOrEmpty(host)
                && string.IsNullOrEmpty(fragment)
                && !pathBase.HasValue)
            {
                var Options = HttpContext.RequestServices.GetService<IOptions<LocalRequestLocalizationOptions>>();

                if (Options == null)
                {
                    return TryFastGenerateUrl(pathData, out url);
                }

                var XOptions = Options.Value.GetReload();
                if (XOptions.SupportedAlias != null && (XOptions.SupportedDefaultAlias || (!XOptions.SupportedDefaultAlias && CultureInfo.CurrentUICulture.Name != XOptions.DefaultRequestCulture.Culture.Name)))
                {
                    var vAlias = XOptions.SupportedAlias.Where(w => w.Enabled && w.Name.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase));
                    if (vAlias.Count() > 0)
                    {
                        url = "/" + vAlias.First().Alia + pathData.VirtualPath;
                        return true;
                    }
                    else
                        return TryFastGenerateUrl(pathData, out url);
                }
                else
                    return TryFastGenerateUrl(pathData, out url);
            }
            return false;
        }

        private bool TryFastGenerateUrl(VirtualPathData pathData, out string url)
        {
            url = null;
            if (pathData.VirtualPath.Length == 0)
            {
                url = "/";
                return true;
            }
            else if (pathData.VirtualPath.StartsWith("/", StringComparison.Ordinal))
            {
                url = pathData.VirtualPath;
                return true;
            }
            return false;
        }

    }

    public class UrlLocalHelperFactory : IUrlHelperFactory
    {
        /// <inheritdoc />
        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            var httpContext = context.HttpContext;

            //if (httpContext == null)
            //{
            //    throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
            //        nameof(ActionContext.HttpContext),
            //        nameof(ActionContext)));
            //}

            //if (httpContext.Items == null)
            //{
            //    throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
            //        nameof(HttpContext.Items),
            //        nameof(HttpContext)));
            //}

            // Perf: Create only one UrlHelper per context
            var urlHelper = httpContext.Items[typeof(IUrlHelper)] as IUrlHelper;
            if (urlHelper == null)
            {
                urlHelper = new LocalUrlHelper(context);
                httpContext.Items[typeof(IUrlHelper)] = urlHelper;
            }

            return urlHelper;
        }
    }

}
