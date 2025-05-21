using Bannerlord.BUTR.Shared.Helpers;
using CulturalRecruitmentRestraints.Settings;
using System.Reflection;

namespace CulturalRecruitmentRestraints
{
    public static class Statics
    {
        public static MCMSettings? _settings;

        public const string ModuleFolder = "CulturalRecruitmentRestraints";
        public const string InstanceID = ModuleFolder;
        public const string DisplayName = "CulturalRecruitmentRestraints";
        public const string FormatType = "json";
        public const string logPath = @"..\\..\\Modules\\" + ModuleFolder + "\\ModLog.txt";
        public const string ConfigFilePath = @"..\\..\\Modules\\" + ModuleFolder + "\\config.json";
        public static string PrePrend { get; set; } = DisplayName;

        //public const string HarmonyId = ModuleFolder + ".harmony";
        public static string GameVersion = ApplicationVersionHelper.GameVersionStr();
        public static string ModVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public const bool UsesHarmony = true;

        #region MCMConfigValues
        public static string? MCMConfigFolder { get; set; }
        public static string? MCMConfigFile { get; set; }
        public static bool MCMConfigFileExists { get; set; } = false;
        public static bool MCMModuleLoaded { get; set; } = false;
        public static bool ModConfigFileExists { get; set; } = false;
        #endregion
    }
}