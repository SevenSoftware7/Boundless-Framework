namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public partial class SkeletonModel : Model {

	[ExportGroup("Dependencies")]
	[Export] public Handedness Handedness { get; private set; }
	[ExportGroup("")]

	[Export] protected Skeleton3D Model { get; private set; } = null!;



	protected SkeletonModel() : base() { }



	public override Aabb GetAabb() => _aabb;
	[Export] private Aabb _aabb;

	public void SetHandedness(Handedness handedness) {
		Handedness = handedness;
	}

	private void UpdateAabb() {
		_aabb = new();

		this.PropagateAction<GeometryInstance3D>(geom => _aabb = _aabb.Merge(geom.GetAabb()));
	}

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		if (Model is not null) {
			Model.ProcessMode = ProcessModeEnum.Inherit;
			Model.Visible = true;
		}
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		if (Model is not null) {
			Model.ProcessMode = ProcessModeEnum.Disabled;
			Model.Visible = false;
		}
	}
}