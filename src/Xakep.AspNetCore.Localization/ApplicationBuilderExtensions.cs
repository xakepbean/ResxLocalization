// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;

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
            this IApplicationBuilder app)
        {
            
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            var options= app.ApplicationServices.GetService<IOptions<LocalRequestLocalizationOptions>>();
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            return app.UseMiddleware<RequestLocalizationMiddleware>(Options.Create(options.Value.GetReload()));
        }
    }
   
}