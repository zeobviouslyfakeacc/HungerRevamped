using MelonLoader;
using UnityEngine;

namespace HungerRevamped {
	internal class HungerRevampedMod : MelonMod {

		public override void OnApplicationStart() {
			CustomModeSettings.Initialize();
			MenuSettings.Initialize();
			Debug.Log($"[{Info.Name}] version {Info.Version} loaded!");
		}
	}
}
