namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WaterMesh : MeshInstance3D, ISerializationListener {
	[Export(PropertyHint.ColorNoAlpha)] public Color ShallowColor {
		get => _shallowColor;
		set {
			_shallowColor = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("shallow_color", _shallowColor);
		}
	}
	private Color _shallowColor = new(0f, 0f, 1f, 1f);

	[Export(PropertyHint.ColorNoAlpha)] public Color DeepColor {
		get => _deepColor;
		set {
			_deepColor = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("deep_color", _deepColor);
		}
	}
	private Color _deepColor = new(0f, 0f, 1f, 1f);


	[Export] public float WaterIntensity {
		get => _waterIntensity;
		set {
			_waterIntensity = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_intensity", _waterIntensity);
		}
	}
	private float _waterIntensity = 1f;

	[Export] public float WaterScale {
		get => _waterScale;
		set {
			_waterScale = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_scale", _waterScale);
		}
	}
	private float _waterScale = 70f;

	[Export] public float FogDistance {
		get => _fogDistance;
		set {
			_fogDistance = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_fog_distance", _fogDistance);
		}
	}
	private float _fogDistance = 90f;

	[Export(PropertyHint.Range, "0,20,0.01")] public float FogFade {
		get => _fogFade;
		set {
			_fogFade = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_fog_fade", _fogFade);
		}
	}
	private float _fogFade = 1f;


	[Export] public float TransparencyDistance {
		get => _transparencyDistance;
		set {
			_transparencyDistance = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_transparency_distance", _transparencyDistance);
		}
	}
	private float _transparencyDistance = 45f;

	[Export(PropertyHint.Range, "0,20,0.01")] public float TransparencyFade {
		get => _transparencyFade;
		set {
			_transparencyFade = value;

			if (Mesh?.SurfaceGetMaterial(0) is not ShaderMaterial shaderMaterial) return;
			shaderMaterial.SetShaderParameter("water_transparency_fade", _transparencyFade);
		}
	}
	private float _transparencyFade = 1f;


	public WaterMesh() : base() {
		Layers = VisualLayers.Water;
	}



	private void OnMeshChanged() {
		WaterMeshManager.Add(this);
	}

	public override void _EnterTree() {
		base._EnterTree();

		Mesh? mesh = Mesh;

		WaterMeshManager.Add(this);

		if (mesh is not null) {
			mesh.Changed += OnMeshChanged;
		}
	}
	public override void _ExitTree() {
		base._ExitTree();

		Mesh? mesh = Mesh;

		WaterMeshManager.Remove(this);

		if (mesh is not null) {
			mesh.Changed -= OnMeshChanged;
		}
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch((ulong)what) {
			case NotificationVisibilityChanged:
				WaterMeshManager.SetEnabled(this, IsVisibleInTree());
				break;
		}
	}

	public override bool _Set(StringName property, Variant value) {
		if (property != MeshInstance3D.PropertyName.Mesh || !IsInsideTree()) return base._Set(property, value);

		Mesh? mesh = Mesh;

		if (mesh is not null) {
			mesh.Changed -= OnMeshChanged;
		}

		mesh = Mesh = value.As<Mesh>();
		WaterMeshManager.Add(this);

		if (mesh is not null) {
			mesh.Changed += OnMeshChanged;
		}

		return true;
	}

	public void OnBeforeSerialize() {
		WaterMeshManager.Remove(this);
	}

	public void OnAfterDeserialize() {
		if (!IsInsideTree()) return;

		if (Mesh is Mesh mesh) {
			mesh.Changed += OnMeshChanged;
		}
		WaterMeshManager.Add(this);
	}
}