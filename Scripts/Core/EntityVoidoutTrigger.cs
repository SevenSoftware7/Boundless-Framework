namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public partial class EntityVoidoutTrigger : EntityTrigger {
	protected override void _EntityEntered(Entity entity) {
		entity.VoidOut();
	}
}