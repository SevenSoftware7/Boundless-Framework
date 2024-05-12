using Godot;

namespace LandlessSkies.Core;

[GlobalClass]
public partial class EntityVoidoutTrigger : EntityTrigger {
	protected override void OnEntityEntered(Entity entity) {
		entity.VoidOut();
	}
}