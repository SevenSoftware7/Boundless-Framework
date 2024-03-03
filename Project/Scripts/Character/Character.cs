using Godot;
using Godot.Collections;
using SevenGame.Utility;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Character : Loadable3D, IDataContainer<CharacterData>, ICostumable<CharacterCostume>, IInputReader, ICustomizable {

	public readonly static string CollisionsName = "Collisions";
	public readonly static string SkeletonName = "Skeleton";
	public readonly static string ModelName = "Model";

	[Export] public Node3D? Collisions { get; private set; }
	[Export] public Skeleton3D? Skeleton { get; private set; }

	private bool _isLoaded = false;
    public override bool IsLoaded {
		get => _isLoaded;
		set {
			if ( this.IsInitializationSetterCall() ) {
				_isLoaded = value;
				return;
			}

			AsILoadable().LoadUnload(value);
		}
	}

	[Export] public CharacterData Data {
		get => _data;
		private set {
			if (_data is not null) return;

			_data = value;

			if ( this.IsInitializationSetterCall() ) return;
			if (Costume is not null) return;

			SetCostume(_data.BaseCostume);
		}
	}
	private CharacterData _data = null!;

	[ExportGroup("Costume")]
	[Export] private CharacterModel? CharacterModel;
	[Export] public CharacterCostume? Costume {
		get => CharacterModel?.Costume;
		set {
			if ( this.IsInitializationSetterCall() ) return;
			SetCostume(value);
		}
	}

	public Basis CharacterRotation { get; private set; } = Basis.Identity;


	public virtual IUIObject UIObject => Data;
	public virtual ICustomizable[] Children => CharacterModel is CharacterModel model ? [model] : [];
	public virtual ICustomizationParameter[] Customizations => [];


	[Signal] public delegate void CostumeChangedEventHandler(CharacterCostume? newCostume, CharacterCostume? oldCostume);



	public Character() : base() {}
	public Character(CharacterData data, CharacterCostume? costume) : this() {
		ArgumentNullException.ThrowIfNull(data);

		_data = data;
		SetCostume(costume ?? data.BaseCostume);
		Name = $"{nameof(Character)} - {Data.DisplayName}";
	}



	public void SetCostume(CharacterCostume? costume, bool forceLoad = false) {
		CharacterCostume? oldCostume = Costume;
		if ( costume == oldCostume ) return;

		new LoadableUpdater<CharacterModel>(ref CharacterModel, () => costume?.Instantiate())
			.BeforeLoad(m => {
				m.Inject(Skeleton);
				m.SafeReparentEditor(this);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}


	public void RotateTowards(Basis target, double delta, float speed = 16f) {
		CharacterRotation = CharacterRotation.SafeSlerp(target, (float)delta * speed);

		RefreshRotation();
	}

	protected virtual void RefreshRotation() {
		Transform = Transform with {
			Basis = CharacterRotation,
		};

		if ( Collisions is null ) return;

		Collisions.Transform = Collisions.Transform with {
			Basis = CharacterRotation,
		};
	}

	public virtual void HandleInput(Player.InputInfo inputInfo) {}


	protected override bool LoadModelBehaviour() {
		if ( ! base.LoadModelBehaviour() ) return false;
		if ( Data is null ) return false;

		Collisions = Data.CollisionScene?.Instantiate() as Node3D;
		if ( Collisions is not null ) {
			Collisions.Name = CollisionsName;
			Collisions.SetOwnerAndParent(GetParent());
		}

		Skeleton = Data.SkeletonScene?.Instantiate() as Skeleton3D;
		if ( Skeleton is not null ) {
			Skeleton.Name = SkeletonName;
			Skeleton.SetOwnerAndParent(this);
		}

		if ( CharacterModel is not null ) {
			CharacterModel.Inject(Skeleton);
			CharacterModel.AsILoadable().LoadModel();
		}

		RefreshRotation();

		_isLoaded = true;

		return true;
	}
	protected override void UnloadModelBehaviour() {
		base.UnloadModelBehaviour();

		Collisions?.UnparentAndQueueFree();
		Collisions = null;

		CharacterModel?.Inject(null);
		Skeleton?.UnparentAndQueueFree();
		Skeleton = null;

		CharacterModel?.AsILoadable().UnloadModel();

		_isLoaded = false;
	}

    public override void Enable() {
        base.Enable();
		CharacterModel?.Enable();
    }
    public override void Disable() {
        base.Disable();
		CharacterModel?.Disable();
    }
}