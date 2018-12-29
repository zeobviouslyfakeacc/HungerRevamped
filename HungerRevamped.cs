using System;
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
		internal static bool HasInstance() => instance != null;

		internal double storedCalories;
		internal readonly Hunger hunger;

		internal HungerRevamped(Hunger hunger) {
			this.hunger = hunger;
			storedCalories = Tuning.startingStoredCalories;
		}

		internal void Update() {
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
	}
}
