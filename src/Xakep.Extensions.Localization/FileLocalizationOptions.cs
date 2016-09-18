// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;

namespace Xakep.Extensions.Localization 
{
    /// <summary>
    /// Provides programmatic configuration for localization.
    /// </summary>
    public class FileLocalizationOptions : LocalizationOptions
    {

        /// <summary>
        /// *,resx file read provider abstraction.
        /// </summary>
        public IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Enable *.resx resource file
        /// </summary>
        public bool EnabledFiles { get; set; } = true;

    }
}
