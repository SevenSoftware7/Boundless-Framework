namespace SevenDev.Utility;

using System.Collections.Generic;
using Godot;

public static class PackedSceneExtensions {
	public static void PackWithSubnodes(this PackedScene scene, Node path) {
		Dictionary<Node, Node> originalOwners = [];
		ReownChildren(path);

		void ReownChildren(Node node, uint layer = 0) {
			foreach (Node item in node.GetChildren()) {
				originalOwners[item] = item.Owner;

				item.Owner = path;
				ReownChildren(item, layer + 1);
			}
		}

		scene.Pack(path);
		foreach (KeyValuePair<Node, Node> original in originalOwners) {
			original.Key.Owner = original.Value;
		}
	}

	public static T GetNodeProperty<[MustBeVariant] T>(this PackedScene scene, StringName propertyName, int nodeIndex = 0) {
		return scene.GetNodeProperty(propertyName, nodeIndex).As<T>();
	}
	public static T GetNodePropertyRecursive<[MustBeVariant] T>(this PackedScene scene, StringName propertyName) {
		return scene.GetNodePropertyRecursive(propertyName).As<T>();
	}
	public static Variant GetNodeProperty(this PackedScene scene, StringName propertyName, int nodeIndex = 0) {
		SceneState state = scene.GetState();

		for (int propIndex = 0; propIndex < state.GetNodePropertyCount(nodeIndex); propIndex++) {
			StringName propName = state.GetNodePropertyName(nodeIndex, propIndex);

			if (propName == propertyName) return state.GetNodePropertyValue(nodeIndex, propIndex);
		}

		return default;
	}
	public static Variant GetNodePropertyRecursive(this PackedScene scene, StringName propertyName) {
		SceneState state = scene.GetState();
		for (int nodeIndex = 0; nodeIndex < state.GetNodeCount(); nodeIndex++) {
			for (int propIndex = 0; propIndex < state.GetNodePropertyCount(nodeIndex); propIndex++) {
				StringName propName = state.GetNodePropertyName(nodeIndex, propIndex);

				if (propName == propertyName) return state.GetNodePropertyValue(nodeIndex, propIndex);
			}

		}

		return default;
	}
}