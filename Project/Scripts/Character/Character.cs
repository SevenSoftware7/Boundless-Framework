using Godot;
using SevenGame.Utility;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Character : Loadable3D, IDataContainer<CharacterData>, ICostumable<CharacterCostume>, IInputReader, ICustomizable {
	private CharacterData _data = null!;
	
	

	public Basis CharacterRotation { get; private set; } = Basis.Identity;


	[Export] public Node3D? Collisions { get; private set; }
	[Export] public Skeleton3D? Armature { get; private set; }

	[Export] public CharacterData Data {
		get => _data;
		private set {
			if (_data is not null) return;
			_data = value;

			if (Costume is not null) return;
			SetCostume(_data.BaseCostume);
		}
	}

	[ExportGroup("Costume")]
	[Export] private CharacterModel? CharacterModel;

	[Export] public CharacterCostume? Costume {
		get => CharacterModel?.Costume;
		set => SetCostume(value);
	}


	public virtual IUIObject UIObject => Data;
	public virtual ICustomizable[] Children => [CharacterModel!];
	public virtual ICustomizationParameter[] Customizations => [];


	[Signal] public delegate void CostumeChangedEventHandler(CharacterCostume? newCostume, CharacterCostume? oldCostume);



	public Character() : base() {}
	public Character(CharacterData data, CharacterCostume? costume, Node3D root) : base(root) {
		ArgumentNullException.ThrowIfNull(data);

		_data = data;
		SetCostume(costume ?? data.BaseCostume);
	}



	public void SetCostume(CharacterCostume? costume, bool forceLoad = false) {
		if ( this.IsEditorGetSetter() ) return;

		CharacterCostume? oldCostume = Costume;
		if ( costume == oldCostume ) return;

		LoadableExtensions.UpdateLoadable(ref CharacterModel)
			.WithConstructor(() => costume?.Instantiate(this))
			.BeforeLoad(() => CharacterModel!.Inject(Armature))
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}


	protected override bool LoadModelImmediate() {
		Node? parent = GetParent();
		if ( parent is null || Owner is null) return false;
		if ( Data is null ) return false;

		Collisions = Data.CollisionScene?.Instantiate() as Node3D;
		if ( Collisions is not null ) {
			Collisions.Name = nameof(Collisions);
			Collisions.SetOwnerAndParentTo(parent);
		}

		Armature = Data.SkeletonScene?.Instantiate() as Skeleton3D;
		if ( Armature is not null ) {
			Armature.Name = nameof(Armature);
			Armature.SetOwnerAndParentTo(this);
		}

		CharacterModel?.Inject(Armature);
		CharacterModel?.LoadModel();

		RefreshRotation();

		return true;
	}
	protected override bool UnloadModelImmediate() {
		Collisions?.UnparentAndQueueFree();
		Collisions = null;

		Armature?.UnparentAndQueueFree();
		Armature = null;

		CharacterModel?.Inject(null);
		CharacterModel?.UnloadModel();

		return true;
	}


	public void RotateTowards(Basis target, double delta, float speed = 12f) {
		CharacterRotation = CharacterRotation.SafeSlerp(target, (float)delta * speed);

		RefreshRotation();
	}

	protected virtual void RefreshRotation() {
		Transform = new(CharacterRotation, Transform.Origin);

		if ( Collisions is null ) return;

		Collisions.Transform = new(CharacterRotation, Collisions.Transform.Origin);
	}

	public virtual void HandleInput(Player.InputInfo inputInfo) {}
}