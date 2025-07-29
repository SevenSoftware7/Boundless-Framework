namespace Seven.Boundless;

using System.Collections.Generic;
using Godot;
using Seven.Boundless.Persistence;

[Tool]
[GlobalClass]
public abstract partial class Costume : Node3D, IPersistent<Costume>, IItem<Costume>, ICustomizable {
	IItemData<Costume>? IItem<Costume>.Data => Data;
	[Export] public SceneCostumeData? Data { get; private set; }
	public string DisplayName => Data?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => Data?.DisplayPortrait;

	public abstract Material? MaterialOverride { get; protected set; }
	public abstract Material? MaterialOverlay { get; protected set; }
	public abstract float Transparency { get; protected set; }

	protected Costume() : base() { }


	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];

	public virtual Aabb GetAabb() => default;

	public IPersistenceData<Costume> Save() => new CustomizableItemPersistenceData<Costume>(this);

}