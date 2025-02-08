namespace LandlessSkies.Core;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;

public sealed class TraitModifierCollection : ICollection<TraitModifier> {
	private readonly Dictionary<Trait, TraitModifierEntry> _dictionary = [];

	public int Count => _dictionary.Values.Sum(e => e.Count);
	public bool IsReadOnly => false;

	public event Action<Trait>? OnModifiersUpdated;


	public float ApplyTo(Trait target, float baseValue) {
		if (!_dictionary.TryGetValue(target, out TraitModifierEntry entry))
			return baseValue;

		return entry.ApplyTo(baseValue);
	}

	public void Add(TraitModifier item) {
		AddInternal(item);
		OnModifiersUpdated?.Invoke(item.Trait);
	}
	public async Task AddProgressively(TraitModifier item, uint timeMilliseconds = 0, Func<float, float, float, float>? function = null) {
		await AddProgressivelyInternal(item, timeMilliseconds, function);
		if (timeMilliseconds == 0) OnModifiersUpdated?.Invoke(item.Trait);
	}

	private async Task AddProgressivelyInternal(TraitModifier item, uint timeMilliseconds, Func<float, float, float, float>? function) {
		AddInternal(item);

		if (timeMilliseconds > 0) {
			function ??= Mathf.Lerp;

			float start = 0f;
			float end = item.Efficiency;

			await AsyncUtils.WaitAndCall(timeMilliseconds, (elapsed) => {
				item.Efficiency = function(start, end, (float)elapsed / timeMilliseconds);
			});
		}
	}
	private void AddInternal(TraitModifier item) {
		ref TraitModifierEntry entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, item.Trait, out bool existed);

		if (!existed) {
			entry = new(item);
		}
		else {
			entry.Add(item);
		}

		item.OnValueModified += OnModifiersUpdated;
	}

	public void AddRange(IEnumerable<TraitModifier> items) {
		HashSet<Trait> traits = [];
		foreach (TraitModifier item in items) {
			AddInternal(item);
			traits.Add(item.Trait);
		}
		foreach (Trait trait in traits) {
			OnModifiersUpdated?.Invoke(trait);
		}
	}

	public bool Remove(TraitModifier item) {
		bool wasRemoved = RemoveInternal(item);
		if (wasRemoved) OnModifiersUpdated?.Invoke(item.Trait);

		return wasRemoved;
	}
	public async Task<bool> RemoveProgressively(TraitModifier item, uint timeMilliseconds = 0, Func<float, float, float, float>? function = null) {
		bool wasRemoved = await RemoveProgressivelyInternal(item, timeMilliseconds, function);
		if (wasRemoved && timeMilliseconds == 0) OnModifiersUpdated?.Invoke(item.Trait);

		return wasRemoved;
	}

	private async Task<bool> RemoveProgressivelyInternal(TraitModifier item, uint timeMilliseconds = 0, Func<float, float, float, float>? function = null) {
		ref var entryRef = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, item.Trait);
		if (Unsafe.IsNullRef(ref entryRef)) return false;
		if (!entryRef.Contains(item)) return false;

		TraitModifierEntry entry = entryRef;

		if (timeMilliseconds > 0) {
			function ??= Mathf.Lerp;

			float start = item.Efficiency;
			float end = 0f;

			await AsyncUtils.WaitAndCall(timeMilliseconds, (elapsed) => {
				item.Efficiency = function(start, end, (float)elapsed / timeMilliseconds);
			});
		}

		if (!entry.Remove(item)) return false;

		item.OnValueModified -= OnModifiersUpdated;
		return true;
	}
	private bool RemoveInternal(TraitModifier item) {
		ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, item.Trait);
		if (Unsafe.IsNullRef(ref entry) || !entry.Remove(item))
			return false;

		item.OnValueModified -= OnModifiersUpdated;
		return true;
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

	public void Set(IEnumerable<TraitModifier> modifiers) {
		HashSet<TraitModifier> newModifiersSet = [.. modifiers];

		// Iterate over the current modifiers and collect those that need to be removed
		List<TraitModifier> currentModifiers = _dictionary.Values.SelectMany(e => e.List).ToList();
		foreach (TraitModifier currentModifier in currentModifiers) {
			if (!newModifiersSet.Contains(currentModifier)) {
				Remove(currentModifier);
			}
		}

		// Iterate over the new modifiers and add those that are not already in the collection
		foreach (TraitModifier newModifier in newModifiersSet) {
			if (!Contains(newModifier)) {
				Add(newModifier);
			}
		}
	}

	public void Clear() {
		var entries = _dictionary.Values;

		foreach (TraitModifierEntry entry in entries) {
			foreach (TraitModifier modifier in entry.List) {
				Remove(modifier);
			}
		}
	}

	public bool Contains(TraitModifier item) =>
		_dictionary.TryGetValue(item.Trait, out TraitModifierEntry entry) &&
		entry.Contains(item);

	public void CopyTo(TraitModifier[] array, int arrayIndex) =>
		_dictionary.Values
			.SelectMany(e => e.List)
			.ToList()
			.CopyTo(array, arrayIndex);

	public IEnumerator<TraitModifier> GetEnumerator() =>
		_dictionary.Values
		.SelectMany(e => e.List)
		.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



	private readonly struct TraitModifierEntry : ICollection<TraitModifier> {
		private readonly List<TraitModifier> _stacking = [];
		private readonly List<TraitModifier> _nonStacking = [];


		public readonly List<TraitModifier> List =>
			[.. _nonStacking, .. _stacking];

		public int Count => _nonStacking.Count + _stacking.Count;
		public bool IsReadOnly => false;


		public TraitModifierEntry() { }
		public TraitModifierEntry(TraitModifier modifier) : this() {
			Add(modifier);
		}
		public TraitModifierEntry(IEnumerable<TraitModifier> modifiers) : this() {
			foreach (TraitModifier modifier in modifiers) {
				Add(modifier);
			}
		}


		public void Add(TraitModifier item) {
			if (item.IsStacking) {
				_stacking.Add(item);
			}
			else {
				_nonStacking.Add(item);
			}
		}

		public bool Remove(TraitModifier item) =>
			_stacking.Remove(item) ||
			_nonStacking.Remove(item);

		public void Clear() {
			_stacking.Clear();
			_nonStacking.Clear();
		}


		public void CopyTo(TraitModifier[] array, int arrayIndex) =>
			List.CopyTo(array, arrayIndex);


		public readonly float ApplyTo(float baseValue) {
			float result = baseValue;

			foreach (ITraitModifier trait in _nonStacking) {
				result = trait.ApplyTo(result);
			}

			foreach (ITraitModifier trait in _stacking) {
				result += trait.ApplyTo(baseValue) - baseValue;
			}

			return result;
		}


		public readonly bool Contains(TraitModifier item) =>
			_stacking.Contains(item) || _nonStacking.Contains(item);

		public readonly IEnumerator<TraitModifier> GetEnumerator() =>
			List.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}