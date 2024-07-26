using Godot;

namespace LandlessSkies.Core;

public interface IWaterDisplacementSubscriber {
	Vector3 GetLocation();

	void UpdateWaterDisplacement(Vector3 waterDisplacement);
}