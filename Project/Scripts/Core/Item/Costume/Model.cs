namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class Model : Node3D, ICustomizable {
	public Costume? Costume;

	public string DisplayName => Costume?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;

	public virtual ICustomizable[] Customizables => [];
	public virtual ICustomization[] Customizations => [];


	protected Model() : base() { }


	public virtual Aabb GetAabb() => default;
}