using Harmony;
using Newtonsoft.Json;
using UnityEngine;

namespace HungerRevamped {
	internal class SerializationPatches {

		public static void OnLoad() {
			AccessTools.Field(typeof(Hunger), "m_HungerSaveDataProxy").SetValue(null, new HungerRevampedSaveDataProxy());

			Debug.Log("HungerRevamped loaded!");
		}

		[HarmonyPatch(typeof(Hunger), "Deserialize")]
		private static class HungerDeserialize {
			private static bool Prefix(Hunger __instance, string text) {
				if (text == null)
					return false;

				HungerRevampedSaveDataProxy saveProxy = JsonConvert.DeserializeObject<HungerRevampedSaveDataProxy>(text);
				HungerRevamped.Instance.storedCalories = saveProxy.storedCalories + Tuning.defaultStoredCalories;
				return true;
			}

			private static void Postfix(Hunger __instance) {
				__instance.m_CurrentReserveCalories = Mathf.Min(__instance.m_CurrentReserveCalories, __instance.m_MaxReserveCalories);
			}
		}

		[HarmonyPatch(typeof(Hunger), "Serialize")]
		private static class HungerSerialize {
			private static void Postfix(ref string __result) {
				HungerRevampedSaveDataProxy saveData = (HungerRevampedSaveDataProxy) AccessTools.Field(typeof(Hunger), "m_HungerSaveDataProxy").GetValue(null);
				saveData.storedCalories = HungerRevamped.Instance.storedCalories - Tuning.defaultStoredCalories;
				__result = JsonConvert.SerializeObject(saveData);
			}
		}
	}
}
