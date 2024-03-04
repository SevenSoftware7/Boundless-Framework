using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GDCol = Godot.Collections;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class ResourceManager : Node {
	[Export] private GDCol.Array<WeaponData> Weapons {
		get => [.. _weapons?.Values.Append(null) ?? []];
		set {
			try {
				SetWeapons(value);
			} catch (ArgumentException e) {
				GD.PushError(e.Message);
			}
		}
	}
	private static Dictionary<Type, WeaponData>? _weapons = null;


	[Export] private GDCol.Array<CharacterData> Characters {
		get => [.. _characters?.Values.Append(null) ?? []];
		set {
			try {
				SetCharacters(value);
			} catch (ArgumentException e) {
				GD.PushError(e.Message);
			}
		}
	}
	private static Dictionary<Type, CharacterData>? _characters = null;


	public ResourceManager() : base() {
		if (_characters is null) {
			SetCharacters(Characters);
		}
		if (_weapons is null) {
			SetWeapons(Weapons);
		}
	}


    private static void SetCharacters(IEnumerable<CharacterData> characters) {
		_characters = characters
			.Where(d => d is not null)
			.ToDictionary(keySelector: d => d.GetType());
	}
	private static void SetWeapons(IEnumerable<WeaponData> weapons) {
		_weapons = weapons
			.Where(d => d is not null)
			.ToDictionary(keySelector: d => d.GetType());
	}

    public static void RegisterWeapon<T>(T data, bool overwrite = false) where T : WeaponData {
		_weapons ??= [];
		Type type = typeof(T);
		if ( _weapons.ContainsKey(type) && ! overwrite ) return;

		_weapons[type] = data;
	}
	public static void RegisterCharacter<T>(T data, bool overwrite = false) where T : CharacterData {
		_characters ??= [];
		Type type = typeof(T);
		if ( _characters.ContainsKey(type) && ! overwrite ) return;

		_characters[type] = data;
	}

	public static T? GetRegisteredWeapon<T>() where T : WeaponData
		=> _weapons?.GetValueOrDefault(typeof(T)) as T;

	public static T? GetRegisteredCharacter<T>() where T : CharacterData
		=> _characters?.GetValueOrDefault(typeof(T)) as T;



    // public override void _Ready() {
    //     base._Ready();
    // }
}