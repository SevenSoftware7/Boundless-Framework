namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class ArmedEntity : Entity, IDamageDealer, ISerializationListener {
	private readonly List<uint> styleSwitchBuffer = [];


	public IWeapon? Weapon {
		get => _weapon;
		set {
			_weapon = value;
			if (_weapon is Node nodeWeapon) {
				nodeWeapon.SafeRename("Weapon");
			}
		}
	}
	private IWeapon? _weapon;


	IDamageable? IDamageDealer.Damageable => this;



	private void GetWeapon() => Weapon = GetChildren().OfType<IWeapon>().FirstOrDefault();


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (_weapon is null) return;

		uint maxWeaponStyle = (uint)Math.Min((int)(Weapon?.MaxStyle ?? 0) + 1, Inputs.SwitchWeaponActions.Length);
		uint? bufferStyle = null;
		for (uint i = 0; i < maxWeaponStyle; i++) {
			if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponActions[i])) {
				GD.PrintS(i, Inputs.SwitchWeaponActions[i]);
				bufferStyle = i;
				break;
			}
		}

		if (bufferStyle.HasValue) {
			if (CurrentAction is Attack attack && !attack.CanCancel()) {
				styleSwitchBuffer.Add(bufferStyle.Value);
			}
			else {
				_weapon.Style = bufferStyle.Value;
			}
		}
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (_weapon is null) return;
		if (CurrentAction is Attack attack && !attack.CanCancel()) return;

		foreach (uint bufferedStyle in styleSwitchBuffer) {
			_weapon.Style = bufferedStyle;
		}

		styleSwitchBuffer.Clear();
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationChildOrderChanged:
				GetWeapon();
				break;
		}
	}

	public override void OnAfterDeserialize() {
		base.OnAfterDeserialize();
		GetWeapon();
	}
}