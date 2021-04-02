using ModSettings;

namespace HungerRevamped {
	internal class MenuSettings : JsonModSettings {
		[Name("Can eat ruined food")]
		[Description("All ruined food is inedible and cannot be used. Ruined canned food items can still be harvested for a recycled can.")]
		public bool canEatRuinedFood = false;

		[Name("Can cook ruined food")]
		[Description("Note: If no, you will still be able to place the food on the fire and \"cook\" the item. However, the cooked item will remain ruined.")]
		public bool canCookRuinedFood = false;

		[Name("Delayed Food Poisoning")]
		[Description("Adds an incubation time of several hours to the Food Poisoning affliction.")]
		public bool delayedFoodPoisoning = true;

		[Name("Gradual Food Poisoning Probability")]
		[Description("Reworks the food poisoning probabilities to be more gradual. 75% and higher is still risk free. 45% is low risk. 25% is medium risk. 15% and lower is high risk. Risk scales with the amount eaten.")]
		public bool realisticFoodPoisoningChance = true;

		[Name("Remove Food Poisoning Immunity")]
		[Description("If enabled, makes it possible to still get food poisoning after achieving level 5 of the Cooking skill.")]
		public bool removeFoodPoisonImmunity = false;

		[Name("Fix Cooking Skill Exploit")]
		[Description("Skill points are awarded by proportional probability. For example, cooking 0.5 kilograms of meat has a 50% probability of giving a skill point. Cooking ruined food will give no points.")]
		public bool fixCookingSkillExploit = true;

		[Name("Cooking Doubles Condition")]
		[Description("Rather than increasing the condition by 50%, the condition of the food item being cooked is doubled.")]
		public bool cookingDoublesCondition = true;

		internal static MenuSettings settings;

		internal static void Initialize() {
			settings = new MenuSettings();
			settings.AddToModSettings("Hunger Revamped");
		}
	}
}
