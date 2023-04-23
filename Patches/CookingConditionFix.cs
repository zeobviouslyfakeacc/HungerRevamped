using HarmonyLib;
using UnityEngine;
using Il2Cpp;

namespace HungerRevamped
{

    [HarmonyPatch(typeof(CookingPotItem), "SetCookedGearProperties")]
    internal class CookingConditionFix
    {

        private static readonly MelonLoader.MelonLogger.Instance logger = new MelonLoader.MelonLogger.Instance(BuildInfo.ModName);

        private static void Postfix(GearItem rawItem, GearItem cookedItem)
        {            
            if (MenuSettings.settings.cookingDoublesCondition && rawItem && cookedItem)
            {
                logger.Msg($"Applying cooking item doubles condition fix");

                var rawItemHp = rawItem.CurrentHP;
                var rawItemMax = rawItem.GearItemData.m_MaxHP;
                var cookedItemMax = cookedItem.GearItemData.m_MaxHP;
                var cookedResultHp = cookedItemMax * Mathf.Clamp01(2 * rawItemHp / rawItemMax);

                logger.Msg($"raw hp: {rawItemHp}, raw max: {rawItemMax}, cooked max: {cookedItemMax}, calculated: {cookedResultHp}");
                cookedItem.CurrentHP = cookedResultHp;
            }
        }
    }
}
