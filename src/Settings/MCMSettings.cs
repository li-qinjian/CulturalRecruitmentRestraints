using Bannerlord.BUTR.Shared.Helpers;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System.Collections.Generic;
using MCM.Abstractions;
using MCM.Common;
using TaleWorlds.Localization;
using MCM.Abstractions.Attributes.v1;

namespace CulturalRecruitmentRestraints.Settings
{
    public class MCMSettings : AttributeGlobalSettings<MCMSettings>
    {
        #region ModSettingsStandard

        public override string Id => Statics.InstanceID;

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        private string modName = Statics.DisplayName;

        public override string DisplayName => TextObjectHelper.Create(StringConstants.ModDisplayName + modName + " {VERSION}", new Dictionary<string, TextObject>()
        {
            { "VERSION", TextObjectHelper.Create(typeof(MCMSettings).Assembly.GetName().Version?.ToString(3) ?? "")! }
        })!.ToString();

#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        public override string FolderName => Statics.ModuleFolder;
        public override string FormatType => Statics.FormatType;

        public bool LoadMCMConfigFile { get; set; } = false;
        public string ModDisplayName
        { get { return DisplayName; } }

        #endregion ModSettingsStandard

        ///~ Mod Specific settings

        //~ Debugging
        #region Debugging

        private bool _Debug = false;
        [SettingPropertyBool(StringConstants.Settings_Debug_MSG, IsToggle = false, Order = 0, RequireRestart = false, HintText = StringConstants.Settings_Debug_MSG)]
        [SettingPropertyGroup(StringConstants.PG_Debug, GroupOrder = 100)]
        public bool Debug
        {
            get => _Debug;
            set
            {
                if (_Debug != value)
                {
                    _Debug = value;
                    OnPropertyChanged(nameof(Debug)); // 始终触发通知
                }
            }
        }

        [SettingPropertyBool(StringConstants.Settings_Debug_LOG, IsToggle = false, Order = 1, RequireRestart = false, HintText = StringConstants.Settings_Debug_LOG)]
        [SettingPropertyGroup(StringConstants.PG_Debug, GroupOrder = 100)]
        public bool LogToFile { get; set; } = false;

        #endregion Debugging


        //~ Sundry Options
        #region Sundry Options

        [SettingPropertyBool(StringConstants.Settings_Sundry_01, Order = 0, RequireRestart = false, HintText = StringConstants.Settings_Sundry_01_Desc)]
        [SettingPropertyGroup(StringConstants.PG_Sundry)]
        public bool EnableCRR { get; set; } = false;

        #endregion Sundry Options


        #region Presets

        public override IEnumerable<ISettingsPreset> GetBuiltInPresets()
        {
            foreach (var preset in base.GetBuiltInPresets())
            {
                yield return preset;
            }

            yield return new MemorySettingsPreset(Id, "native all off", "Native All Off", () => new MCMSettings
            {
                EnableCRR = false,
            }); ;

            yield return new MemorySettingsPreset(Id, "native all on", "Native All On", () => new MCMSettings
            {
                EnableCRR = true,
            });
        }

        #endregion Presets

        public MCMSettings()
        {
            PropertyChanged += MCMSettings_PropertyChanged;
        }

        private void MCMSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Debug))
            {
                LogToFile = false;
            }
        }
    }
}