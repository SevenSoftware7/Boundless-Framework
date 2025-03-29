namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public partial class KillTrigger : DetectorArea3D<IDamageable> {
	protected override void OnTargetEntered(IDamageable target) {
		target.Kill();
	}
}