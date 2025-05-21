using HarmonyLib;
using CulturalRecruitmentRestraints.Settings;
using CulturalRecruitmentRestraints.Utils;
using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CulturalRecruitmentRestraints
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = $"{nameof(CulturalRecruitmentRestraints)}.harmony";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                SubModule.harmony = new Harmony(HarmonyId);
                SubModule.harmony.PatchAll();

                RecruitmentCampaignBehaviorPatch.Patch(SubModule.harmony);
            }
            catch (Exception ex)
            {
                IM.ShowError("CulturalRecruitmentRestraints patch error", "OnSubModuleLoad", ex);
            }
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
        //protected override void OnGameStart(Game game, IGameStarter gameStarter)
        //{
        //    base.OnGameStart(game, gameStarter);

        //    if (game.GameType is Campaign && gameStarter is CampaignGameStarter campaignGameStarter)
        //    {
        //        campaignGameStarter.AddBehavior(new ArmouryBehavior());
        //    }
        //}

        public static Harmony? harmony;
    }
}