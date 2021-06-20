using HarmonyLib;
using UnityEngine;

namespace HungerRevamped {
	internal static class HarvestFoodForCanPatch {

		/*
		 * Allow harvesting the cans of canned food items. The idea is that while
		 * the food inside the can may be ruined, the can itself should still be usable.
		 */

		[HarmonyPatch(typeof(FoodItem), "Awake")]
		private static class AllowBreakingDownFoodForContainer {
			private static void Postfix(FoodItem __instance) {
				GameObject item = __instance.gameObject;
				if (item.GetComponent<Harvest>()) return; // Item can already be broken down
				if (!__instance.m_GearPrefabHarvestAfterFinishEatingNormal) return; // Not an item-rewarding food item

				GearItem resultItem = __instance.m_GearPrefabHarvestAfterFinishEatingNormal.GetComponent<GearItem>();

				Harvest harvest = item.AddComponent<Harvest>();
				harvest.m_YieldGear = new GearItem[] { resultItem };
				harvest.m_YieldGearUnits = new int[] { 1 };
				harvest.m_DurationMinutes = 5;
				harvest.m_Audio = "Play_OpenCan";

				// Retroactively cache Harvest in GearItem
				GearItem baseGear = __instance.GetComponent<GearItem>();
				baseGear.m_Harvest = harvest;
			}
		}
	}
}
