using System;
using System.Collections.Generic;
using UnityEngine;

namespace HungerRevamped {
	public class HungerRevamped {

		private static HungerRevamped instance = null;
		internal static HungerRevamped Instance {
			get {
				if (instance == null)
					throw new NullReferenceException("HungerRevamped instance was null - accessed before Hunger#Start was called.");
				return instance;
			}
			set {
				instance = value;
			}
		}

		internal readonly Hunger hunger;
		internal readonly List<DeferredFoodPoisoning> deferredFoodPoisonings;

		internal double storedCalories;
		internal double wellFedHungerScore;

		internal HungerRevamped(Hunger hunger) {
			this.hunger = hunger;
			deferredFoodPoisonings = new List<DeferredFoodPoisoning>();

			if (GameManager.InCustomMode() || IsRelentlessNightGame()) {
				storedCalories = CustomModeSettings.settings.startingStoredCalories;
			} else {
				storedCalories = Tuning.startingStoredCalories;
			}
			wellFedHungerScore = Tuning.startingWellFedHungerScore;
		}

		private static bool IsRelentlessNightGame() {
			if (!GameManager.GetVersionString().Contains("Relentless Night")) return false;
			string typeName = System.Reflection.Assembly.CreateQualifiedName("RelentlessNight", "RelentlessNight.RnGl");
			Type rnGlobals = Type.GetType(typeName, false);
			if (rnGlobals == null) return false;
			return (bool) HarmonyLib.AccessTools.Field(rnGlobals, "rnActive").GetValue(null);
		}

		internal void Update() {
			if (GameManager.m_IsPaused)
				return;

			float todHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);

			// Adjust stored calories
			float storedCalorieChange = ClampCalorieChange(todHours * GetStoredCaloriesChangePerHour());
			storedCalories += storedCalorieChange;
			hunger.m_CurrentReserveCalories -= storedCalorieChange;

			// Make *absolutely sure* we stay in range
			storedCalories = Math.Min(Math.Max(storedCalories, 0), Tuning.maximumStoredCalories);
			hunger.m_CurrentReserveCalories = Mathf.Clamp(hunger.m_CurrentReserveCalories, 0f, hunger.m_MaxReserveCalories);

			// Maybe apply damage due to starving
			float conditionChange = todHours * GetConditionChangePerHour();
			if (conditionChange < 0f) {
				GameManager.GetConditionComponent().AddHealth(conditionChange, DamageSource.Starving);
			}

			UpdateFoodPoisoning();
			UpdateWellFedHungerScore(todHours);
		}

		internal float GetStoredCaloriesChangePerHour() {
			float hungerRatio = hunger.m_CurrentReserveCalories / hunger.m_MaxReserveCalories;
			return GetStoredCaloriesChangePerHour(hungerRatio, storedCalories, hunger.GetCurrentCalorieBurnPerHour());
		}

		private static float GetStoredCaloriesChangePerHour(float hungerRatio, double storedCalories, float calorieBurnRate) {
			if (hungerRatio > Tuning.hungerLevelWellFed) {
				// Add calories if possible
				return Tuning.GetCaloriesStoredPerHour(hungerRatio);
			} else if (hungerRatio < Tuning.hungerLevelMalnourished) {
				// Remove calories if possible
				return Tuning.GetCaloriesRemovedPerHour(hungerRatio, storedCalories, calorieBurnRate);
			} else {
				return 0f;
			}
		}

		private float ClampCalorieChange(float calorieChange) {
			return ClampCalorieChange(calorieChange, hunger.m_CurrentReserveCalories, hunger.m_MaxReserveCalories, storedCalories);
		}

		private static float ClampCalorieChange(float calorieChange, float hungerCalories, float maxHungerCalories, double storedCalories) {
			float result = calorieChange;
			// Clamp by stored calories
			result = Mathf.Clamp(result, (float) -storedCalories, (float) (Tuning.maximumStoredCalories - storedCalories));
			// Clamp by hunger calories
			result = Mathf.Clamp(result, hungerCalories - maxHungerCalories, hungerCalories);
			return result;
		}

		internal float GetConditionChangePerHour() {
			float hungerRatio = hunger.m_CurrentReserveCalories / hunger.m_MaxReserveCalories;
			if (hungerRatio < Tuning.hungerLevelStarving) {
				return Tuning.GetConditionChangePerHour(hungerRatio, storedCalories);
			} else {
				return 0f;
			}
		}

		internal float GetStoredCaloriesWarmthBonus() {
			float storedCalorieRatio = (float) (storedCalories / Tuning.maximumStoredCalories);
			return Tuning.GetStoredCaloriesWarmthBonus(storedCalorieRatio);
		}

		internal float GetCalorieBurnRateMultiplier() {
			float storedCalorieRatio = (float) (storedCalories / Tuning.maximumStoredCalories);
			return Tuning.GetCalorieBurnRateMultiplier(storedCalorieRatio);
		}

		internal HungerTuple SimulateHungerBar(float calorieBurnPerHour, float hours) {
			const int simulationStepsPerHour = 10;

			int steps = Mathf.CeilToInt(simulationStepsPerHour * hours);
			float todHours = hours / steps;
			float calorieBurn = calorieBurnPerHour * todHours;

			float maxHunger = this.hunger.m_MaxReserveCalories;
			float hunger = this.hunger.m_CurrentReserveCalories;
			float stored = (float) storedCalories;

			// Approximate solution using midpoint method
			// This seems to be good enough of an approximation for this problem
			for (int i = 0; i < steps; ++i) {
				float midpointDelta = todHours / 2 * GetStoredCaloriesChangePerHour(hunger / maxHunger, stored, calorieBurnPerHour);
				float midpointHunger = hunger - (midpointDelta + calorieBurn / 2);
				float midpointStored = stored + midpointDelta;

				// Ensure that 0 stored calories can always be reached
				midpointStored = Mathf.Clamp(midpointStored, 1f, (float) Tuning.maximumStoredCalories);

				float deltaStored = todHours * GetStoredCaloriesChangePerHour(midpointHunger / maxHunger, midpointStored, calorieBurnPerHour);
				deltaStored = ClampCalorieChange(deltaStored, hunger, maxHunger, stored);
				stored += deltaStored;
				hunger -= deltaStored + calorieBurn;
			}

			stored = Mathf.Clamp(stored, 0f, (float) Tuning.maximumStoredCalories);
			hunger = Mathf.Clamp(hunger, 0f, maxHunger);

			return new HungerTuple(stored, hunger / maxHunger);
		}

		internal void AddFoodPoisoningCall(string causeId) {
			float incubationPeriodHours = UnityEngine.Random.Range(Tuning.foodPoisoningDelayHoursMin, Tuning.foodPoisoningDelayHoursMax);
			float triggerTimeHours = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + incubationPeriodHours;

			DeferredFoodPoisoning foodPoisoning = new DeferredFoodPoisoning() { start = triggerTimeHours, cause = causeId };
			deferredFoodPoisonings.Add(foodPoisoning);
		}

		internal void OnPlayerTookAntibiotics() {
			if (GameManager.GetFoodPoisoningComponent().HasFoodPoisoning()) {
				deferredFoodPoisonings.Clear();
			} else {
				deferredFoodPoisonings.RemoveAll(foodPoisoning =>
						Utils.RollChance(Tuning.foodPoisoningPreventedByAntibioticsChance));
			}
		}

		private void UpdateFoodPoisoning() {
			float currentTimeHours = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();

			for (int i = deferredFoodPoisonings.Count - 1; i >= 0; --i) {
				DeferredFoodPoisoning foodPoisoning = deferredFoodPoisonings[i];

				if (currentTimeHours > foodPoisoning.start) {
					deferredFoodPoisonings.RemoveAt(i);
					GameStatePatches.IsApplyingDeferredFoodPoisoning = true;
					GameManager.GetFoodPoisoningComponent().FoodPoisoningStart(foodPoisoning.cause, true, false);
					GameStatePatches.IsApplyingDeferredFoodPoisoning = false;
				}
			}
		}

		private void UpdateWellFedHungerScore(float todHours) {
			double decay = Math.Pow(Tuning.wellFedHungerScoreDecayPerHour, todHours);
			float hungerRatio = hunger.m_CurrentReserveCalories / hunger.m_MaxReserveCalories;
			float hungerScoreChange = Tuning.GetWellFedHungerScoreChange(hungerRatio);

			// First change the hunger score ...
			wellFedHungerScore = decay * wellFedHungerScore + (1 - decay) * hungerScoreChange;
			// ... then clamp it to [-1, 1]
			wellFedHungerScore = Math.Min(Math.Max(wellFedHungerScore, -1), 1);
		}

		internal float GetCarryBonus() {
			float storedCalorieRatio = (float) (storedCalories / Tuning.maximumStoredCalories);
			float storedCalorieScore = 2 * storedCalorieRatio - 1;
			float carryBonus = Tuning.GetCarryBonus((float) wellFedHungerScore, storedCalorieScore);
			return (float) Math.Round(carryBonus, 1);
		}
	}
}
