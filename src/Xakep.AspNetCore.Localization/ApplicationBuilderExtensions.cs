// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Linq;

namespace Xakep.AspNetCore.Localization
{
    /// <summary>
    /// Extension methods for adding the <see cref="RequestLocalizationMiddleware"/> to an application.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        
        /// <summary>
        /// Adds the <see cref="RequestLocalizationMiddleware"/> to automatically set culture information for
        /// requests based on information provided by the client.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="options">The <see cref="RequestLocalizationOptions"/> to configure the middleware with.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseLocalRequestLocalization(
            this IApplicationBuilder app,
            LocalRequestLocalizationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.SupportedAlias)
            {
                options.RequestCultureProviders.Insert(0, new LocalRequestCultureProvider() { Options = options });
                if (options.SupportedAliasUICultures == null)
                {
                    options.SupportedAliasUICultures = options.SupportedUICultures.Select(w => new KeyValuePair<string, string>(w.Name, w.Name)).ToList();
                }
                return app.UseMiddleware<LocalRequestLocalizationMiddleware>(Options.Create(options))
                    .UseMiddleware<RequestLocalizationMiddleware>(Options.Create(options))
                    .Map("/Local/Redirect", builder => builder.Run((content) => LocalUrlHelper.RedirectLocal(content)));
            }
            return app.UseMiddleware<RequestLocalizationMiddleware>(Options.Create(options));
        }
    }
   
}