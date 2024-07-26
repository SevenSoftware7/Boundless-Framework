namespace LandlessSkies.Core;

public interface IWaterCollisionNotifier {
	void Enter(WaterArea water);
	void Exit(WaterArea water);
}