namespace HungerRevamped {

	// Since we can't have tuples in .NET 3.5
	internal struct HungerTuple {
		internal float storedCalories;
		internal float hungerRatio;

		internal HungerTuple(float stored, float hunger) {
			storedCalories = stored;
			hungerRatio = hunger;
		}
	}
}
