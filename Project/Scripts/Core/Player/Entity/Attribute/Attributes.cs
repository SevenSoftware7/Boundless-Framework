namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public static class Attributes {
	public static readonly StringName MeleeDamage = "melee_damage";
	public static readonly StringName jumpHeight = "jump_height";
	public static readonly StringName MoveSpeed = "move_speed";

	public static List<StringName> GenericAttributes { get; private set; } = [
		MeleeDamage,
		jumpHeight,
		MoveSpeed,
	];

}