namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class Costume : Node3D, IPersistent<Costume>, IItem<Costume>, ICustomizable {
	[Export] public DataKey Key { get; private set; } = new();
	[Export] public ItemUIData? UI { get; private set; }
	public string DisplayName => UI?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UI?.DisplayPortrait;

	public abstract Material? MaterialOverride { get; protected set; }
	public abstract Material? MaterialOverlay { get; protected set; }
	public abstract float Transparency { get; protected set; }

	protected Costume() : base() { }


	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];

	public virtual Aabb GetAabb() => default;

	public IPersistenceData<Costume> Save() => new ItemPersistenceData<Costume>(this);

}