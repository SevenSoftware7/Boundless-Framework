namespace LandlessSkies.Core;

using Godot;

public interface IWaterDisplacementSubscriber {
	Vector3 GetLocation();

	void UpdateWaterDisplacement(Vector3 waterDisplacement);
}