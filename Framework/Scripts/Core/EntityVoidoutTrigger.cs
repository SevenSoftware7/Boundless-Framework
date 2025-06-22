namespace SevenDev.Boundless;

using Godot;

[GlobalClass]
public partial class EntityVoidoutTrigger : EntityTrigger {
	protected override void _EntityEntered(Entity entity) {
		entity.VoidOut();
	}
}