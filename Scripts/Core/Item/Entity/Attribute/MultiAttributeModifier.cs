// namespace LandlessSkies.Core;

// using System.Collections.Generic;
// using System.Linq;


// public struct MultiAttributeModifier(IEnumerable<IAttributeModifier>? attributes = null) : IAttributeModifier {
// 	public IEnumerable<IAttributeModifier> Attributes { get; private set; } = FlattenAttributes(attributes?.OfType<IAttributeModifier>() ?? []);
// 	public readonly bool IsStacking => true;

// 	private static IEnumerable<IAttributeModifier> FlattenAttributes(IEnumerable<IAttributeModifier> attributes) {
// 		foreach (IAttributeModifier attribute in attributes) {
// 			if (attribute is MultiAttributeModifier multi) {
// 				foreach (IAttributeModifier contained in multi.Attributes) yield return contained;
// 			}
// 			yield return attribute;
// 		}
// 	}


// 	public readonly float ApplyTo(float baseValue) {
// 	}
// }