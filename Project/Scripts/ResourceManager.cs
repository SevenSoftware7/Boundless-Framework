using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenGame.Utility;

using GDCol = Godot.Collections;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class ResourceManager : Node, ISingleton<ResourceManager> {
	[Export] private GDCol.Array<WeaponData> _weapons {
		get => [.. Weapons.Values.Append(null)];
		set {
			try {
				Weapons = value
					.Where(d => d is not null)
					.ToDictionary(keySelector: d => d.GetType());
			} catch (ArgumentException e) {
				GD.PushError(e.Message);
			}
		}
	}
	private static Dictionary<Type, WeaponData> Weapons = [];


	[Export] private GDCol.Array<CharacterData> _characters {
		get => [.. Characters.Values.Append(null)];
		set {
			try {
				Characters = value
					.Where(d => d is not null)
					.ToDictionary(keySelector: d => d.GetType());
			} catch (ArgumentException e) {
				GD.PushError(e.Message);
			}
		}
	}
	private static Dictionary<Type, CharacterData> Characters = [];




	public ResourceManager() : base() {
		ISingleton<ResourceManager>.SetInstance(this);
	}


	public static void RegisterWeapon<T>(T data, bool overwrite = false) where T : WeaponData {
		Type type = typeof(T);
		if ( Weapons.ContainsKey(type) && ! overwrite ) return;

		Weapons[type] = data;
	}
	public static void RegisterCharacter<T>(T data, bool overwrite = false) where T : CharacterData {
		Type type = typeof(T);
		if ( Characters.ContainsKey(type) && ! overwrite ) return;

		Characters[type] = data;
	}

	public static T? GetRegisteredWeapon<T>() where T : WeaponData
		=> Weapons.GetValueOrDefault(typeof(T)) as T;

	public static T? GetRegisteredCharacter<T>() where T : CharacterData
		=> Characters.GetValueOrDefault(typeof(T)) as T;
}