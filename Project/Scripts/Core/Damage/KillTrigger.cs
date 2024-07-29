namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[GlobalClass]
public partial class KillTrigger : DetectorArea3D<IDamageable> {
	protected override void OnTargetEntered(IDamageable target) {
		target.Kill();
	}
}