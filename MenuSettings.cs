﻿using ModSettings;

namespace HungerRevamped {
    internal class MenuSettings : JsonModSettings {
        [Name("Calorie Burn Rate Multiplier")]
        [Description("Multiplies the rate at which you expend calories. Default = 1")]
        [Slider(0.1f, 3f, 30)]
        public float calorieBurnRateMultiplier = 1f;

        [Name("Can eat ruined food")]
        [Description("All ruined food is inedible and useless except for harvesting recycled cans.")]
        public bool canEatRuinedFood = false;

        [Name("Can cook ruined food")]
        [Description("Note: If no, you will still be able to place the food on the fire and \"cook\" the item. However, the cooked item will still be ruined.")]
        public bool canCookRuinedFood = false;

        [Name("Realistic Food Poisoning Chance")]
        [Description("Reworks the food poisoning probabilities to be more gradual. 75% and higher is still risk free. 45% is low risk. 25% is medium risk. 15% and lower is high risk. Risk scales with the amount eaten.")]
        public bool realisticFoodPoisoningChance = true;

        [Name("Fix Cooking Skill Exploit")]
        [Description("Skill points are awarded by proportional probability. For example, cooking 0.5 kilograms of meat has a 50% probability of giving a skill point. Cooking ruined food will give no points.")]
        public bool fixCookingSkillExploit = true;

        [Name("Cooking Doubles Condition")]
        [Description("Rather than increasing the condition by 50%, condition is doubled.")]
        public bool cookingDoublesCondition = true;

        internal static MenuSettings settings;

        internal static void Initialize() {
            settings = new MenuSettings();
            settings.AddToModSettings("Hunger Revamped");
        }
    }
}
