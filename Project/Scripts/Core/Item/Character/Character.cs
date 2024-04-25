namespace LandlessSkies.Core;

using Godot;
using SevenGame.Utility;
using System;

[Tool]
[GlobalClass]
public partial class Character : Loadable3D, IInputReader, ICustomizable {
	[Export] public Node3D? Collisions { get; private set; }
	[Export] public Skeleton3D? Skeleton { get; private set; }

	public override bool IsLoaded {
		get => _isLoaded;
		set => this.BackingFieldLoadUnload(ref _isLoaded, value);
	}
	private bool _isLoaded;


	[Export] public CharacterData Data {
		get => _data;
		private set {
			_data = value;

			if (this.IsInitializationSetterCall())
				return;
			if (Costume is not null)
				return;

			SetCostume(_data?.BaseCostume);
		}
	}
	private CharacterData _data = null!;

	[ExportGroup("Costume")]
	[Export] private Model? CharacterModel;
	[Export] public CharacterCostume? Costume {
		get => CharacterModel?.Costume as CharacterCostume;
		set {
			if (this.IsInitializationSetterCall())
				return;
			SetCostume(value);
		}
	}

	public Basis CharacterRotation { get; private set; } = Basis.Identity;


	public virtual IUIObject UIObject => Data;
	public virtual ICustomizable[] Children => CharacterModel is Model model ? [model] : [];
	public virtual ICustomizationParameter[] Customizations => [];


	[Signal] public delegate void CostumeChangedEventHandler(CharacterCostume? newCostume, CharacterCostume? oldCostume);



	public Character() : base() { }
	public Character(CharacterData data, CharacterCostume? costume) : this() {
		ArgumentNullException.ThrowIfNull(data);

		_data = data;
		SetCostume(costume ?? data.BaseCostume);
		Name = $"{nameof(Character)} - {Data.DisplayName}";
	}



	public void SetCostume(CharacterCostume? costume, bool forceLoad = false) {
		CharacterCostume? oldCostume = Costume;
		if (costume == oldCostume)
			return;

		new LoadableUpdater<Model>(ref CharacterModel, () => costume?.Instantiate())
			.BeforeLoad(m => {
				// if (m is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
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

		if (Collisions is null)
			return;

		Collisions.Transform = Collisions.Transform with {
			Basis = CharacterRotation,
		};
	}

	public virtual void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice) { }


	protected override bool LoadBehaviour() {
		if (!base.LoadBehaviour())
			return false;
		if (Data is null)
			return false;

		Collisions = Data.CollisionScene?.Instantiate() as Node3D;
		if (Collisions is not null) {
			Collisions.Name = nameof(Collisions);
			Collisions.SetOwnerAndParent(GetParent());
		}

		Skeleton = Data.SkeletonScene?.Instantiate() as Skeleton3D;
		if (Skeleton is not null) {
			Skeleton.Name = nameof(Skeleton);
			Skeleton.SetOwnerAndParent(this);
		}

		// CharacterModel.Inject(Skeleton);
		CharacterModel?.AsILoadable().Load();

		RefreshRotation();

		_isLoaded = true;

		return true;
	}
	protected override void UnloadBehaviour() {
		base.UnloadBehaviour();

		// CharacterModel?.Inject(null);
		CharacterModel?.AsILoadable().Unload();

		Skeleton?.UnparentAndQueueFree();
		Skeleton = null;

		Collisions?.UnparentAndQueueFree();
		Collisions = null;


		_isLoaded = false;
	}

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		CharacterModel?.AsIEnablable().Enable();
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		CharacterModel?.AsIEnablable().Disable();
	}
}