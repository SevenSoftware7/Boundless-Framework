namespace Seven.Boundless;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;


public sealed class TraitModifierCollection : ICollection<ITraitModifier> {
	public delegate double InterpFunction(double start, double end, double t);

	private readonly Dictionary<Trait, TraitModifierEntry> _dictionary = [];

	public int Count => _dictionary.Values.Sum(e => e.Count);
	public bool IsReadOnly => false;

	public event Action<Trait>? OnModifiersUpdated;


	// private async Task ProgressivelyMoveTo(TraitModifierEntry entry, ITraitModifier item, float start, float end, uint timeMilliseconds, InterpFunction? function = null) {
	// 	function ??= Mathf.Lerp;

	// 	await foreach (float elapsed in AsyncUtils.WaitAndYield(timeMilliseconds)) {
	// 		ref float multiplierRef = ref CollectionsMarshal.GetValueRefOrNullRef(entry.Modifiers, item);
	// 		if (Unsafe.IsNullRef(ref multiplierRef)) break;

	// 		multiplierRef = function(start, end, elapsed / timeMilliseconds);
	// 		OnModifiersUpdated?.Invoke(item.Trait);
	// 	}
	// }

	public float ApplyTo(Trait target, float baseValue) {
		if (!_dictionary.TryGetValue(target, out TraitModifierEntry entry))
			return baseValue;

		return entry.ApplyTo(baseValue);
	}

	public void Add(ITraitModifier item) {
		AddInternal(item);
		OnModifiersUpdated?.Invoke(item.Trait);
	}

	public void AddRange(IEnumerable<ITraitModifier> items) {
		HashSet<Trait> traits = [];
		foreach (ITraitModifier item in items) {
			AddInternal(item);
			traits.Add(item.Trait);
		}
		foreach (Trait trait in traits) {
			OnModifiersUpdated?.Invoke(trait);
		}
	}

	public void AddProgressively(Node node, ITraitModifier item, uint durationMilliseconds, uint delayMilliseconds = 0, InterpFunction? function = null) {
		if (durationMilliseconds == 0 && delayMilliseconds == 0) {
			Add(item);
			return;
		}

		TraitModifierAdder adder = new(this, item) {
			Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
			Delay = TimeSpan.FromMilliseconds(delayMilliseconds),
			InterpolationFunction = function
		};

		node.AddChild(adder);
	}

	// public async Task AddProgressively(ITraitModifier item, uint timeMilliseconds = 0, InterpFunction? function = null) {
	// 	await AddProgressivelyInternal(item, timeMilliseconds, function);
	// 	if (timeMilliseconds == 0) OnModifiersUpdated?.Invoke(item.Trait);
	// }

	// private async Task AddProgressivelyInternal(ITraitModifier item, uint timeMilliseconds, InterpFunction? function) {
	// 	TraitModifierEntry entry = AddInternal(item);

	// 	if (timeMilliseconds > 0) {
	// 		await ProgressivelyMoveTo(entry, item, 0f, 1f, timeMilliseconds, function);
	// 	}
	// }

	private TraitModifierEntry AddInternal(ITraitModifier item) {
		ref TraitModifierEntry entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, item.Trait, out bool existed);

		if (!existed) {
			entry = new(item);
		}
		else {
			entry.Add(item);
		}

		item.OnValueModified += OnModifiersUpdated;

		return entry;
	}


	public bool Remove(ITraitModifier item) {
		bool wasRemoved = RemoveInternal(item);
		if (wasRemoved) OnModifiersUpdated?.Invoke(item.Trait);

		return wasRemoved;
	}

	public void RemoveRange(IEnumerable<TraitModifier> items) {
		HashSet<Trait> traits = [];
		foreach (TraitModifier item in items) {
			RemoveInternal(item);
			traits.Add(item.Trait);
		}

		foreach (Trait trait in traits) {
			OnModifiersUpdated?.Invoke(trait);
		}
	}

	public void RemoveProgressively(Node node, ITraitModifier item, uint durationMilliseconds, uint delayMilliseconds = 0, InterpFunction? function = null) {
		if (durationMilliseconds == 0 && delayMilliseconds == 0) {
			Remove(item);
			return;
		}

		TraitModifierRemover remover = new(this, item) {
			Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
			Delay = TimeSpan.FromMilliseconds(delayMilliseconds),
			InterpolationFunction = function
		};

		node.AddChild(remover);
	}

	// public async Task<bool> RemoveProgressively(TraitModifier item, uint timeMilliseconds = 0, InterpFunction? function = null) {
	// 	bool wasRemoved = await RemoveProgressivelyInternal(item, timeMilliseconds, function);
	// 	if (wasRemoved && timeMilliseconds == 0) OnModifiersUpdated?.Invoke(item.Trait);

	// 	return wasRemoved;
	// }

	// private async Task<bool> RemoveProgressivelyInternal(TraitModifier item, uint timeMilliseconds = 0, InterpFunction? function = null) {
	// 	ref var entryRef = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, item.Trait);
	// 	if (Unsafe.IsNullRef(ref entryRef)) return false;
	// 	if (!entryRef.Contains(item)) return false;

	// 	TraitModifierEntry entry = entryRef;

	// 	if (timeMilliseconds > 0) {
	// 		await ProgressivelyMoveTo(entry, item, 1f, 0f, timeMilliseconds, function);
	// 	}

	// 	if (!entry.Remove(item)) return false;

	// 	item.OnValueModified -= OnModifiersUpdated;
	// 	return true;
	// }

	private bool RemoveInternal(ITraitModifier item) {
		ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, item.Trait);
		if (Unsafe.IsNullRef(ref entry) || !entry.Remove(item))
			return false;

		item.OnValueModified -= OnModifiersUpdated;
		return true;
	}


	public void Set(IEnumerable<ITraitModifier> modifiers) {
		HashSet<ITraitModifier> newModifiersSet = [.. modifiers];
		HashSet<Trait> modifiedTraits = [];

		// Iterate over the current modifiers and collect those that need to be removed
		List<ITraitModifier> currentModifiers = _dictionary.Values.SelectMany(e => e.List).ToList();
		foreach (ITraitModifier currentModifier in currentModifiers) {
			if (!newModifiersSet.Contains(currentModifier)) {
				RemoveInternal(currentModifier);
				modifiedTraits.Add(currentModifier.Trait);
			}
		}

		// Iterate over the new modifiers and add those that are not already in the collection
		foreach (ITraitModifier newModifier in newModifiersSet) {
			if (!Contains(newModifier)) {
				AddInternal(newModifier);
				modifiedTraits.Add(newModifier.Trait);
			}
		}

		// Call OnModifiersUpdated for each modified Trait
		foreach (Trait trait in modifiedTraits) {
			OnModifiersUpdated?.Invoke(trait);
		}
	}

	public void SetMultiplier(ITraitModifier modifier, float multiplier) {
		ref var entryRef = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, modifier.Trait);
		if (Unsafe.IsNullRef(ref entryRef))
			throw new KeyNotFoundException("Trait not found in collection");

		ref float multiplierRef = ref CollectionsMarshal.GetValueRefOrNullRef(entryRef.Modifiers, modifier);
		if (Unsafe.IsNullRef(ref multiplierRef))
			throw new KeyNotFoundException("TraitModifier not found in collection");

		multiplierRef = multiplier;
		OnModifiersUpdated?.Invoke(modifier.Trait);
	}

	public void Clear() {
		foreach (Trait trait in _dictionary.Keys) {
			_dictionary.Remove(trait);
			OnModifiersUpdated?.Invoke(trait);
		}
	}

	public bool Contains(ITraitModifier item) =>
		_dictionary.TryGetValue(item.Trait, out TraitModifierEntry entry) &&
		entry.Contains(item);

	public void CopyTo(ITraitModifier[] array, int arrayIndex) =>
		_dictionary.Values
			.SelectMany(e => e.List)
			.ToList()
			.CopyTo(array, arrayIndex);

	public IEnumerator<ITraitModifier> GetEnumerator() =>
		_dictionary.Values
		.SelectMany(e => e.List)
		.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



	private readonly struct TraitModifierEntry : ICollection<ITraitModifier> {
		public readonly Dictionary<ITraitModifier, float> Modifiers = [];


		public readonly List<ITraitModifier> List =>
			[.. Modifiers.Keys];

		public readonly int Count => Modifiers.Count;
		public readonly bool IsReadOnly => false;


		public TraitModifierEntry() { }
		public TraitModifierEntry(ITraitModifier modifier) : this() {
			Add(modifier);
		}
		public TraitModifierEntry(IEnumerable<ITraitModifier> modifiers) : this() {
			foreach (ITraitModifier modifier in modifiers) {
				Add(modifier);
			}
		}


		public readonly void Add(ITraitModifier item) =>
			Modifiers.Add(item, 1f);

		public readonly bool Remove(ITraitModifier item) =>
			Modifiers.Remove(item);

		public readonly void Clear() =>
			Modifiers.Clear();



		public readonly void CopyTo(ITraitModifier[] array, int arrayIndex) =>
			List.CopyTo(array, arrayIndex);


		public readonly float ApplyTo(float baseValue) {
			float result = baseValue;

			List<KeyValuePair<ITraitModifier, float>> stacking = new(Modifiers.Count);

			foreach (KeyValuePair<ITraitModifier, float> kvp in Modifiers) {
				if (kvp.Key.IsStacking) {
					stacking.Add(kvp);
				} else {
					result = kvp.Key.ApplyTo(result, kvp.Value);
				}
			}

			foreach ((ITraitModifier trait, float multiplier) in stacking) {
				result += trait.ApplyTo(baseValue, multiplier) - baseValue;
			}

			return result;
		}


		public readonly bool Contains(ITraitModifier item) =>
			Modifiers.ContainsKey(item);

		public readonly IEnumerator<ITraitModifier> GetEnumerator() =>
			List.GetEnumerator();
		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}