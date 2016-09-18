using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Localization.Internal;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xml.Linq;

namespace Xakep.Extensions.Localization
{
    public class FileResourceManagerStringLocalizer : ResourceManagerStringLocalizer
    {
        private readonly Assembly _resourceAssembly;
        private readonly string _resourceBaseName;
        private readonly IResourceNamesCache _resourceNamesCache;
        private readonly ResourceManager _resourceManager;
        private readonly ConcurrentDictionary<string, object> _missingManifestCache = new ConcurrentDictionary<string, object>();
        private readonly string _resourcePath;
        private readonly IFileProvider _fileProvider;

        private readonly bool _EnabledFiles = false;
        protected internal string PathName { get; set; }

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _fileResourceCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        /// <summary>
        /// Creates a new <see cref="ResourceManagerStringLocalizer"/>.
        /// </summary>
        /// <param name="resourceManager">The <see cref="ResourceManager"/> to read strings from.</param>
        /// <param name="resourceAssembly">The <see cref="Assembly"/> that contains the strings as embedded resources.</param>
        /// <param name="baseName">The base name of the embedded resource that contains the strings.</param>
        /// <param name="resourceNamesCache">Cache of the list of strings for a given resource assembly name.</param>
        /// <param name="fileProvider"></param>
        /// <param name="resourcePath"></param>
        /// <param name="pathName">存储路径</param>
        /// <param name="EnabledFiles"></param>
        public FileResourceManagerStringLocalizer(
            ResourceManager resourceManager,
            Assembly resourceAssembly,
            string baseName,
            IResourceNamesCache resourceNamesCache, IFileProvider fileProvider, string resourcePath, string pathName, bool EnabledFiles)
            : base(
                  resourceManager,
                  resourceAssembly,
                  baseName,
                  resourceNamesCache)
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException(nameof(resourceAssembly));
            }
            _resourceAssembly = resourceAssembly;
            _fileProvider = fileProvider;
            _resourcePath = resourcePath;
            PathName = pathName;
            _EnabledFiles = EnabledFiles;
            _resourceManager = resourceManager;
            _resourceBaseName = baseName;
            _resourceNamesCache = resourceNamesCache;
        }

        public override LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetStringSafely(name, null);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        /// <inheritdoc />
        public override LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetStringSafely(name, null);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }



        protected new string GetStringSafely(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            var cultureName = (culture ?? CultureInfo.CurrentUICulture).Name;

            var fKey = $"culture={PathName}.{cultureName}.resx".ToLower();
            if (_EnabledFiles && !_missingManifestCache.ContainsKey(fKey))
            {
                if (!_fileResourceCache.ContainsKey(fKey))
                {
                    IChangeToken CToken;
                    ConcurrentDictionary<string, string> _cultureResourceCache = GetFileChangeTokenResource(cultureName, out CToken);
                    if (CToken != null && CToken.ActiveChangeCallbacks)
                        CToken.RegisterChangeCallback(itemKey =>
                        {
                            if (_fileResourceCache.ContainsKey(fKey))
                            {
                                ConcurrentDictionary<string, string> outCache;
                                _fileResourceCache.TryRemove(fKey, out outCache);
                            }
                            var delCKey = $"&culture={cultureName}";
                            var vDelKey = _missingManifestCache.Where(w => w.Key == fKey || w.Key.EndsWith(delCKey)).Select(w => w.Key);
                            foreach (var dKey in vDelKey)
                            {
                                object outobject;
                                _missingManifestCache.TryRemove(dKey, out outobject);
                            }
                        }, null);
                    if (_cultureResourceCache != null)
                    {
                        _fileResourceCache.TryAdd(fKey, _cultureResourceCache);
                        if (_cultureResourceCache.ContainsKey(name))
                            return _cultureResourceCache[name];
                    }
                    else
                        _missingManifestCache.TryAdd(fKey, null);
                }
                if (_fileResourceCache.ContainsKey(fKey) && _fileResourceCache[fKey].ContainsKey(name))
                    return _fileResourceCache[fKey][name];
            }
            return base.GetStringSafely(name, culture);

        }


        public new IStringLocalizer WithCulture(CultureInfo culture)
        {
            return culture == null
                ? new FileResourceManagerStringLocalizer(
                    _resourceManager,
                    _resourceAssembly,
                    _resourceBaseName,
                    _resourceNamesCache,
                    _fileProvider,
                    _resourcePath,
                    PathName,
                    _EnabledFiles)
                : new FileResourceManagerWithCultureStringLocalizer(
                    _resourceManager,
                    _resourceAssembly,
                    _resourceBaseName,
                    _resourceNamesCache,
                    culture,
                    _fileProvider,
                    _resourcePath,
                    PathName,
                    _EnabledFiles);
        }

        /// <inheritdoc />
        public override IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        /// <summary>
        /// Returns all strings in the specified culture.
        /// </summary>
        /// <param name="includeParentCultures"></param>
        /// <param name="culture">The <see cref="CultureInfo"/> to get strings for.</param>
        /// <returns>The strings.</returns>
        protected new IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var resourceNames = base.GetAllStrings(includeParentCultures, culture).Select(w=>w.Name);

            foreach (var name in resourceNames)
            {
                var value = GetStringSafely(name, culture);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        private ConcurrentDictionary<string, string> GetFileChangeTokenResource(string cultureName, out IChangeToken CToken)
        {
            string subPath = $"{_resourcePath}{PathName.Replace('.', '/')}.{cultureName}.resx";
            ConcurrentDictionary<string, string> _cultureResourceCache = GetFileResource(subPath, out CToken);
            if (_cultureResourceCache == null)
            {
                var dotsubPath = $"{_resourcePath}{PathName}.{cultureName}.resx";
                _cultureResourceCache = GetFileResource(dotsubPath, out CToken);
                if (_cultureResourceCache == null)
                {
                    CToken = _fileProvider.Watch(subPath);
                }
            }
            return _cultureResourceCache;
        }

        private ConcurrentDictionary<string, string> GetFileResource(string subPath, out IChangeToken CToken)
        {
            ConcurrentDictionary<string, string> _cultureResourceCache = null;
            var vFileInfo = _fileProvider.GetFileInfo(subPath);
            if (vFileInfo.Exists)
            {
                var stream = vFileInfo.CreateReadStream();
                XElement XRoot = XElement.Load(stream);
                stream.Dispose();
                var rData = XRoot.Elements("data").Where(w => w.Attribute("name") != null && w.Element("value") != null);
                if (rData.Count() > 0)
                {
                    _cultureResourceCache = new ConcurrentDictionary<string, string>();
                    foreach (var item in rData)
                    {
                        _cultureResourceCache.TryAdd(item.Attribute("name").Value, item.Element("value").Value);
                    }
                }
                CToken = _fileProvider.Watch(subPath);
            }
            else
                CToken = null;
            return _cultureResourceCache;
        }
    }
}
