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
			if (player.InputDevice.IsActionJustPressed("switch_weapon_primary")) {
				_weapon.Style = 0;
			}
			else if (player.InputDevice.IsActionJustPressed("switch_weapon_secondary")) {
				_weapon.Style = 1;
			}
			else if (player.InputDevice.IsActionJustPressed("switch_weapon_ternary")) {
				_weapon.Style = 2;
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