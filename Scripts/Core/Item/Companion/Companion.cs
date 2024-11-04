namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;

[Tool]
[GlobalClass]
public partial class Companion : Node3D, ICustomizable, ICostumable, IPersistent<Companion>, IItem<Companion>, IInjectionBlocker<Skeleton3D> {
	[Export] public CompanionResourceDataKey ResourceKeyProvider { get; private set; } = new();
	IDataKeyProvider<Companion> IItem<Companion>.KeyProvider => ResourceKeyProvider;

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
	[Export] public CostumeHolder? CostumeHolder { get; set; }


	protected Companion() : base() { }
	public Companion(IItemData<Costume>? costume = null) {
		if (CostumeHolder is null) {
			CostumeHolder = new CostumeHolder(costume).ParentTo(this);
		}
		else {
			CostumeHolder.SetCostume(costume);
		}
	}


	public override void _Ready() {
		base._Ready();
		this.PropagateInjection<Skeleton3D>();
	}


	public virtual List<ICustomizable> GetSubCustomizables() => [];
	public virtual List<ICustomization> GetCustomizations() => [];


	public virtual IPersistenceData<Companion> Save() => new CompanionSaveData<Companion>(this);

	[Serializable]
	public class CompanionSaveData<T>(T companion) : ItemPersistenceData<Companion>(companion) where T : Companion;
}