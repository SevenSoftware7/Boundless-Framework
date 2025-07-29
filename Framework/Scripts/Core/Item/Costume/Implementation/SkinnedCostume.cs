namespace Seven.Boundless;

using System.Linq;
using Godot;
using Seven.Boundless.Injection;

[Tool]
[GlobalClass]
public partial class SkinnedCostume : MeshCostume {
	public IInjectionNode InjectionNode { get; }

	[ExportGroup("Dependencies")]
	[Injectable] public Handedness Handedness { get; private set; }
	public Skeleton3D? Skeleton { get; private set; }


	protected SkinnedCostume(GeometryInstance3D[] meshes) : base(meshes) {
		InjectionNode = new GodotNodeInjectionNode(this);
	}
	protected SkinnedCostume() : this([]) { }


	[Injectable]
	public void InjectSkeleton(Skeleton3D? skeleton) {
		Skeleton = skeleton;

		if (Skeleton is null) return;

		foreach (MeshInstance3D mesh in Meshes.OfType<MeshInstance3D>()) {
			NodePath path = mesh.GetPathTo(Skeleton);
			mesh.Skeleton = path;
		}
	}

	public void RequestInjection() {
		if (!this.RequestInjection<Skeleton3D>()) Skeleton = null;
		if (!this.RequestInjection<Handedness>()) Handedness = Handedness.Right;
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (Skeleton is null) return;

		GlobalTransform = Skeleton.GlobalTransform;
	}

	public override void _Ready() {
		base._Ready();

		if (Meshes.Count != 0) {
			RequestInjection();
		}
	}
}