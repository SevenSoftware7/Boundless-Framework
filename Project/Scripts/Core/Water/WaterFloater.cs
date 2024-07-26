using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class WaterFloater : Node3D, IWaterDisplacementSubscriber, ISerializationListener {
	private float waterSurfaceDisplacement = 0f;

	public Vector3 GetLocation() => GlobalPosition;
	public void UpdateWaterDisplacement(Vector3 waterDisplacement) => waterSurfaceDisplacement = waterDisplacement.Y;



	public override void _EnterTree() {
		base._EnterTree();
		WaterDisplacementEffect.Subscribers.Add(this);
	}

	public override void _ExitTree() {
		base._ExitTree();
		WaterDisplacementEffect.Subscribers.Remove(this);
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what){
			case NotificationPredelete:
				WaterDisplacementEffect.Subscribers.Remove(this);
				break;
		}
	}

	public void OnBeforeSerialize() {
		WaterDisplacementEffect.Subscribers.Remove(this);
	}

	public void OnAfterDeserialize() {
		WaterDisplacementEffect.Subscribers.Add(this);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		GlobalPosition = GlobalPosition with {
			Y = waterSurfaceDisplacement - 1f
		};
	}
}