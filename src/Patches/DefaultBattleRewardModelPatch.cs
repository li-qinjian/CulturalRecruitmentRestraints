using HarmonyLib;
using TaleWorlds.CampaignSystem.Party;
using CulturalRecruitmentRestraints.Utils;

namespace CulturalRecruitmentRestraints
{
    [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.GameComponents.DefaultBattleRewardModel), "GetPartySavePrisonerAsMemberShareProbability")]
    public class DefaultBattleRewardModel_GetPartySavePrisonerAsMemberShareProbability_Patch
    {
        [HarmonyPrefix]
        // 前置补丁：在原方法执行前修改参数
        public static bool Prefix(PartyBase winnerParty, float lootAmount, ref float __result)
        {
            __result = 0f;

            return false;
        }
    }
}
