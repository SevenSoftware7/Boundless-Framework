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

			if ( this.IsEditorGetSetter() ) return;
			if (Costume is not null) return;
			SetCostume(_data?.BaseCostume);
		}
	}

	[ExportGroup("Costume")]
	[Export] public override WeaponCostume? Costume {
		get => WeaponModel?.Costume;
		set {
			if ( this.IsEditorGetSetter() ) return;
			SetCostume(value);
		}
	}
	[Export] private WeaponModel? WeaponModel;


	[ExportGroup("Dependencies")]
	[Export] public Entity? Entity {
		get => _entity;
		private set {
			if ( this.IsEditorGetSetter() ) {
				_entity = value;
				return;
			}
			Inject(value);
		}
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
		
		if (this.JustBuilt()) Callable.From(() => {
			DisconnectEvents();
			ConnectEvents();
		}).CallDeferred();
	}
	public SingleWeapon(WeaponData data, WeaponCostume? costume) : base(data, costume) {
		InitializeAttacks();
	}



	protected virtual void InitializeAttacks() {}


	public override void SetCostume(WeaponCostume? costume) {
		WeaponCostume? oldCostume = Costume;
		if ( costume == oldCostume ) return;

		new LoadableUpdater<WeaponModel>(ref WeaponModel, () => costume?.Instantiate(this))
			.BeforeLoad(m => {
				m.Inject(Entity?.Armature);
				m.Inject(WeaponHandedness);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}

	public override void Inject(Entity? entity) {
		DisconnectEvents();
		_entity = entity;

		WeaponModel?.Inject(entity?.Armature);
		
		if ( entity is null ) return;
		ConnectEvents();
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


	public override IEnumerable<AttackAction.IInfo> GetAttacks(Entity target) {
		return [];
	}

	protected override bool LoadModelImmediate() {
		return WeaponModel?.LoadModel() ?? false;
	}
	protected override bool UnloadModelImmediate() {
		return WeaponModel?.UnloadModel() ?? false;
	}


	public override void _Ready() {
		base._Ready();

		ConnectEvents();
	}
	public override void _ExitTree() {
		base._ExitTree();

		DisconnectEvents();
	}

	public override void _Predelete() {
		base._Predelete();

		UnloadModel();
		WeaponModel?.QueueFree();
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		switch (property["name"].AsStringName()) {
			case nameof(Data) when Data is not null:
			case nameof(WeaponModel):
				property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
				break;
			case nameof(Costume):
				property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
				break;
		}
	}
}