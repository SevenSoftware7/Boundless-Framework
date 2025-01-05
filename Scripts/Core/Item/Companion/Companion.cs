namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class Companion : Node3D, ICustomizable, ICostumable, IPersistent<Companion>, IItem<Companion>, IInjectionBlocker<Skeleton3D> {
	public IInjectionNode InjectionNode { get; }

	IItemKeyProvider<Companion> IItem<Companion>.KeyProvider => KeyProvider;
	[Export] private CompanionResourceItemKey KeyProvider = new();

	[Export] public ItemUIData? UI { get; private set; }
	public string DisplayName => UI?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UI?.DisplayPortrait;


	[Export]
	[Injector]
	public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;
			this.PropagateInjection<Skeleton3D>();
		}
	}
	private Skeleton3D? _skeleton;

	[ExportGroup("Costume")]
	public CostumeHolder CostumeHolder => _costumeHolder ??= new CostumeHolder().SafeReparentAndSetOwner(this).SafeRename(nameof(CostumeHolder));
	[Export] private CostumeHolder? _costumeHolder;


	public Companion() : base() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}


	public override void _Ready() {
		base._Ready();
		this.PropagateInjection<Skeleton3D>();
	}


	public virtual IEnumerable<IUIObject> GetSubObjects() => [CostumeHolder];
	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];


	public virtual IPersistenceData<Companion> Save() => new CompanionSaveData<Companion>(this);

	[Serializable]
	public class CompanionSaveData<T>(T companion) : ItemPersistenceData<Companion>(companion) where T : Companion;
}