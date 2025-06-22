namespace SevenDev.Boundless;

public interface IWaterCollisionListener {
	public void OnEnterWater(Water water);
	public void OnExitWater(Water water);
}