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
		{ Traits.GenericGravity, 32f },
	};
	private static readonly Dictionary<TraitResource, float> DefaultTraitResourceDict = new(DefaultTraitValues.ToDictionary(trait => new TraitResource(trait.Key), trait => trait.Value));
	private static readonly Godot.Collections.Dictionary<TraitResource, float> DefaultTraitResourceValuesGodotDict = new(DefaultTraitResourceDict);


	private Dictionary<Trait, float> TraitValues = [];

	[Export]
	protected Godot.Collections.Dictionary<TraitResource, float> TraitValuesDict {
		get => _traitValuesDict;
		set {
			_traitValuesDict = value;
			TraitValues = value.ToDictionary(pair => pair.Key.Trait, pair => pair.Value);
		}
	}
	private Godot.Collections.Dictionary<TraitResource, float> _traitValuesDict;


	public TraitsCollection() : base() {
		_traitValuesDict = DefaultTraitResourceValuesGodotDict;
		TraitValues = DefaultTraitValues;
	}

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

		TraitResource traitRes = new(key);
		_traitValuesDict.Add(traitRes, value);

		EmitSignalPropertyListChanged();
	}
	public void Add(KeyValuePair<Trait, float> item) => Add(item.Key, item.Value);

	public bool ContainsKey(Trait key) => TraitValues.ContainsKey(key);

	public bool Remove(Trait key) {
		if (!TraitValues.ContainsKey(key)) return false;

		TraitResource? existing = _traitValuesDict.Keys.FirstOrDefault(traitResource => traitResource.Trait == key);
		if (existing is null) return false;

		bool res = _traitValuesDict.Remove(existing) && TraitValues.Remove(key);

		EmitSignalPropertyListChanged();

		return res;
	}

	public bool TryGetValue(Trait key, [MaybeNullWhen(false)] out float value) => TraitValues.TryGetValue(key, out value);

	public void Clear() {
		TraitValues.Clear();
		_traitValuesDict.Clear();

		EmitSignalPropertyListChanged();
	}

	public bool Contains(KeyValuePair<Trait, float> item) => TraitValues.Contains(item);

	public void CopyTo(KeyValuePair<Trait, float>[] array, int arrayIndex) => ((IDictionary<Trait, float>)TraitValues).CopyTo(array, arrayIndex);

	public bool Remove(KeyValuePair<Trait, float> item) => Remove(item.Key);

	public IEnumerator<KeyValuePair<Trait, float>> GetEnumerator() => TraitValues.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}