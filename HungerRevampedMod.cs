using MelonLoader;
using UnityEngine;

namespace HungerRevamped {
	internal class HungerRevampedMod : MelonMod {

		public override void OnApplicationStart() {
			CustomModeSettings.Initialize();
			Debug.Log($"[{InfoAttribute.Name}] version {InfoAttribute.Version} loaded!");
		}
	}
}
