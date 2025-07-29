namespace Seven.Boundless;

using Godot;

public interface IWaterDisplacementSubscriber {
	public (Vector3 location, WaterMesh mesh)? GetInfo();

	public void UpdateWaterDisplacement(Vector3 waterDisplacement);
}