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
}