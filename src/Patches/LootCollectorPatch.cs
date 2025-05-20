using HarmonyLib;
using TaleWorlds.CampaignSystem.Party;
using CulturalRecruitmentRestraints.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using System.Reflection;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace CulturalRecruitmentRestraints
{
    class LootCollectorPatch
    {
        public static float GetPartySavePrisonerAsMemberShareProbability(PartyBase winnerParty, float lootAmount)
        {
            float result = lootAmount;
            if (winnerParty.IsMobile && (winnerParty.MobileParty.IsVillager || winnerParty.MobileParty.IsCaravan || winnerParty.MobileParty.IsMilitia || (winnerParty.MobileParty.IsBandit && winnerParty.MobileParty.CurrentSettlement != null && winnerParty.MobileParty.CurrentSettlement.IsHideout)))
            {
                result = 0f;
            }
            return result;
        }

        public static void Postfix(object __instance, TroopRoster memberRoster, PartyBase winnerParty, float lootAmount, MapEvent mapEvent)
        {
            //IM.WriteMessage("GiveShareOfLootToParty Postfix", IM.MsgType.Notify);

            //LootCollector
            var type = AccessTools.TypeByName("TaleWorlds.CampaignSystem.MapEvents.LootCollector");
            if (type == null)
                return;

            //获取 LootedPrisoners 属性
            PropertyInfo prop = AccessTools.Property(type, "LootedPrisoners");
            if (prop == null)
                return;

            float partySavePrisonerAsMemberShareProbability = GetPartySavePrisonerAsMemberShareProbability(winnerParty, lootAmount);

            bool flag = winnerParty == PartyBase.MainParty;
            bool flag3 = winnerParty.IsMobile && winnerParty.MobileParty.IsGarrison;
            bool flag7 = winnerParty.IsMobile && winnerParty.MobileParty.IsBandit;
            int num = (winnerParty.IsMobile ? winnerParty.MobileParty.LimitedPartySize : winnerParty.PartySizeLimit);

            if (partySavePrisonerAsMemberShareProbability > 0f)
            {
                // 读取属性值
                TroopRoster _LootedPrisoners = (TroopRoster)prop.GetValue(__instance);

                // 获取胜利方的领袖文化（安全校验版本）
                CultureObject? winnerCulture = null;
                if (winnerParty.IsMobile && winnerParty.MobileParty != null)
                {
                    Hero leaderHero = winnerParty.MobileParty.LeaderHero;
                    if (leaderHero != null)
                    {
                        winnerCulture = leaderHero.Culture;
                    }
                }

                for (int j = _LootedPrisoners.Count - 1; j >= 0; j--)
                {
                    int elementNumber = _LootedPrisoners.GetElementNumber(j);
                    CharacterObject characterAtIndex = _LootedPrisoners.GetCharacterAtIndex(j);

                    if (winnerCulture != null && characterAtIndex.Culture != winnerCulture)
                    {
                        IM.WriteMessage("文化不符合：" + characterAtIndex.Name.ToString(), IM.MsgType.Notify);
                        continue;
                    }

                    int num2 = 0;
                    for (int k = 0; k < elementNumber; k++)
                    {
                        bool flag8 = characterAtIndex.IsHero && characterAtIndex.HeroObject.IsReleased;
                        bool flag9 = flag7 && characterAtIndex.Occupation != Occupation.Bandit;
                        bool flag10 = flag3 && characterAtIndex.Occupation == Occupation.Bandit;
                        if (!flag8 && !flag9 && !flag10 && MBRandom.RandomFloat < partySavePrisonerAsMemberShareProbability)
                        {
                            if (!flag && memberRoster.TotalManCount + 1 > num)
                            {
                                break;
                            }
                            if (characterAtIndex.IsHero && !flag)
                            {
                                EndCaptivityAction.ApplyByReleasedAfterBattle(characterAtIndex.HeroObject);
                            }
                            else
                            {
                                memberRoster.AddToCounts(characterAtIndex, 1, false, 0, 0, true, -1);
                            }
                            num2++;
                        }
                    }
                    if (num2 > 0)
                    {
                        _LootedPrisoners.AddToCounts(characterAtIndex, -num2, false, 0, 0, true, -1);
                    }
                }
            }
        }
    }
}
