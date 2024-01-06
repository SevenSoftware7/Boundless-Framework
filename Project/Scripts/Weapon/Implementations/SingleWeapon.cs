using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class SingleWeapon : Weapon {
	private Entity? _entity;
	private WeaponData _data = null!;
	private IWeapon.Handedness _weaponHandedness = IWeapon.Handedness.Right;
	
	

	
	[Export] public override WeaponData Data {
		get => _data;
		protected set {
			if (_data is not null) return;
			_data = value;

			if (Costume is not null) return;
			SetCostume(_data?.BaseCostume);
		}
	}

	[ExportGroup("Costume")]
	[Export] public override WeaponCostume? Costume {
		get => WeaponModel?.Costume;
		set => SetCostume(value);
	}
	[Export] private WeaponModel? WeaponModel;


	[ExportGroup("Dependencies")]
	[Export] public Entity? Entity {
		get => _entity;
		private set => Inject(value);
	}


	public override IWeapon.Handedness WeaponHandedness {
		get => _weaponHandedness;
		set {
			_weaponHandedness = value;
			WeaponModel?.Inject(value);
		}
	}

	public override ICustomizable[] Children => [.. base.Children.Append(WeaponModel!)];



	protected SingleWeapon() : base() {
		InitializeAttacks();

		// Reconnect events on build
		if ( this.JustBuilt() ) Callable.From(ConnectEvents).CallDeferred();
	}
	public SingleWeapon(WeaponData data, WeaponCostume? costume, Node3D root) : base(data, costume, root) {
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


	public override IEnumerable<AttackAction.IAttackInfo> GetAttacks(Entity target) {
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

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		PropertyUsageFlags current = property["usage"].As<PropertyUsageFlags>();
		
		switch (property["name"].AsStringName()) {
			case nameof(Data) when Data is not null:
			case nameof(WeaponModel):
				property["usage"] = (int)(current | PropertyUsageFlags.ReadOnly);
				break;
			case nameof(Costume):
				property["usage"] = (int)(current & ~PropertyUsageFlags.Storage);
				break;
		}
	}
}