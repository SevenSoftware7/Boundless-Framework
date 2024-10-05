namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;


public abstract partial class EntityTrigger : DetectorArea3D<Entity> {
	[Signal] public delegate void EntityEnteredEventHandler(Entity entity);


	public override void _Ready() {
		base._Ready();
		CollisionMask = CollisionLayers.Entity;
	}

	protected sealed override void OnTargetEntered(Entity target) {
		OnEntityEntered(target);
		_EntityEntered(target);
	}

	protected abstract void _EntityEntered(Entity entity);
}