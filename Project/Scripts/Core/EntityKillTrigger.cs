using Godot;

namespace LandlessSkies.Core;

[GlobalClass]
public partial class EntityKillTrigger : EntityTrigger {
	protected override void OnEntityEntered(Entity entity) {
		entity.Health?.Kill();
	}
}