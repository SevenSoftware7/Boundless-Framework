namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class Model : Node3D, ICustomizable {
	public Costume? Costume;

	public string DisplayName => Costume?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;

	public abstract Material? MaterialOverride { get; protected set; }
	public abstract Material? MaterialOverlay { get; protected set; }
	public abstract float Transparency { get; protected set; }


	protected Model() : base() { }


	public virtual List<ICustomizable> GetSubCustomizables() => [];
	public virtual List<ICustomization> GetCustomizations() => [];

	public virtual Aabb GetAabb() => default;
}