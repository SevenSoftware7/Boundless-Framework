namespace LandlessSkies.Core;

using Godot;


[GlobalClass]
public partial class EntityKillTrigger : EntityTrigger {
	protected override void OnEntityEntered(Entity entity) {
		entity.Health?.Kill();
	}
}