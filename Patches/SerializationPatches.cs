using HarmonyLib;
using MelonLoader.TinyJSON;
using UnityEngine;
using Il2Cpp;


namespace HungerRevamped
{
    internal static class SerializationPatches
    {

        private static readonly HungerRevampedSaveDataProxy saveDataProxy = new HungerRevampedSaveDataProxy();

        private static readonly MelonLoader.MelonLogger.Instance logger = new MelonLoader.MelonLogger.Instance(BuildInfo.ModName);


        [HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData")]
        private static class HungerDeserialize
        {

            private static void Postfix(string name)
            {
                logger.Msg($"Attempting to load {name}");

                string? json = HungerRevampedMod.sdm.LoadData();

                if (json == null)
                {
                    logger.Msg("Could not load!");
                    return;
                }
                logger.Msg(json);
                logger.Msg("Loaded Successfully");

                HungerRevamped hungerRevamped = HungerRevamped.Instance;
                JSON.Load(json).Populate(saveDataProxy);

                hungerRevamped.storedCalories = saveDataProxy.storedCalories + Tuning.defaultStoredCalories;
                hungerRevamped.wellFedHungerScore = saveDataProxy.wellFedHungerScore;
                hungerRevamped.deferredFoodPoisonings.Clear();
                if (saveDataProxy.deferredFoodPoisonings != null)
                {
                    hungerRevamped.deferredFoodPoisonings.AddRange(saveDataProxy.deferredFoodPoisonings);
                }

                Hunger hunger = GameManager.GetHungerComponent();
                hunger.m_CurrentReserveCalories = Mathf.Min(hunger.m_CurrentReserveCalories, hunger.m_MaxReserveCalories);
                logger.Msg($"Current reserve calories: {hunger.m_CurrentReserveCalories}");
                logger.Msg($"Max reserve calories: {hunger.m_MaxReserveCalories}");
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData")]
        private static class HungerSerialize
        {

            [HarmonyPostfix]
            private static void Postfix(SlotData slot)
            {
                logger.Msg($"Attempting to save");

                HungerRevamped hungerRevamped = HungerRevamped.Instance;

                saveDataProxy.storedCalories = hungerRevamped.storedCalories - Tuning.defaultStoredCalories;
                saveDataProxy.wellFedHungerScore = hungerRevamped.wellFedHungerScore;
                saveDataProxy.deferredFoodPoisonings = hungerRevamped.deferredFoodPoisonings.ToArray();

                string json = JSON.Dump(saveDataProxy, EncodeOptions.NoTypeHints);
                MelonLoader.MelonLogger.Msg(json);

                bool success = HungerRevampedMod.sdm.Save(json);
                if (success)
                    logger.Msg("Success");
                else
                    logger.Msg("Failed to save");
            }
        }
    }
}
