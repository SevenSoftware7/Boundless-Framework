namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class SkinnedModel : Model, IInjectable<Skeleton3D?>, IInjectable<Handedness> {
	[Export] protected GeometryInstance3D Model { get; private set; } = null!;

	[ExportGroup("Dependencies")]
	public Skeleton3D? Skeleton { get; private set; }
	public Handedness Handedness { get; private set; }


	protected SkinnedModel() : base() { }


	public override Aabb GetAabb() => Model.GetAabb();

	public void Inject(Handedness handedness) {
		Handedness = handedness;
	}
	public void Inject(Skeleton3D? skeleton) {
		Skeleton = skeleton;

		if (Model is null || Model is not MeshInstance3D meshInstance) return;

		if (Skeleton is null) {
			meshInstance.Skeleton = "..";
			return;
		}

		meshInstance.Skeleton = meshInstance.GetPathTo(Skeleton);
	}

	public void RequestInjection() {
		this.RequestInjection<Skeleton3D?>();
		this.RequestInjection<Handedness>();
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (Skeleton is null) return;

		GlobalTransform = Skeleton.GlobalTransform;
	}

	public override void _Ready() {
		base._Ready();

		if (GetParent()?.IsNodeReady() ?? false) {
			RequestInjection();
		}
	}
}