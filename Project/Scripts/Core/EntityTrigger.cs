namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


public abstract partial class EntityTrigger : DetectorArea3D<Entity> {
	[Signal] public delegate void EntityEnteredEventHandler(Entity entity);


	protected sealed override void OnTargetEntered(Entity target) {
		EmitSignal(SignalName.EntityEntered, target);
		OnEntityEntered(target);
	}

	protected abstract void OnEntityEntered(Entity entity);
}