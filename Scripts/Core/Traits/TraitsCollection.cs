namespace LandlessSkies.Core;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
	private static readonly Dictionary<TraitResource, float> DefaultTraitResourceDict = new(DefaultTraitValues.ToDictionary(trait => new TraitResource(trait.Key), trait => trait.Value));
	private static readonly Godot.Collections.Dictionary<TraitResource, float> DefaultTraitResourceValuesGodotDict = new(DefaultTraitResourceDict);



	[Export]
	protected Godot.Collections.Dictionary<TraitResource, float> TraitValuesDict {
		get => TraitValues.GodotDict;
		set => TraitValues.GodotDict = value;
	}
	private TraitDictionaryWrapper TraitValues = new(
		DefaultTraitValues,
		DefaultTraitResourceValuesGodotDict
	);


	public TraitsCollection() : base() { }

	public override bool _PropertyCanRevert(StringName property) {
		return base._PropertyCanRevert(property) || property == PropertyName.TraitValuesDict;
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.TraitValuesDict) return DefaultTraitResourceValuesGodotDict;
		return base._PropertyGetRevert(property);
	}


	public ICollection<Trait> Keys => TraitValues.Keys;
	public ICollection<float> Values => TraitValues.Values;
	public int Count => TraitValues.Count;
	public bool IsReadOnly => false;
	IEnumerable<Trait> IReadOnlyDictionary<Trait, float>.Keys => Keys;
	IEnumerable<float> IReadOnlyDictionary<Trait, float>.Values => Values;

	public float this[Trait key] {
		get => TraitValues[key];
		set => TraitValues[key] = value;
	}

	public void Add(Trait key, float value) {
		TraitValues.Add(key, value);

		EmitSignalPropertyListChanged();
	}
	public void Add(KeyValuePair<Trait, float> item) => Add(item.Key, item.Value);

	public bool Remove(Trait key) {
		if (!TraitValues.ContainsKey(key)) return false;

		bool res = TraitValues.Remove(key);

		EmitSignalPropertyListChanged();

		return res;
	}

	public void Clear() {
		TraitValues.Clear();

		EmitSignalPropertyListChanged();
	}

	public float GetOrDefault(Trait key, float defaultValue = default) {
		if (TraitValues.TryGetValue(key, out float value)) return value;
		if (DefaultTraitValues.TryGetValue(key, out value)) return value;
		return defaultValue;
	}

	public bool ContainsKey(Trait key) => TraitValues.ContainsKey(key);

	public bool Contains(KeyValuePair<Trait, float> item) => TraitValues.Contains(item);

	public bool TryGetValue(Trait key, [MaybeNullWhen(false)] out float value) => TraitValues.TryGetValue(key, out value);

	public void CopyTo(KeyValuePair<Trait, float>[] array, int arrayIndex) => ((IDictionary<Trait, float>)TraitValues).CopyTo(array, arrayIndex);

	public bool Remove(KeyValuePair<Trait, float> item) => Remove(item.Key);

	public IEnumerator<KeyValuePair<Trait, float>> GetEnumerator() => TraitValues.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


	private struct TraitDictionaryWrapper : IDictionary<Trait, float>, IReadOnlyDictionary<Trait, float> {
		public readonly ReadOnlyDictionary<Trait, float> Dict => _dict.AsReadOnly();
		private Dictionary<Trait, float> _dict;

		public Godot.Collections.Dictionary<TraitResource, float> GodotDict {
			readonly get => new (_godotDict);
			set {
				_godotDict = value;
				_dict = value.ToDictionary(pair => pair.Key.Trait, pair => pair.Value);
			}
		}
		private Godot.Collections.Dictionary<TraitResource, float> _godotDict;


		public TraitDictionaryWrapper(Dictionary<Trait, float> dict, Godot.Collections.Dictionary<TraitResource, float> godotDict) {
			_dict = dict;
			_godotDict = godotDict;
		}


		public readonly float this[Trait key] {
			get => _dict[key];
			set => _dict[key] = value;
		}

		public readonly ICollection<Trait> Keys => _dict.Keys;
		public readonly ICollection<float> Values => _dict.Values;
		public readonly int Count => _dict.Count;
		public readonly bool IsReadOnly => ((IDictionary<Trait, float>)_dict).IsReadOnly;
		readonly IEnumerable<Trait> IReadOnlyDictionary<Trait, float>.Keys => _dict.Keys;
		readonly IEnumerable<float> IReadOnlyDictionary<Trait, float>.Values => _dict.Values;

		public readonly void Add(Trait key, float value) {
			_dict.Add(key, value);

			TraitResource traitRes = new(key);
			_godotDict.Add(traitRes, value);
		}

		public readonly void Add(KeyValuePair<Trait, float> item) {
			Add(item.Key, item.Value);
		}

		public readonly void Clear() {
			_dict.Clear();
			_godotDict.Clear();
		}

		public readonly bool Contains(KeyValuePair<Trait, float> item) {
			return ((IDictionary<Trait, float>)_dict).Contains(item);
		}

		public readonly bool ContainsKey(Trait key) {
			return _dict.ContainsKey(key);
		}

		public readonly void CopyTo(KeyValuePair<Trait, float>[] array, int arrayIndex) {
			((IDictionary<Trait, float>)_dict).CopyTo(array, arrayIndex);
		}

		public readonly IEnumerator<KeyValuePair<Trait, float>> GetEnumerator() => _dict.GetEnumerator();

		public readonly bool Remove(Trait key) {
			TraitResource? existing = _godotDict.Keys.FirstOrDefault(traitResource => traitResource.Trait == key);
			if (existing is null) return false;

			return _godotDict.Remove(existing) && _dict.Remove(key);
		}

		public readonly bool Remove(KeyValuePair<Trait, float> item) {
			return ((IDictionary<Trait, float>)_dict).Remove(item);
		}

		public readonly bool TryGetValue(Trait key, [MaybeNullWhen(false)] out float value) {
			return _dict.TryGetValue(key, out value);
		}

		readonly IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}