using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;

namespace LandlessSkies.Core;

public sealed class AttributeModifierCollection : ICollection<AttributeModifier> {
	private readonly Dictionary<EntityAttribute, AttributeModifierEntry> _dictionary = [];

	public int Count => _dictionary.Values.Sum(e => e.Count);
	public bool IsReadOnly => false;

	public event Action<AttributeModifier>? OnModifierAdded;
	public event Action<AttributeModifier>? OnModifierRemoved;


	public float ApplyTo(EntityAttribute target, float baseValue) {
		if (!_dictionary.TryGetValue(target, out AttributeModifierEntry entry))
			return baseValue;

		return entry.ApplyTo(baseValue);
	}

	public void Add(AttributeModifier item) {
		ref AttributeModifierEntry entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, item.Target, out bool existed);

		if (!existed) {
			entry = new(item);
		}
		else {
			entry.Add(item);
		}

		OnModifierAdded?.Invoke(item);
	}

	public void AddRange(IEnumerable<AttributeModifier> items) {
		foreach (AttributeModifier item in items) {
			Add(item);
		}
	}

	public bool Remove(AttributeModifier item) {
		GD.Print($"remove {item}");

		ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, item.Target);
		if (Unsafe.IsNullRef(ref entry) || !entry.Remove(item))
			return false;

		OnModifierRemoved?.Invoke(item);
		return true;
	}

	public void RemoveRange(IEnumerable<AttributeModifier> items) {
		foreach (AttributeModifier item in items) {
			Remove(item);
		}
	}

	public void Set(IEnumerable<AttributeModifier> modifiers) {
		HashSet<AttributeModifier> newModifiersSet = new(modifiers);

		// Iterate over the current modifiers and collect those that need to be removed
		List<AttributeModifier> currentModifiers = _dictionary.Values.SelectMany(e => e.List).ToList();
		foreach (AttributeModifier currentModifier in currentModifiers) {
			if (!newModifiersSet.Contains(currentModifier)) {
				Remove(currentModifier);
			}
		}

		// Iterate over the new modifiers and add those that are not already in the collection
		foreach (AttributeModifier newModifier in newModifiersSet) {
			if (!Contains(newModifier)) {
				Add(newModifier);
			}
		}
	}

	public void Clear() {
		var entries = _dictionary.Values;

		foreach (AttributeModifierEntry entry in entries) {
			foreach (AttributeModifier modifier in entry.List) {
				Remove(modifier);
			}
		}
	}

	public bool Contains(AttributeModifier item) =>
		_dictionary.TryGetValue(item.Target, out AttributeModifierEntry entry) &&
		entry.Contains(item);
	public bool ContainsAll(IEnumerable<AttributeModifier> items) {
		foreach (AttributeModifier item in items) {
			if (!Contains(item)) {
				GD.Print(item);
				return false;
			}
		}
		return true;
	}

	public void CopyTo(AttributeModifier[] array, int arrayIndex) =>
		_dictionary.Values
			.SelectMany(e => e.List)
			.ToList()
			.CopyTo(array, arrayIndex);

	public IEnumerator<AttributeModifier> GetEnumerator() =>
		_dictionary.Values
		.SelectMany(e => e.List)
		.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



	private readonly struct AttributeModifierEntry : ICollection<AttributeModifier> {
		private readonly List<AttributeModifier> _stacking = [];
		private readonly List<AttributeModifier> _nonStacking = [];


		public readonly List<AttributeModifier> List =>
			[.. _nonStacking, .. _stacking];

		public int Count => _nonStacking.Count + _stacking.Count;
		public bool IsReadOnly => false;


		public AttributeModifierEntry() { }
		public AttributeModifierEntry(AttributeModifier modifier) : this() {
			Add(modifier);
		}
		public AttributeModifierEntry(IEnumerable<AttributeModifier> modifiers) : this() {
			foreach (AttributeModifier modifier in modifiers) {
				Add(modifier);
			}
		}


		public void Add(AttributeModifier item) {
			if (item.IsStacking) {
				_stacking.Add(item);
			}
			else {
				_nonStacking.Add(item);
			}
		}

		public bool Remove(AttributeModifier item) =>
			_stacking.Remove(item) ||
			_nonStacking.Remove(item);

		public void Clear() {
			_stacking.Clear();
			_nonStacking.Clear();
		}


		public void CopyTo(AttributeModifier[] array, int arrayIndex) =>
			List.CopyTo(array, arrayIndex);


		public readonly float ApplyTo(float baseValue) {
			float result = baseValue;

			foreach (IAttributeModifier attribute in _nonStacking) {
				result = attribute.ApplyTo(result);
			}

			foreach (IAttributeModifier attribute in _stacking) {
				result += attribute.ApplyTo(baseValue) - baseValue;
			}

			return result;
		}


		public readonly bool Contains(AttributeModifier item) =>
			_stacking.Contains(item) || _nonStacking.Contains(item);

		public readonly IEnumerator<AttributeModifier> GetEnumerator() =>
			List.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}