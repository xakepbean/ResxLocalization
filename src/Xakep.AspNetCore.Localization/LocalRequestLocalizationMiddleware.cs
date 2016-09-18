// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Globalization;
using Microsoft.Extensions.Options;

namespace Xakep.AspNetCore.Localization
{
    /// <summary>
    /// Enables automatic setting of the culture for <see cref="Http.HttpRequest"/>s based on information
    /// sent by the client in headers and logic provided by the application.
    /// </summary>
    public class LocalRequestLocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LocalRequestLocalizationOptions _options;

        /// <summary>
        /// Creates a new <see cref="LocalRequestLocalizationMiddleware"/>.
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
        /// <param name="options">The <see cref="RequestLocalizationOptions"/> representing the options for the
        /// <see cref="LocalRequestLocalizationMiddleware"/>.</param>
        public LocalRequestLocalizationMiddleware(RequestDelegate next, IOptions<LocalRequestLocalizationOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options.Value;
        }

        /// <summary>
        /// Invokes the logic of the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="Task"/> that completes when the middleware has completed processing.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Features.Set<ILocalRequestCultureFeature>(new LocalRequestCultureFeature(_options));
            await _next(context);
        }
    }
}