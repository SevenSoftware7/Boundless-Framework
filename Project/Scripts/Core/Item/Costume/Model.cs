namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public partial class Model : Node3D, ICustomizable {
	public Costume? Costume;

	public string DisplayName => Costume?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;


	protected Model() : base() { }


	public virtual List<ICustomizable> GetSubCustomizables() => [];
	public virtual List<ICustomization> GetCustomizations() => [];

	public virtual Aabb GetAabb() => default;
}