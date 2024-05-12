using System.Runtime.CompilerServices;
using Godot;

namespace LandlessSkies.Core;


public static class NodeExtensions {
#if TOOLS
	private static readonly ulong buildFrame;



	static NodeExtensions() {
		buildFrame = Engine.GetProcessFrames();
	}
#endif


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool JustBuilt(this Node node) =>
#if TOOLS
		node.IsNodeReady() && Engine.GetProcessFrames() == buildFrame;
#else
		false;
#endif


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInitializationSetterCall(this Node node) =>
#if TOOLS
		!node.IsNodeReady() || Engine.GetProcessFrames() == buildFrame;
#else
		false;
#endif


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEditorEnterTree(this Node node) =>
#if TOOLS
		node.IsNodeReady() || Engine.GetProcessFrames() == buildFrame; // TODO: Make this return true when switching scene
#else
		false;
#endif


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEditorExitTree(this Node node) =>
#if TOOLS
		!node.IsNodeReady() || Engine.GetProcessFrames() == buildFrame; // TODO: Make this return true when switching scene
#else
		false;
#endif



	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T SafeReparentEditor<T>(this T child, Node? newParent, bool keepGlobalTransform = true) where T : Node {
		child.SafeReparent(newParent, keepGlobalTransform);

		if (!Engine.IsEditorHint())
			return child;

		if (child.Owner == newParent?.Owner)
			return child;

		Reown(child, newParent?.Owner);

		static void Reown(Node childNode, Node? newOwner) {
			childNode.Owner = newOwner;
			foreach (Node child in childNode.GetChildren()) {
				Reown(child, newOwner);
			}
		}

		return child;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T SafeReparentAndSetOwner<T>(this T child, Node? newParent, bool keepGlobalTransform = true) where T : Node {
		child.SafeReparent(newParent, keepGlobalTransform);
		child.Owner = newParent?.Owner;
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T SafeReparent<T>(this T child, Node? newParent, bool keepGlobalTransform = true) where T : Node {
		if (!child.IsInsideTree()) {
			child.Unparent();
		}
		if (child.GetParent() is not Node parent) {
			newParent?.AddChild(child);
			return child;
		}
		if (parent == newParent)
			return child;

		if (newParent is not null) {
			child.Reparent(newParent, keepGlobalTransform);
		}
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T AddChildAndSetOwner<T>(this T obj, Node child, bool forceReadableName = false) where T : Node {
		obj.AddChild(child, forceReadableName);
		child.Owner = obj.Owner;
		return obj;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TNode ParentTo<TNode>(this TNode child, Node parent) where TNode : Node {
		parent.AddChild(child);
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TNode SetOwnerAndParent<TNode>(this TNode child, Node parent) where TNode : Node {
		parent.AddChildAndSetOwner(child);
		return child;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TNode Unparent<TNode>(this TNode child) where TNode : Node {
		child.Owner = null;
		child.GetParent()?.RemoveChild(child);
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UnparentAndQueueFree(this Node obj) {
		obj.QueueFree();
		obj.GetParent()?.RemoveChild(obj);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? GetNodeByTypeName<T>(this Node obj) where T : Node {
		return obj.GetNodeOrNull<T>(typeof(T).Name);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetNode<T>(this Node obj, NodePath nodePath, out T node) where T : class {
		node = default!;
		if (!obj.HasNode(nodePath))
			return false;

		if (obj.GetNodeOrNull(nodePath) is T tNode) {
			node = tNode;
			return true;
		}

		return false;
	}

	public static void MakeLocal(this Node node, Node owner) {
		node.SceneFilePath = string.Empty;
		node.Owner = owner;
		foreach (Node childNode in node.GetChildren()) {
			MakeLocal(childNode, owner);
		}
	}
}