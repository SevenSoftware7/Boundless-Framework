namespace LandlessSkies.Core;

using Godot;

public interface IWaterDisplacementSubscriber {
	(Vector3 location, WaterMesh mesh)? GetInfo();

	void UpdateWaterDisplacement(Vector3 waterDisplacement);
}