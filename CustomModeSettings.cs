using ModSettings;

namespace HungerRevamped {
	internal class CustomModeSettings : ModSettingsBase {

		[Name("Starting stored calories")]
		[Slider(0f, 20_000f, 21, NumberFormat = "{0:F0}")]
		public float startingStoredCalories = 12_000f;

		internal static CustomModeSettings settings;

		internal static void Initialize() {
			settings = new CustomModeSettings();
			settings.AddToCustomModeMenu(Position.BelowGameStart);
		}
	}
}
