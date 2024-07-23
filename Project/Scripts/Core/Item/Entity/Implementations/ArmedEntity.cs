namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public partial class ArmedEntity : Entity, IDamageDealer {
	private readonly List<int> styleSwitchBuffer = [];

	[ExportGroup("Weapon")]
	public IWeapon? Weapon {
		get => _weapon;
		set {
			_weapon = value;
			if (_weapon is Node nodeWeapon) {
				nodeWeapon.Name = "Weapon";
			}
		}
	}
	private IWeapon? _weapon;


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (_weapon is null) return;


		int bufferStyle = -1;
		if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponPrimary)) {
			bufferStyle = 0;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponSecondary)) {
			bufferStyle = 1;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponTertiary)) {
			bufferStyle = 2;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponQuaternary)) {
			bufferStyle = 3;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponQuinary)) {
			bufferStyle = 4;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponSenary)) {
			bufferStyle = 5;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponSeptenary)) {
			bufferStyle = 6;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponOctonary)) {
			bufferStyle = 7;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponNonary)) {
			bufferStyle = 8;
		}
		else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponDenary)) {
			bufferStyle = 9;
		}

		if (bufferStyle != -1) {
			if (CurrentAction is Attack) {
				styleSwitchBuffer.Add(bufferStyle);
			}
			else {
				_weapon.Style = bufferStyle;
			}
		}
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (CurrentAction is Attack || _weapon is null) return;

		for (int i = 0; i < styleSwitchBuffer.Count; i++) {
			_weapon.Style = styleSwitchBuffer[i];
		}

		styleSwitchBuffer.Clear();
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationChildOrderChanged:
				Weapon = GetChildren().OfType<IWeapon>().FirstOrDefault();
				break;
		}
	}
}