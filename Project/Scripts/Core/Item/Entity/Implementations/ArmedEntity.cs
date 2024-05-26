namespace LandlessSkies.Core;

using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public partial class ArmedEntity : Entity {
	[ExportGroup("Weapon")]
	[Export] public Weapon? Weapon {
		get => _weapon;
		set {
			_weapon?.Inject(null);

			_weapon = value;
			if (_weapon is not null) {
				_weapon.Inject(Skeleton);
				_weapon.Inject(Handedness);
				_weapon.Name = PropertyName.Weapon;
			}
		}
	}
	private Weapon? _weapon;


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		if (_weapon is not null) {
			if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponPrimary)) {
				_weapon.Style = 0;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponSecondary)) {
				_weapon.Style = 1;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponTertiary)) {
				_weapon.Style = 2;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponQuaternary)) {
				_weapon.Style = 3;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponQuinary)) {
				_weapon.Style = 4;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponSenary)) {
				_weapon.Style = 5;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponSeptenary)) {
				_weapon.Style = 6;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponOctonary)) {
				_weapon.Style = 7;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponNonary)) {
				_weapon.Style = 8;
			}
			else if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponDenary)) {
				_weapon.Style = 9;
			}
		}
	}


	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
		case NotificationChildOrderChanged:
			Weapon ??= GetChildren().OfType<Weapon>().FirstOrDefault();
			break;
		}
	}
}