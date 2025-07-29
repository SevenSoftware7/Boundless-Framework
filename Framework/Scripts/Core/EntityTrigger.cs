namespace Seven.Boundless;

using Godot;

public abstract partial class EntityTrigger : DetectorArea3D<Entity> {
	[Signal] public delegate void EntityEnteredEventHandler(Entity entity);


	public override void _Ready() {
		base._Ready();
		CollisionMask = CollisionLayers.Entity;
	}

	protected sealed override void OnTargetEntered(Entity target) {
		EmitSignalEntityEntered(target);
		_EntityEntered(target);
	}

	protected abstract void _EntityEntered(Entity entity);
}