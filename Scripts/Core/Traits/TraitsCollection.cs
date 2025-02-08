namespace LandlessSkies.Core;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public partial class TraitsCollection : Resource, IDictionary<Trait, float>, IReadOnlyDictionary<Trait, float> {
	public static readonly Dictionary<Trait, float> DefaultTraitValues = new() {
		{ Traits.GenericMaxHealth, 1f },
		{ Traits.GenericMeleeDamage, 1f},
		{ Traits.GenericMoveSpeed, 8f},
		{ Traits.GenericTurnSpeed, 20f},
		{ Traits.GenericStepHeight, 0.5f},
		{ Traits.GenericJumpHeight, 20f},
		{ Traits.GenericGravity, 1f},
	};

	public Dictionary<Trait, float> TraitValues = new(DefaultTraitValues);

	[Export]
	private Godot.Collections.Dictionary<TraitResource, float> _traitValues {
		get => new(TraitValues.ToDictionary(pair => new TraitResource { Trait = pair.Key }, pair => pair.Value));
		set => TraitValues = value.ToDictionary(pair => pair.Key.Trait, pair => pair.Value);
	}


	public ICollection<Trait> Keys => TraitValues.Keys;
	public ICollection<float> Values => TraitValues.Values;
	public int Count => TraitValues.Count;
	public bool IsReadOnly => ((IDictionary<Trait, float>)TraitValues).IsReadOnly;
	IEnumerable<Trait> IReadOnlyDictionary<Trait, float>.Keys => TraitValues.Keys;
	IEnumerable<float> IReadOnlyDictionary<Trait, float>.Values => TraitValues.Values;

	public float this[Trait key] {
		get => TraitValues[key];
		set => TraitValues[key] = value;
	}

	public TraitsCollection() : base() { }

	public float GetOrDefault(Trait key) {
		return TraitValues.TryGetValue(key, out float value)
			? value
			: DefaultTraitValues.TryGetValue(key, out value)
				? value
				: 0f;
	}
	public float GetOrDefault(Trait key, float @default) {
		return TraitValues.TryGetValue(key, out float value)
			? value
			: @default;
	}

	public void Add(Trait key, float value) => TraitValues.Add(key, value);

	public bool ContainsKey(Trait key) => TraitValues.ContainsKey(key);

	public bool Remove(Trait key) => TraitValues.Remove(key);

	public bool TryGetValue(Trait key, [MaybeNullWhen(false)] out float value) => TraitValues.TryGetValue(key, out value);

	public void Add(KeyValuePair<Trait, float> item) => ((IDictionary<Trait, float>)TraitValues).Add(item);

	public void Clear() => TraitValues.Clear();

	public bool Contains(KeyValuePair<Trait, float> item) => ((IDictionary<Trait, float>)TraitValues).Contains(item);

	public void CopyTo(KeyValuePair<Trait, float>[] array, int arrayIndex) => ((IDictionary<Trait, float>)TraitValues).CopyTo(array, arrayIndex);

	public bool Remove(KeyValuePair<Trait, float> item) => ((IDictionary<Trait, float>)TraitValues).Remove(item);

	public IEnumerator<KeyValuePair<Trait, float>> GetEnumerator() => TraitValues.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}