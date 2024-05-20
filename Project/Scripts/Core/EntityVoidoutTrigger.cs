namespace LandlessSkies.Core;

using Godot;


[GlobalClass]
public partial class EntityVoidoutTrigger : EntityTrigger {
	protected override void OnEntityEntered(Entity entity) {
		entity.VoidOut();
	}
}