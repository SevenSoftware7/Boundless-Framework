namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WaterMesh : MeshInstance3D, ISerializationListener {
	[Export(PropertyHint.ColorNoAlpha)] public Color WaterColor {
		get => _waterColor;
		set {
			_waterColor = value;

			if (Mesh is not Mesh mesh) return;
			if (mesh.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_color", _waterColor);
		}
	}
	private Color _waterColor = new(0f, 0f, 1f, 1f);

	[Export] public float WaterIntensity {
		get => _waterIntensity;
		set {
			_waterIntensity = value;

			if (Mesh is not Mesh mesh) return;
			if (mesh.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_intensity", _waterIntensity);
		}
	}
	private float _waterIntensity = 1f;

	[Export] public float WaterScale {
		get => _waterScale;
		set {
			_waterScale = value;

			if (Mesh is not Mesh mesh) return;
			if (mesh.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_scale", _waterScale);
		}
	}
	private float _waterScale = 70f;

	private Transform3D? lastTransform = null;


	public WaterMesh() : base() {
		Layers = VisualLayers.Water;
	}



	private void OnMeshChanged() {
		WaterMeshManager.UpdateMesh(this);
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (lastTransform is not null && GlobalTransform == lastTransform) return;

		lastTransform = GlobalTransform;
		WaterMeshManager.UpdateTransform(this);
	}

	public override void _EnterTree() {
		base._EnterTree();

		Mesh? mesh = Mesh;

		WaterMeshManager.Add(this);
		lastTransform = GlobalTransform;

		if (mesh is not null) {
			mesh.Changed += OnMeshChanged;
		}
	}
	public override void _ExitTree() {
		base._ExitTree();

		Mesh? mesh = Mesh;

		lastTransform = null;
		WaterMeshManager.Remove(this);

		if (mesh is not null) {
			mesh.Changed -= OnMeshChanged;
		}

	}

	public override bool _Set(StringName property, Variant value) {
		if (property != MeshInstance3D.PropertyName.Mesh || !IsInsideTree()) return base._Set(property, value);

		Mesh? mesh = Mesh;

		if (mesh is not null) {
			WaterMeshManager.Remove(this);
			mesh.Changed -= OnMeshChanged;
		}

		mesh = Mesh = value.As<Mesh>();

		if (mesh is not null) {
			WaterMeshManager.Add(this);
			mesh.Changed += OnMeshChanged;
		}

		return true;
	}

	public void OnBeforeSerialize() {
		if (Mesh is Mesh mesh) {
			mesh.Changed -= OnMeshChanged;
		}
	}

	public void OnAfterDeserialize() {
		if (Mesh is Mesh mesh) {
			mesh.Changed += OnMeshChanged;
		}
	}
}