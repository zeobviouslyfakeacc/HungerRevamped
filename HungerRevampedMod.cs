using MelonLoader;
using UnityEngine;

namespace HungerRevamped {
    internal sealed class HungerRevampedMod : MelonMod
    {
        internal static SaveDataManager sdm = new SaveDataManager();

        public override void OnInitializeMelon()
        {
            CustomModeSettings.Initialize();
            MenuSettings.Initialize();
            Debug.Log($"[{Info.Name}] version {Info.Version} loaded");
            new MelonLoader.MelonLogger.Instance($"{Info.Name}").Msg($"Version {Info.Version} loaded");
        }
    }
}
