using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon {
	private int _style;
	private Entity? _entity;
	private WeaponData _data = null!;
	private IWeapon.Handedness _weaponHandedness = IWeapon.Handedness.Right;
	
	

	
	[Export] public override WeaponData Data {
		get => _data;
		protected set {
			if (_data is not null) return;
			_data = value;

			if ( this.IsInitializationSetterCall() ) return;
			if (Costume is not null) return;
			SetCostume(_data?.BaseCostume);
		}
	}

	[ExportGroup("Costume")]
	[Export] public override WeaponCostume? Costume {
		get => WeaponModel?.Costume;
		set {
			if ( this.IsInitializationSetterCall() ) return;
			SetCostume(value);
		}
	}
	[Export] private WeaponModel? WeaponModel;


	[ExportGroup("Dependencies")]
	[Export] public Entity? Entity {
		get => _entity;
		private set {
			if ( this.IsInitializationSetterCall() ) {
				_entity = value;
				return;
			}
			Inject(value);
		}
	}

	protected virtual uint StyleMax { get; } = 0;
	public override int Style { 
		get => _style;
		set => _style = value % ((int)StyleMax + 1);
	}

	public override IWeapon.Handedness WeaponHandedness {
		get => _weaponHandedness;
		set {
			_weaponHandedness = value;
			WeaponModel?.Inject(value);
		}
	}

	public override ICustomizable[] Children => [.. base.Children.Append(WeaponModel!)];



	protected SingleWeapon() : base() {}
	public SingleWeapon(WeaponData data, WeaponCostume? costume) : base(data, costume) {}


	public override void SetCostume(WeaponCostume? costume) {
		WeaponCostume? oldCostume = Costume;
		if ( costume == oldCostume ) return;

		new LoadableUpdater<WeaponModel>(ref WeaponModel, () => costume?.Instantiate(this))
			.BeforeLoad(m => {
				m.Inject(Entity?.Skeleton);
				m.Inject(WeaponHandedness);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}

	public override void Inject(Entity? entity) {
		if (entity == _entity) return;

		Callable callableLoadUnloadEvent = new(this, MethodName.OnCharacterLoadedUnloaded);
		_entity?.Disconnect(Entity.SignalName.CharacterLoadedUnloaded, callableLoadUnloadEvent);

		_entity = entity;

		WeaponModel?.Inject(entity?.Skeleton);
		entity?.Connect(Entity.SignalName.CharacterLoadedUnloaded, callableLoadUnloadEvent, (uint)ConnectFlags.Persist);
	}


	private void OnCharacterLoadedUnloaded(bool isLoaded) {
		WeaponModel?.Inject(Entity?.Skeleton);
	}


	protected override bool LoadModelImmediate() {
		return WeaponModel is WeaponModel model && (model.IsLoaded || (model?.LoadModel() ?? false));
	}
	protected override bool UnloadModelImmediate() {
		return WeaponModel is WeaponModel model && (! model.IsLoaded || (model?.UnloadModel() ?? false));
	}



	public override void _Predelete() {
		base._Predelete();

		UnloadModel();
		WeaponModel?.QueueFree();
	}

	public override bool _PropertyCanRevert(StringName property) {
		if (property == PropertyName.Costume) {
			return Costume != Data?.BaseCostume;
		}
		return base._PropertyCanRevert(property);
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.Costume) {
			return Data?.BaseCostume!;
		}
		return base._PropertyGetRevert(property);
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if (
			name == PropertyName.WeaponModel ||
			(name == PropertyName.Data && Data is not null)
		) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		
		} else if (
			name == PropertyName.Costume
		) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}
}