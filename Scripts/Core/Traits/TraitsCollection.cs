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
		{ Traits.GenericMaxHealth, 25f },
		{ Traits.GenericMoveSpeed, 8f },
		{ Traits.GenericTurnSpeed, 20f },
		{ Traits.GenericAcceleration, 50f },
		{ Traits.GenericDeceleration, 35f },
		{ Traits.GenericStepHeight, 0.5f },
		{ Traits.GenericJumpHeight, 20f },
		{ Traits.GenericGravity, 1f },
	};
	private static Dictionary<TraitResource, float> GetDefaultTraitResourceValues() => new(DefaultTraitValues.ToDictionary(trait => new TraitResource(trait.Key), trait => trait.Value));
	private static readonly Dictionary<TraitResource, float> DefaultTraitResourceDict = GetDefaultTraitResourceValues();
	private static readonly Godot.Collections.Dictionary<TraitResource, float> DefaultTraitResourceValuesGodotDict = new(DefaultTraitResourceDict);

	private Dictionary<TraitResource, float> TraitValues = DefaultTraitResourceDict;

	[Export]
	protected Godot.Collections.Dictionary<TraitResource, float> TraitValuesDict {
		get => new (TraitValues);
		set => TraitValues = new (value);
	}
	public ICollection<Trait> Keys => [.. TraitValues.Keys.Select(traitResource => traitResource.Trait)];
	public ICollection<float> Values => TraitValues.Values;
	public int Count => TraitValues.Count;
	public bool IsReadOnly => false;
	IEnumerable<Trait> IReadOnlyDictionary<Trait, float>.Keys => Keys;
	IEnumerable<float> IReadOnlyDictionary<Trait, float>.Values => Values;

	public float this[Trait key] {
		get => TraitValues[key];
		set => TraitValues[key] = value;
	}

	public override bool _PropertyCanRevert(StringName property) {
		return base._PropertyCanRevert(property) || property == PropertyName.TraitValuesDict;
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.TraitValuesDict) return DefaultTraitResourceValuesGodotDict;
		return base._PropertyGetRevert(property);
	}


	public void Add(Trait key, float value) => TraitValues.Add(key, value);

	public bool ContainsKey(Trait key) => TraitValues.ContainsKey(key);

	public bool Remove(Trait key) => TraitValues.Remove(key);

	public bool TryGetValue(Trait key, [MaybeNullWhen(false)] out float value) =>
		TraitValues.TryGetValue(key, out value);

	public void Add(KeyValuePair<Trait, float> item) => Add(item.Key, item.Value);

	public void Clear() => TraitValues.Clear();

	public bool Contains(KeyValuePair<Trait, float> item) =>
		TraitValues.ContainsKey(item.Key) && TraitValues[item.Key] == item.Value;

	public void CopyTo(KeyValuePair<Trait, float>[] array, int arrayIndex) {
		foreach (var item in TraitValues) {
			array[arrayIndex++] = new KeyValuePair<Trait, float>(item.Key, item.Value);
		}
	}

	public bool Remove(KeyValuePair<Trait, float> item) => Contains(item) && Remove(item.Key);

	public IEnumerator<KeyValuePair<Trait, float>> GetEnumerator() =>
		TraitValues.Select(item => new KeyValuePair<Trait, float>(item.Key, item.Value)).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}