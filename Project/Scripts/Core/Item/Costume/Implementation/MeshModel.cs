namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class MeshModel : Model {
	[Export] protected Godot.Collections.Array<GeometryInstance3D> Meshes = [];


	public override Material? MaterialOverride {
		get => _materialOverride;
		protected set {
			foreach (GeometryInstance3D model in Meshes) {
				Material? materialOverride = model.MaterialOverride;

				if (materialOverride == _materialOverride || materialOverride is null || _materialOverride is null) {
					model.MaterialOverride = value;
				}
			}

			_materialOverride = value;
		}
	}
	private Material? _materialOverride;

	public override Material? MaterialOverlay {
		get => _materialOverlay;
		protected set {
			foreach (GeometryInstance3D model in Meshes) {
				Material? materialOverlay = model.MaterialOverlay;

				if (materialOverlay == _materialOverlay || materialOverlay is null || _materialOverlay is null) {
					model.MaterialOverlay = value;
				}
			}

			_materialOverlay = value;
		}
	}
	private Material? _materialOverlay;

	public override float Transparency {
		get => _transparency;
		protected set {
			foreach (GeometryInstance3D model in Meshes) {
				float transparency = model.Transparency;

				if (transparency == _transparency) {
					model.Transparency = value;
				}
			}

			_transparency = value;
		}
	}
	private float _transparency;


	protected MeshModel(GeometryInstance3D[] meshes) : base() {
		Meshes = [.. meshes];
	}
	protected MeshModel() : base() { }


	public override Aabb GetAabb() {
		Aabb bounds = new();

		foreach (GeometryInstance3D mesh in Meshes) {
			bounds = bounds.Merge(mesh.GetAabb());
		}

		return bounds;
	}
}