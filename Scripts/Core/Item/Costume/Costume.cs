namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class Costume : Node3D, IPersistent<Costume>, IItem<Costume>, ICustomizable {
	IItemData<Costume>? IItem<Costume>.Data => Data.Value;

	[Export] public InterfaceResource<IItemData<Costume>> Data = new();
	public string DisplayName => Data.Value?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => Data.Value?.DisplayPortrait;

	public abstract Material? MaterialOverride { get; protected set; }
	public abstract Material? MaterialOverlay { get; protected set; }
	public abstract float Transparency { get; protected set; }

	protected Costume() : base() { }


	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];

	public virtual Aabb GetAabb() => default;

	public IPersistenceData<Costume> Save() => new ItemPersistenceData<Costume>(this);

}