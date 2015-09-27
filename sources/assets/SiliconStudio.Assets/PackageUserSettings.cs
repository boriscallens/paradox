// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Settings;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A class representing the user settings related to a <see cref="Package"/>. These settings are stored in a .user
    /// file along the package file.
    /// </summary>
    public class PackageUserSettings
    {
        private const string SettingsExtension = ".pdxpkg.user";
        private readonly Package package;
        private readonly SettingsProfile profile;

        public static SettingsContainer SettingsContainer = new SettingsContainer();

        internal PackageUserSettings(Package package)
        {
            if (package == null) throw new ArgumentNullException("package");
            this.package = package;
            if (package.FullPath == null)
            {
                profile = SettingsContainer.CreateSettingsProfile(false);
            }
            else
            {
                var path = Path.Combine(package.FullPath.GetFullDirectory(), package.FullPath.GetFileName() + SettingsExtension);
                try
                {
                    profile = SettingsContainer.LoadSettingsProfile(path, false);
                }
                catch (Exception e)
                {
                    e.Ignore();
                }
                if (profile == null)
                    profile = SettingsContainer.CreateSettingsProfile(false);
            }
        }

        public bool Save()
        {
            if (package.FullPath == null)
                return false;

            var path = Path.Combine(package.FullPath.GetFullDirectory(), package.FullPath.GetFileName() + SettingsExtension);
            return SettingsContainer.SaveSettingsProfile(profile, path);
        }

        public SettingsProfile Profile { get { return profile; } }

        public T GetValue<T>(SettingsKey<T> key)
        {
            return key.GetValue(profile, true);
        }

        public void SetValue<T>(SettingsKey<T> key, T value)
        {
            key.SetValue(value, profile);
        }
    }
}