﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Settings;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Assets
{
    public static class GameUserSettings
    {
        public static class Effect
        {
            public static SettingsKey<EffectCompilationMode> EffectCompilation = new SettingsKey<EffectCompilationMode>("Package/Game/Effect/EffectCompilation", PackageUserSettings.SettingsContainer, EffectCompilationMode.LocalOrRemote)
            {
                DisplayName = "Effect Compiler"
            };
            public static SettingsKey<bool> RecordUsedEffects = new SettingsKey<bool>("Package/Game/Effect/RecordUsedEffects", PackageUserSettings.SettingsContainer, true)
            {
                DisplayName = "Record used effects"
            };
        }
    }
}