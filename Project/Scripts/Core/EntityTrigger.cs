namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


public abstract partial class EntityTrigger : DetectorArea3D<Entity> {
	[Signal] public delegate void EntityEnteredEventHandler(Entity entity);


	public override void _Ready() {
		base._Ready();
		CollisionMask = CollisionLayers.Entity;
	}

	protected sealed override void OnTargetEntered(Entity target) {
		EmitSignal(SignalName.EntityEntered, target);
		OnEntityEntered(target);
	}

	protected abstract void OnEntityEntered(Entity entity);
}