using HarmonyLib;
using System;
using Il2Cpp;

namespace HungerRevamped {
	internal static class PreventCookingRuinedFoodPatches {

		[HarmonyPatch(typeof(CookingPotItem), "SetCookedGearProperties", new Type[] { typeof(GearItem), typeof(GearItem) })]
		private static class RuinedFoodRemainsRuinedWhenCooked {
			private static void Postfix(GearItem rawItem, GearItem cookedItem) {
				if (MenuSettings.settings.canCookRuinedFood) return;
				if (!rawItem || !cookedItem) return;

				if (rawItem.IsWornOut()) {
					cookedItem.ForceWornOut();
					cookedItem.UpdateDamageShader();
				}
			}
		}

		[HarmonyPatch(typeof(Cookable), "MaybeStartWarmingUpDueToNearbyFire")]
		private static class PreventWarmingUpRuinedFood {
			private static bool Prefix(Cookable __instance) {
				if (MenuSettings.settings.canCookRuinedFood) return true;
				GearItem gearItem = __instance.GetComponent<GearItem>();
				return !gearItem.IsWornOut(); // Do not run original method when item is ruined
			}
		}
	}
}
