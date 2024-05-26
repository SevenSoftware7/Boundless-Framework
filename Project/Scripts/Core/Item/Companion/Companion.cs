namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class Companion : Node3D, IUIObject, ICostumable<CompanionCostume>, ICustomizable, ISaveable<Companion>, IInjector<Skeleton3D?> {
	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;


	[Export] public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;
			this.PropagateInject(_skeleton);
		}
	}
	private Skeleton3D? _skeleton;
	public Skeleton3D? Inject() => Skeleton;


	public virtual IUIObject UIObject => this;
	public virtual ICustomizable[] Customizables => [];
	public virtual ICustomization[] Customizations => [];


	[ExportGroup("Costume")]
	[Export] public CompanionCostume? Costume {
		get => _costume;
		set => SetCostume(value);
	}
	private CompanionCostume? _costume;

	public Model? Model { get; private set; }
	public bool IsLoaded => Model is not null;


	[Signal] public delegate void CostumeChangedEventHandler(CompanionCostume? newCostume, CompanionCostume? oldCostume);



	protected Companion() : base() { }
	public Companion(CompanionCostume? costume) : this() {
		SetCostume(costume);
	}



	public void SetCostume(CompanionCostume? newCostume) {
		CompanionCostume? oldCostume = _costume;
		if (newCostume == oldCostume) return;

		_costume = newCostume;
		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);

		Callable.From<bool>(Load).CallDeferred(true);
	}


	public void Load(bool forceReload = false) {
		if (IsLoaded && ! forceReload) return;

		Unload();

		Model = Costume?.Instantiate()?.ParentTo(this);

		if (Model is null) return;

		Model?.PropagateInject(Skeleton);
	}
	public void Unload() {
		Model?.QueueFree();
		Model = null;
	}


	public virtual ISaveData<Companion> Save() => new CompanionSaveData<Companion>(this);


	[Serializable]
	public class CompanionSaveData<T>(T companion) : CostumableSaveData<Companion, T, CompanionCostume>(companion) where T : Companion {

		// public override Companion? Load() {
		// 	if (base.Load() is not Companion weapon) return null;

		// 	return base.Load();
		// }
	}
}