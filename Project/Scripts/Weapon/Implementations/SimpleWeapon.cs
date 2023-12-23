using System;
using System.Collections.Generic;
using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class SimpleWeapon : Weapon {

	private Entity? _entity;
	private WeaponData _data = null!;
	private IWeapon.Handedness _weaponHandedness = IWeapon.Handedness.Right;
	
	

	public override WeaponData Data {
		get => _data;
		protected set => _data ??= value;
	}

	public override WeaponCostume? Costume {
		get => WeaponModel?.Costume;
		set => SetCostume(value);
	}

	public override IWeapon.Handedness WeaponHandedness {
		get => _weaponHandedness;
		set {
			_weaponHandedness = value;
			WeaponModel?.Inject(value);
		}
	}


	[ExportGroup("Costume")]
	[Export] private WeaponModel? WeaponModel;


	[ExportGroup("Dependencies")]
	[Export]
	public Entity? Entity {
		get => _entity;
		private set => Inject(value);
	}



	protected SimpleWeapon() : base() {
		Name = GetType().Name;
		InitializeAttacks();

		// Reconnect events on build
		if ( this.JustBuilt() ) {
			Callable.From( ConnectEvents ).CallDeferred();
		}
	}
	public SimpleWeapon(WeaponData data, WeaponCostume? costume, Node3D root) : base(data, costume, root) {
		Name = GetType().Name;
		InitializeAttacks();
	}



	protected virtual void InitializeAttacks() {}


	public override void SetCostume(WeaponCostume? costume) {
		if ( this.IsEditorGetSetter() ) return;

		WeaponCostume? oldCostume = Costume;
		if ( costume == oldCostume ) return;

		LoadableExtensions.UpdateLoadable(ref WeaponModel)
			.WithConstructor(() => costume?.Instantiate(this))
			.BeforeLoad(() => {
				WeaponModel!.Inject(Entity?.Armature);
				WeaponModel!.Inject(WeaponHandedness);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}

	public override void Inject(Entity? entity) {
		DisconnectEvents();
		_entity = entity;
		if ( this.IsEditorGetSetter() ) {
			return;
		}
		ConnectEvents();

		WeaponModel?.Inject(entity?.Armature);
		ReloadModel();
	}


	private void OnCharacterLoadedUnloaded(bool isLoaded) {
		WeaponModel?.Inject(Entity?.Armature);
	}


	private void ConnectEvents() {
		if (_entity is not null) {
			_entity.CharacterLoadedUnloaded += OnCharacterLoadedUnloaded;
		}
	}
	private void DisconnectEvents() {
		if (_entity is not null) {
			_entity.CharacterLoadedUnloaded -= OnCharacterLoadedUnloaded;
		}
	}


	public override IEnumerable<AttackAction.Info> GetAttacks(Entity target) {
		return [];
	}


	protected override bool LoadModelImmediate() {
		WeaponModel?.LoadModel();

		return true;
	}

	protected override bool UnloadModelImmediate() {
		WeaponModel?.UnloadModel();

		return true;
	}

	public override void _Ready() {
		base._Ready();

		ConnectEvents();
	}

	public override void _ExitTree() {
		base._ExitTree();

		DisconnectEvents();

		WeaponModel?.Inject(null);
	}
}
