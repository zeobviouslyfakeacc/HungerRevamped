using Harmony;
using MelonLoader.TinyJSON;
using UnityEngine;

namespace HungerRevamped {
	internal static class SerializationPatches {

		private const string SAVE_SLOT_NAME = "MOD_HungerRevamped";
		private static readonly HungerRevampedSaveDataProxy saveDataProxy = new HungerRevampedSaveDataProxy();

		[HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData")]
		private static class HungerDeserialize {

			private static void Postfix(string name) {
				string json = SaveGameSlots.LoadDataFromSlot(name, SAVE_SLOT_NAME);

				if (string.IsNullOrEmpty(json))
					return;

				HungerRevamped hungerRevamped = HungerRevamped.Instance;
				JSON.Load(json).Populate(saveDataProxy);

				hungerRevamped.storedCalories = saveDataProxy.storedCalories + Tuning.defaultStoredCalories;
				hungerRevamped.wellFedHungerScore = saveDataProxy.wellFedHungerScore;
				hungerRevamped.deferredFoodPoisonings.Clear();
				if (saveDataProxy.deferredFoodPoisonings != null) {
					hungerRevamped.deferredFoodPoisonings.AddRange(saveDataProxy.deferredFoodPoisonings);
				}

				Hunger hunger = GameManager.GetHungerComponent();
				hunger.m_CurrentReserveCalories = Mathf.Min(hunger.m_CurrentReserveCalories, hunger.m_MaxReserveCalories);
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData")]
		private static class HungerSerialize {

			private static void Postfix(SaveSlotType gameMode, string name) {
				HungerRevamped hungerRevamped = HungerRevamped.Instance;

				saveDataProxy.storedCalories = hungerRevamped.storedCalories - Tuning.defaultStoredCalories;
				saveDataProxy.wellFedHungerScore = hungerRevamped.wellFedHungerScore;
				saveDataProxy.deferredFoodPoisonings = hungerRevamped.deferredFoodPoisonings.ToArray();

				string json = JSON.Dump(saveDataProxy, EncodeOptions.NoTypeHints);
				SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, SAVE_SLOT_NAME, json);
			}
		}
	}
}
