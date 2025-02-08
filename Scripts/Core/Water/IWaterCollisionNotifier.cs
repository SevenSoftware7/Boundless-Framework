namespace LandlessSkies.Core;

public interface IWaterCollisionNotifier {
	public void OnEnterWater(Water water);
	public void OnExitWater(Water water);
}