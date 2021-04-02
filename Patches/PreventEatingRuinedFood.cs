using System;
using Harmony;

namespace HungerRevamped {

	/*
	 * Before TLD v1.33, the game tried to prevent the player from eating ruined food items, but there were still ways
	 * around these restrictions. For example, one could drop a ruined food item on the ground and eat it by pressing the
	 * space key. Similarly, one could cook ruined meat to get 50% cooked meat items with only a low food poisoning chance.
	 *
	 * Then, in v1.33, with the introduction of the new cooking mechanics, all of these restrictions for ruined food
	 * suddenly vanished. Meat can now be stored forever at 0% condition. And when eaten after aquiring the level 5
	 * cooking skill, these items don't even carry a risk of food poisoning.
	 */

	internal static class PreventEatingRuinedFood {

		[HarmonyPatch(typeof(Panel_Inventory), "OnEquip")]
		private static class DontAllowUseAtZeroHP {
			private static bool Prefix(Panel_Inventory __instance) {
				if (MenuSettings.settings.canEatRuinedFood) return true;
				GearItem gi = __instance.GetCurrentlySelectedGearItem();
				if (gi.m_FoodItem && gi.IsWornOut()) {
					GameAudioManager.PlayGUIError();
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "CanUseFoodInventoryItem", new Type[] { typeof(GearItem) })]
		private static class PreventnUseFoodInventoryItem {
			private static bool Prefix(ref bool __result, GearItem gi) {
				if (MenuSettings.settings.canEatRuinedFood) return true;
				if (gi.IsWornOut()) {
					GameAudioManager.PlayGUIError();
					__result = false;
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(ItemDescriptionPage), "GetEquipButtonLocalizationId", new Type[] { typeof(GearItem) })]
		private static class DontShowEatButtonForRuinedFood {
			private static bool Prefix(ref string __result, GearItem gi) {
				if (MenuSettings.settings.canEatRuinedFood) return true;
				if (gi && gi.m_FoodItem && gi.IsWornOut()) {
					__result = string.Empty;
					return false;
				}
				return true;
			}
		}
	}
}
