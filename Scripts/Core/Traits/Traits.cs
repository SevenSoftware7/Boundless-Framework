namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public static class Traits {
	public static readonly Trait GenericMaxHealth = "generic_max_health";
	public static readonly Trait GenericMoveSpeed = "generic_move_speed";
	public static readonly Trait GenericSlowMoveSpeedMultiplier = "generic_slow_move_speed_multiplier";
	public static readonly Trait GenericFastMoveSpeedMultiplier = "generic_fast_move_speed_multiplier";
	public static readonly Trait GenericTurnSpeed = "generic_turn_speed";
	public static readonly Trait GenericAcceleration = "generic_acceleration";
	public static readonly Trait GenericDeceleration = "generic_deceleration";
	public static readonly Trait GenericStepHeight = "generic_step_height";
	public static readonly Trait GenericJumpHeight = "generic_jump_height";
	public static readonly Trait GenericGravity = "generic_gravity";

	public static readonly List<Trait> GenericTraits = [
		GenericMaxHealth,
		GenericMoveSpeed,
		GenericSlowMoveSpeedMultiplier,
		GenericFastMoveSpeedMultiplier,
		GenericTurnSpeed,
		GenericAcceleration,
		GenericDeceleration,
		GenericStepHeight,
		GenericJumpHeight,
		GenericGravity,
	];

	public static readonly StringName JoinedGenericTraits = string.Join(',', GenericTraits);

}