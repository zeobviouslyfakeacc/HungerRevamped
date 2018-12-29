using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace HungerRevamped {
	internal class SerializationPatches {

		public static void OnLoad() {
			AccessTools.Field(typeof(Hunger), "m_HungerSaveDataProxy").SetValue(null, new HungerRevampedSaveDataProxy());

			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Debug.Log("[HungerRevamped] Version " + version + " loaded!");
		}

		[HarmonyPatch(typeof(Hunger), "Deserialize")]
		private static class HungerDeserialize {
			private static bool Prefix(Hunger __instance, string text) {
				if (text == null)
					return false;

				HungerRevampedSaveDataProxy saveProxy = Utils.DeserializeObject<HungerRevampedSaveDataProxy>(text);
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
				__result = Utils.SerializeObject(saveData);
			}
		}
	}
}
