namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

public abstract partial class Interactable : Area3D {
	public abstract string InteractLabel { get; }
	public virtual float? MinLookIncidence => null;


	public Interactable() : base() {
		CollisionLayer = CollisionLayers.Interactable;
		CollisionMask = 0;
		Monitorable = false;
		Monitoring = false;
	}


	public CollisionShape3D? GetShape3D(int shapeIndex) {
		return ShapeOwnerGetOwner(ShapeFindOwner(shapeIndex)) as CollisionShape3D;
	}


	public abstract bool IsInteractable(Entity entity);
	public abstract void Interact(Entity entity, Player? player = null, int shapeIndex = 0);
}
