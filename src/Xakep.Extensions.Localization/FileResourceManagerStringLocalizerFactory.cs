// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;

namespace Xakep.Extensions.Localization
{
    /// <summary>
    /// An <see cref="IStringLocalizerFactory"/> that creates instances of <see cref="FileResourceManagerStringLocalizer"/>.
    /// </summary>
    public class FileResourceManagerStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IResourceNamesCache _resourceNamesCache = new ResourceNamesCache();
        private readonly ConcurrentDictionary<string, FileResourceManagerStringLocalizer> _localizerCache =
            new ConcurrentDictionary<string, FileResourceManagerStringLocalizer>();
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _resourcesRelativePath;
        private readonly string _ResourcePath;
        private readonly IFileProvider _fileProvider;
        private readonly bool _EnabledFiles = false;
        /// <summary>
        /// Creates a new <see cref="FileResourceManagerStringLocalizer"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="localizationOptions">The <see cref="IOptions{LocalizationOptions}"/>.</param>
        public FileResourceManagerStringLocalizerFactory(
            IHostingEnvironment hostingEnvironment,
            IOptions<FileLocalizationOptions> localizationOptions)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }
            
            _hostingEnvironment = hostingEnvironment;
            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
            _EnabledFiles = localizationOptions.Value.EnabledFiles;
            _fileProvider =  _hostingEnvironment.ContentRootFileProvider;
            if (!string.IsNullOrEmpty(_resourcesRelativePath))
            {
                _ResourcePath += _resourcesRelativePath.TrimEnd(Path.AltDirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar) + "/";
                _resourcesRelativePath = _resourcesRelativePath.Replace(Path.AltDirectorySeparatorChar, '.')
                    .Replace(Path.DirectorySeparatorChar, '.') + ".";
            }
        }

        /// <summary>
        /// Gets the resource prefix used to look up the resource.
        /// </summary>
        /// <param name="typeInfo">The type of the resource to be looked up.</param>
        /// <returns>The prefix for resource lookup.</returns>
        protected virtual string GetResourcePrefix(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            return GetResourcePrefix(typeInfo, _hostingEnvironment.ApplicationName, _resourcesRelativePath);
        }

        /// <summary>
        /// Gets the resource prefix used to look up the resource.
        /// </summary>
        /// <param name="typeInfo">The type of the resource to be looked up.</param>
        /// <param name="baseNamespace">The base namespace of the application.</param>
        /// <param name="resourcesRelativePath">The folder containing all resources.</param>
        /// <returns>The prefix for resource lookup.</returns>
        /// <remarks>
        /// For the type "Sample.Controllers.Home" if there's a resourceRelativePath return
        /// "Sample.Resourcepath.Controllers.Home" if there isn't one then it would return "Sample.Controllers.Home".
        /// </remarks>
        protected virtual string GetResourcePrefix(TypeInfo typeInfo, string baseNamespace, string resourcesRelativePath)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (string.IsNullOrEmpty(baseNamespace))
            {
                throw new ArgumentNullException(nameof(baseNamespace));
            }

            return string.IsNullOrEmpty(resourcesRelativePath)
                ? typeInfo.FullName
                : baseNamespace + "." + resourcesRelativePath + TrimPrefix(typeInfo.FullName, baseNamespace + ".");
        }

        /// <summary>
        /// Gets the resource prefix used to look up the resource.
        /// </summary>
        /// <param name="baseResourceName">The name of the resource to be looked up</param>
        /// <param name="baseNamespace">The base namespace of the application.</param>
        /// <returns>The prefix for resource lookup.</returns>
        protected virtual string GetResourcePrefix(string baseResourceName, string baseNamespace)
        {
            if (string.IsNullOrEmpty(baseResourceName))
            {
                throw new ArgumentNullException(nameof(baseResourceName));
            }

            var locationPath = baseNamespace == _hostingEnvironment.ApplicationName ?
                baseNamespace + "." + _resourcesRelativePath :
                baseNamespace + ".";
            baseResourceName = locationPath + TrimPrefix(baseResourceName, baseNamespace + ".");

            return baseResourceName;
        }

        /// <summary>
        /// Creates a <see cref="FileResourceManagerStringLocalizer"/> using the <see cref="Assembly"/> and
        /// <see cref="Type.FullName"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="resourceSource">The <see cref="Type"/>.</param>
        /// <returns>The <see cref="FileResourceManagerStringLocalizer"/>.</returns>
        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }

            var typeInfo = resourceSource.GetTypeInfo();
            var assembly = typeInfo.Assembly;

            // Re-root the base name if a resources path is set
            var baseName = GetResourcePrefix(typeInfo);

            string pathName = TrimPrefix(typeInfo.FullName, _hostingEnvironment.ApplicationName + ".");
            return _localizerCache.GetOrAdd(baseName, _ =>
                new FileResourceManagerStringLocalizer(
                    new ResourceManager(baseName, assembly),
                    assembly,
                    baseName,
                    _resourceNamesCache,
                    _fileProvider,
                    _ResourcePath,
                    pathName,
                    _EnabledFiles)
            );
        }

        /// <summary>
        /// Creates a <see cref="FileResourceManagerStringLocalizer"/>.
        /// </summary>
        /// <param name="baseName">The base name of the resource to load strings from.</param>
        /// <param name="location">The location to load resources from.</param>
        /// <returns>The <see cref="FileResourceManagerStringLocalizer"/>.</returns>
        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            location = location ?? _hostingEnvironment.ApplicationName;

            string pathName = TrimPrefix(baseName, location + "."); //获取文件名，不包含后缀名
            baseName = GetResourcePrefix(baseName, location);

            return _localizerCache.GetOrAdd($"B={baseName},L={location}", _ =>
            {
                var assembly = Assembly.Load(new AssemblyName(location));
                return new FileResourceManagerStringLocalizer(
                    new ResourceManager(baseName, assembly),
                    assembly,
                    baseName,
                    _resourceNamesCache,
                    _fileProvider,
                    _ResourcePath,
                    pathName,
                    _EnabledFiles);
            });
        }

        private static string TrimPrefix(string name, string prefix)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }
            return name;
        }

    }
}