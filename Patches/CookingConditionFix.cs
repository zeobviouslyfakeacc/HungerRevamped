using Harmony;
using UnityEngine;

namespace HungerRevamped {
	[HarmonyPatch(typeof(CookingPotItem),"SetCookedGearProperties")]
	internal class CookingConditionFix {
		private static void Postfix(GearItem rawItem,GearItem cookedItem) {
			if (!MenuSettings.settings.cookingDoublesCondition || !rawItem || !cookedItem) return;
			cookedItem.m_CurrentHP = cookedItem.m_MaxHP * Mathf.Clamp01(2 * rawItem.m_CurrentHP / rawItem.m_MaxHP);
		}
	}
}
