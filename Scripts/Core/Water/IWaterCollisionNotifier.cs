namespace LandlessSkies.Core;

public interface IWaterCollisionNotifier {
	void OnEnterWater(Water water);
	void OnExitWater(Water water);
}