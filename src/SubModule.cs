using HarmonyLib;
using CulturalRecruitmentRestraints.Settings;
using CulturalRecruitmentRestraints.Utils;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.InputSystem;
using SandBox.GauntletUI;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using HarmonyLib.BUTR.Extensions;

namespace CulturalRecruitmentRestraints;

public class SubModule : MBSubModuleBase
{
    private const string HarmonyId = $"{nameof(CulturalRecruitmentRestraints)}.harmony";

    private readonly Lazy<Harmony> _harmony = new(() => new Harmony(HarmonyId));


    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

        //EnableAchievementsPatch:
        // 1. re-enables achievements in previously tainted savefiles
        // 2. activates achievements even if mods or cheat mode is present

        //EnableSandboxAchievementsPatch:
        // 1. loads the achievements behavior in Sandbox mode games

        //SwallowStoryModeAchievementsDuringSandboxPatch:
        // 1. prevents game from crashing while registering Story mode achievements that Sandbox doesn't have

        //SuppressModulesPatch:
        // 1. hides non-official mods from the module list, so that tainted saves can be used in vanilla and after using this mod

        //SuppressCheatIntegrityPatch:
        // 1. re-enables achievements in previously tainted savefiles (from cheating)
        // 2. passes cheat integrity check even if cheat mode is present

        //SuppressUsedVersionsPatch
        // 1. hides past used versions, so that version downgrades don't taint the save

        //ItemObjectPatch.Patch(_harmony.Value);

        _harmony.Value.PatchAll(Assembly.GetExecutingAssembly());

        _harmony.Value.TryPatch(
            AccessTools2.DeclaredMethod("TaleWorlds.CampaignSystem.CampaignBehaviors.PlayerVariablesBehavior:OnPlayerBattleEnd"),
            prefix: AccessTools2.DeclaredMethod(typeof(SubModule), nameof(SkipMethod)));
    }

    //(Second) Starts when the first loading screen is done.Called just before the main menu first appears, helpful if your mod depends on other things being set up during the initial load
    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        try
        {
            ConfigLoader.LoadConfig();
        }
        catch (Exception ex)
        {
            IM.ShowError("Error loading", "initial config", ex);
        }
    }

    //see https://docs.bannerlordmodding.lt/modding/harmony/
    //(Third) Starts as soon as you load a game.Called immediately upon loading after selecting a game mode (submodule) from the main menu
    protected override void OnGameStart(Game game, IGameStarter gameStarter)
    {
        base.OnGameStart(game, gameStarter);

        //if (game.GameType is Campaign && gameStarter is CampaignGameStarter campaignGameStarter)
        //{
        //    campaignGameStarter.AddBehavior(new ArmouryBehavior());
        //}
    }

    private static bool SkipMethod()
    {
        return false;
    }
}