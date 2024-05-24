namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public static class Attributes {
	public static readonly StringName GenericMaxHealth = "generic_max_health";
	public static readonly StringName GenericMeleeDamage = "generic_melee_damage";
	public static readonly StringName GenericjumpHeight = "generic_jump_height";
	public static readonly StringName GenericMoveSpeed = "generic_move_speed";

	public static List<StringName> GenericAttributes { get; private set; } = [
		GenericMaxHealth,
		GenericMeleeDamage,
		GenericjumpHeight,
		GenericMoveSpeed,
	];

}