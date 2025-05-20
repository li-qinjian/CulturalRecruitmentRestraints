using HarmonyLib;
using TaleWorlds.CampaignSystem.Party;
using CulturalRecruitmentRestraints.Utils;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.MapEvents;

namespace CulturalRecruitmentRestraints
{
    [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.MapEvents.MapEventSide), "CaptureWoundedTroops")]
    public class CaptureWoundedTroops_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(object lootCollector, PartyBase defeatedParty, ref bool isSurrender, ref bool playerCaptured)
        {
            if (defeatedParty.LeaderHero != null && !isSurrender)
            {
                if (defeatedParty.LeaderHero.IsWounded)
                {
                    IM.WriteMessage(defeatedParty.Name + "领袖受伤", IM.MsgType.Notify);
                    if (defeatedParty.MemberRoster.TotalHealthyCount > 10)
                        defeatedParty.LeaderHero.HitPoints += 20;
                }

            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.MapEvents.MapEventSide), "CalculateContributionAndGiveShareToParty")]
    public class CalculateContributionAndGiveShareToParty_Patch
    {
        private static Harmony harmony = new Harmony("internal_class_haromony");
        public static bool _harmonyPatchApplied = false;

        [HarmonyPrefix]
        public static bool Prefix(object lootCollector, MapEventParty partyRec, int totalContribution)
        {
            if (_harmonyPatchApplied)
                return true;

            var original = lootCollector.GetType().GetMethod("GiveShareOfLootToParty", AccessTools.all);
            var postfix = typeof(LootCollectorPatch).GetMethod("Postfix");
            if (original != null && postfix != null)
                harmony.Patch(original, postfix: new HarmonyMethod(postfix));

            _harmonyPatchApplied = true; 

            return true;
        }
    }
}
