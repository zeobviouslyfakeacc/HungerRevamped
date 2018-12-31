using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace HungerRevamped {
	internal static class SerializationPatches {

		private static readonly FieldInfo m_HungerSaveDataProxy = AccessTools.Field(typeof(Hunger), "m_HungerSaveDataProxy");

		public static void OnLoad() {
			m_HungerSaveDataProxy.SetValue(null, new HungerRevampedSaveDataProxy());

			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Debug.Log("[HungerRevamped] Version " + version + " loaded!");
		}

		[HarmonyPatch(typeof(Hunger), "Deserialize")]
		private static class HungerDeserialize {

			private static bool Prefix(Hunger __instance, string text) {
				if (text == null)
					return false;

				HungerRevamped hungerRevamped = HungerRevamped.Instance;
				HungerRevampedSaveDataProxy saveProxy = Utils.DeserializeObject<HungerRevampedSaveDataProxy>(text);

				hungerRevamped.storedCalories = saveProxy.storedCalories + Tuning.defaultStoredCalories;
				hungerRevamped.wellFedHungerScore = saveProxy.wellFedHungerScore;
				hungerRevamped.deferredFoodPoisonings.Clear();
				if (saveProxy.deferredFoodPoisonings != null) {
					hungerRevamped.deferredFoodPoisonings.AddRange(saveProxy.deferredFoodPoisonings);
				}

				return true;
			}

			private static void Postfix(Hunger __instance) {
				__instance.m_CurrentReserveCalories = Mathf.Min(__instance.m_CurrentReserveCalories, __instance.m_MaxReserveCalories);
			}
		}

		[HarmonyPatch(typeof(Hunger), "Serialize")]
		private static class HungerSerialize {

			private static void Postfix(ref string __result) {
				HungerRevamped hungerRevamped = HungerRevamped.Instance;
				HungerRevampedSaveDataProxy saveData = (HungerRevampedSaveDataProxy) m_HungerSaveDataProxy.GetValue(null);

				saveData.storedCalories = hungerRevamped.storedCalories - Tuning.defaultStoredCalories;
				saveData.wellFedHungerScore = hungerRevamped.wellFedHungerScore;
				saveData.deferredFoodPoisonings = hungerRevamped.deferredFoodPoisonings.ToArray();

				__result = Utils.SerializeObject(saveData);
			}
		}
	}
}
