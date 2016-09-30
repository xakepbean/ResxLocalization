// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Xakep.AspNetCore.Localization
{
    public static class LocalizationServiceCollectionExtensions
    {

        /// <summary>
        /// Adds services required for application localization.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setupAction">
        /// An <see cref="Action{LocalizationOptions}"/> to configure the <see cref="LocalizationOptions"/>.
        /// </param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddLocalRequestLocalization(
            this IServiceCollection services,
            Action<LocalRequestLocalizationOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton<IUrlHelperFactory, UrlLocalHelperFactory>();
            return services;
        }

        public static IServiceCollection AddLocalRequestLocalization(
            this IServiceCollection services,
            IConfiguration Config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }
            services.AddOptions();
            
            services.Configure<LocalRequestLocalizationOptions>(options =>  {
                options.FormatJson(Config);
            });
            services.AddSingleton<IUrlHelperFactory, UrlLocalHelperFactory>();
            return services;
        }


    }
}