namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public partial class SkeletonModel : Model {
	[Export] protected Skeleton3D Model { get; private set; } = null!;

	[ExportGroup("Dependencies")]
	[Export] public Handedness Handedness { get; private set; }




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
}