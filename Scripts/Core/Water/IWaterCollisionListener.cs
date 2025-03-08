namespace LandlessSkies.Core;

public interface IWaterCollisionListener {
	public void OnEnterWater(Water water);
	public void OnExitWater(Water water);
}