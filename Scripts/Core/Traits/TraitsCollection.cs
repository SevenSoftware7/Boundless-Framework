namespace LandlessSkies.Core;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

[Tool]
[GlobalClass]
public partial class TraitsCollection : Resource, IDictionary<Trait, float>, IReadOnlyDictionary<Trait, float> {
	public static readonly Dictionary<Trait, float> DefaultTraitValues = new() {
		{ Traits.GenericMaxHealth, 25f },
		{ Traits.GenericMoveSpeed, 8f },
		{ Traits.GenericSlowMoveSpeedMultiplier, 0.375f },
		{ Traits.GenericFastMoveSpeedMultiplier, 1.5f },
		{ Traits.GenericTurnSpeed, 20f },
		{ Traits.GenericAcceleration, 50f },
		{ Traits.GenericDeceleration, 35f },
		{ Traits.GenericStepHeight, 0.5f },
		{ Traits.GenericJumpHeight, 20f },
		{ Traits.GenericGravity, 32f },
	};
	private static readonly Dictionary<StringName, float> DefaultTraitResourceDict = new(DefaultTraitValues.ToDictionary(trait => trait.Key.Name, trait => trait.Value));
	private static readonly Godot.Collections.Dictionary<StringName, float> DefaultTraitResourceValuesGodotDict = new(DefaultTraitResourceDict);


	private Dictionary<Trait, float> _traitValues = new(DefaultTraitValues);
	[Export]
	protected Godot.Collections.Dictionary<StringName, float> TraitValues {
		get;
		set {
			field = value;
			_traitValues = new Dictionary<Trait, float>(value.ToDictionary(p => new Trait(p.Key), p => p.Value));
		}
	} = new (DefaultTraitResourceValuesGodotDict);



	public override bool _PropertyCanRevert(StringName property) {
		return base._PropertyCanRevert(property) || property == PropertyName.TraitValues;
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.TraitValues) return DefaultTraitResourceValuesGodotDict;
		return base._PropertyGetRevert(property);
	}


	public ICollection<Trait> Keys => _traitValues.Keys;
	public ICollection<float> Values => _traitValues.Values;
	public int Count => _traitValues.Count;
	public bool IsReadOnly => false;
	IEnumerable<Trait> IReadOnlyDictionary<Trait, float>.Keys => Keys;
	IEnumerable<float> IReadOnlyDictionary<Trait, float>.Values => Values;

	public float this[Trait key] {
		get => _traitValues[key];
		set => _traitValues[key] = value;
	}

	public void Add(Trait key, float value) {
		_traitValues.Add(key, value);
		TraitValues.Add(key.Name, value);

		EmitSignalPropertyListChanged();
	}
	public void Add(KeyValuePair<Trait, float> item) => Add(item.Key, item.Value);

	public bool Remove(Trait key) {
		bool res = _traitValues.Remove(key);
		if (res) {
			TraitValues.Remove(key.Name);
			EmitSignalPropertyListChanged();
		}

		return res;
	}

	public void Clear() {
		_traitValues.Clear();
		TraitValues.Clear();

		EmitSignalPropertyListChanged();
	}

	public float GetOrDefault(Trait key, float defaultValue = default) {
		if (_traitValues.TryGetValue(key, out float value)) return value;
		if (DefaultTraitValues.TryGetValue(key, out value)) return value;
		return defaultValue;
	}

	public bool ContainsKey(Trait key) => _traitValues.ContainsKey(key);

	public bool Contains(KeyValuePair<Trait, float> item) => _traitValues.Contains(item);

	public bool TryGetValue(Trait key, [MaybeNullWhen(false)] out float value) => _traitValues.TryGetValue(key, out value);

	public void CopyTo(KeyValuePair<Trait, float>[] array, int arrayIndex) => ((IDictionary<Trait, float>)TraitValues).CopyTo(array, arrayIndex);

	public bool Remove(KeyValuePair<Trait, float> item) => Remove(item.Key);

	public IEnumerator<KeyValuePair<Trait, float>> GetEnumerator() => _traitValues.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}