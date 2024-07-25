namespace LandlessSkies.Core;

using System.Collections.Generic;

public static class Attributes {
	public static readonly EntityAttribute GenericGravity = "generic_gravity";
	public static readonly EntityAttribute GenericMaxHealth = "generic_max_health";
	public static readonly EntityAttribute GenericMeleeDamage = "generic_melee_damage";
	public static readonly EntityAttribute GenericjumpHeight = "generic_jump_height";
	public static readonly EntityAttribute GenericMoveSpeed = "generic_move_speed";
	public static readonly EntityAttribute GenericTurnSpeed = "generic_turn_speed";

	public static readonly List<EntityAttribute> GenericAttributes = [
		GenericMaxHealth,
		GenericMeleeDamage,
		GenericMoveSpeed,
		GenericTurnSpeed,
		GenericjumpHeight,
		GenericGravity,
	];

}