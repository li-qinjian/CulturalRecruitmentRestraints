using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

[HarmonyPatch(typeof(RecruitmentCampaignBehavior), "UpdateVolunteersOfNotablesInSettlement")]
public static class RecruitmentCampaignBehaviorPatch
{
    // 王国骑兵升级概率配置（键：王国ID，值：允许升级为骑兵的概率）
    public static Dictionary<string, float> KingdomCavalryProbability = new Dictionary<string, float>
    {
        {"empire_w", 0.2f},
        {"empire_s", 0.24f},
        {"empire", 0.22f},
        {"sturgian", 0.12f},
        {"khuzait", 0.4f},
        {"aserai", 0.28f},
        {"battanian", 0.1f},
        {"vlandian", 0.18f}
    };

    [HarmonyPrefix]
    public static bool Prefix(
        RecruitmentCampaignBehavior __instance, 
        Settlement settlement
    ) {
        // 仅当定居点非叛乱状态时执行（与原版逻辑一致）
        bool isSettlementStable = 
            (settlement.IsTown && !settlement.Town.InRebelliousState) || 
            (settlement.IsVillage && !settlement.Village.Bound.Town.InRebelliousState);

        if (!isSettlementStable) {
            return true; // 跳过非稳定定居点的处理
        }

        // 遍历定居点所有贵族
        foreach (Hero noble in settlement.Notables) {
            // 过滤条件：贵族可招募士兵且存活（原版遗漏了IsAlive检查，此处是否修复需确认）
            if (!noble.CanHaveRecruits || !noble.IsAlive) {
                continue;
            }

            bool isVolunteerUpdated = false;
            CharacterObject baseVolunteer = Campaign.Current.Models.VolunteerModel.GetBasicVolunteer(noble);

            // 处理6个等级的志愿兵槽位
            for (int tierIndex = 0; tierIndex < 6; tierIndex++) {
                // 检查每日生成/升级概率
                float spawnProbability = Campaign.Current.Models.VolunteerModel.GetDailyVolunteerProductionProbability(noble, tierIndex, settlement);
                if (MBRandom.RandomFloat >= spawnProbability) {
                    continue; // 未触发生成/升级
                }

                CharacterObject currentVolunteer = noble.VolunteerTypes[tierIndex];
                if (currentVolunteer == null) {
                    // 填充基础志愿兵
                    noble.VolunteerTypes[tierIndex] = baseVolunteer;
                    isVolunteerUpdated = true;
                } else {
                    // 计算升级相关参数（限制最高3级）
                    int currentTier = currentVolunteer.Tier;
                    if (currentTier > 3 || currentVolunteer.UpgradeTargets.Length == 0) {
                        continue; // 超过等级限制或无升级路径
                    }

                    // 计算升级概率（与贵族权力平方成反比）
                    float power = Math.Max(50f, noble.Power);
                    float upgradeChanceDenominator = power * power;
                    float randomThreshold = Math.Max(2f, currentTier * (40000f / upgradeChanceDenominator));
                    
                    if (MBRandom.RandomInt((int)randomThreshold) != 0) {
                        continue; // 未通过升级随机检查
                    }

                    // 尝试升级为骑兵或步兵（根据王国特性）
                    CharacterObject targetVolunteer = currentVolunteer.UpgradeTargets[MBRandom.RandomInt(currentVolunteer.UpgradeTargets.Length)];
                    bool isUpgradingToCavalry = 
                        !currentVolunteer.IsMounted && 
                        targetVolunteer.IsMounted && 
                        currentVolunteer.UpgradeTargets.Length != 1;

                    if (isUpgradingToCavalry) {
                        // 根据王国ID获取骑兵升级允许概率
                        float cavalryChance = KingdomCavalryProbability.TryGetValue(
                            settlement.OwnerClan.Kingdom.StringId, 
                            out float value) ? value : 0.2f;

                        if (MBRandom.RandomFloat >= cavalryChance) {
                            continue; // 王国特性不允许升级为骑兵
                        }
                    }

                    // 应用升级
                    noble.VolunteerTypes[tierIndex] = targetVolunteer;
                    isVolunteerUpdated = true;
                }
            }

            // 当志愿兵有更新时进行等级排序
            if (isVolunteerUpdated) {
                CharacterObject[] volunteerSlots = noble.VolunteerTypes;
                
                // 冒泡排序：按等级从高到低排列（骑兵等级+0.5权重）
                for (int sortIndex = 1; sortIndex < 6; sortIndex++) {
                    CharacterObject currentUnit = volunteerSlots[sortIndex];
                    if (currentUnit == null) {
                        continue; // 跳过空槽位
                    }

                    int insertPosition = sortIndex;
                    float currentUnitWeight = currentUnit.Level + (currentUnit.IsMounted ? 0.5f : 0f);

                    // 向前查找合适的插入位置
                    while (insertPosition > 0) {
                        int prevIndex = insertPosition - 1;
                        CharacterObject prevUnit = volunteerSlots[prevIndex];
                        float prevUnitWeight = prevUnit?.Level + (prevUnit.IsMounted ? 0.5f : 0f) ?? 0f;

                        if (currentUnitWeight >= prevUnitWeight) {
                            break; // 找到正确位置
                        }

                        // 移动较高等级的单位向后
                        volunteerSlots[insertPosition] = prevUnit;
                        insertPosition = prevIndex;
                    }

                    volunteerSlots[insertPosition] = currentUnit; // 插入当前单位
                }
            }
        }

        return false; // 阻止原方法执行（完全由补丁替代）
    }

    // 招募前置检查：禁止从敌对派系定居点招募
    [HarmonyPatch(typeof(RecruitmentCampaignBehavior), "CheckRecruiting")]
    [HarmonyPrefix]
    public static bool CheckRecruitingPreFix(
        RecruitmentCampaignBehavior __instance, 
        MobileParty recruitingParty, 
        Settlement targetSettlement
    ) {
        return recruitingParty.MapFaction == targetSettlement.MapFaction; // 仅允许同派系招募
    }

    // 辅助方法：获取王国骑兵升级概率（原CanNotUpgrade逻辑优化）
    private static bool ShouldBlockCavalryUpgrade(string kingdomId) {
        if (!KingdomCavalryProbability.TryGetValue(kingdomId, out float allowedChance)) {
            allowedChance = 0.2f; // 默认概率
        }
        return MBRandom.RandomFloat >= allowedChance; // 反向判断是否阻止
    }
}