namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class Companion : Node3D, IUIObject, ICustomizable, ICostumable, ISaveable<Companion>, IInjectionProvider<Skeleton3D?> {
	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => CostumeHolder?.Costume?.DisplayPortrait;

	[Export] public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;
			if (IsNodeReady()) {
				this.PropagateInject<Skeleton3D?>();
			}
		}
	}
	private Skeleton3D? _skeleton;

	[ExportGroup("Costume")]
	[Export] public CostumeHolder? CostumeHolder { get; set; }


	[Signal] public delegate void CostumeChangedEventHandler(CompanionCostume? newCostume, CompanionCostume? oldCostume);


	protected Companion() : base() { }
	public Companion(CompanionCostume? costume = null) {
		CostumeHolder = new CostumeHolder(costume).ParentTo(this);
	}


	public virtual List<ICustomizable> GetSubCustomizables() => [];
	public virtual List<ICustomization> GetCustomizations() => [];

	public Skeleton3D? GetInjection() => _skeleton;


	public virtual ISaveData<Companion> Save() => new CompanionSaveData<Companion>(this);

	public override void _Ready() {
		base._Ready();
		if (IsNodeReady()) {
			this.PropagateInject<Skeleton3D?>();
		}
	}


	[Serializable]
	public class CompanionSaveData<T>(T companion) : CostumableSaveData<Companion, CompanionCostume>(companion) where T : Companion {

	}
}