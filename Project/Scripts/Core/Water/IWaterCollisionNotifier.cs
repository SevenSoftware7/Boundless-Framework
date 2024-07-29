namespace LandlessSkies.Core;

public interface IWaterCollisionNotifier {
	void Enter(Water water);
	void Exit(Water water);
}