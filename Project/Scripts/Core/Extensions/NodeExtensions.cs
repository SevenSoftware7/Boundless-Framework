namespace SevenDev.Utility;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using LandlessSkies.Core;

public static class NodeExtensions {

	public static bool IsEnabled(this Node node) => node.ProcessMode == Node.ProcessModeEnum.Inherit || node.ProcessMode == Node.ProcessModeEnum.Always;

	public static void Enable(this Node node) {
		if (node.ProcessMode == Node.ProcessModeEnum.Disabled) {
			node.ProcessMode = Node.ProcessModeEnum.Inherit;
		}
	}
	public static void Disable(this Node node) {
		node.ProcessMode = Node.ProcessModeEnum.Disabled;
	}

	public static void SetEnabled(this Node node, bool enabled) {
		if (enabled) {
			node.Enable();
		}
		else {
			node.Disable();
		}
	}

	public static void Enable(this Node2D node) {
		node.Visible = true;
		if (node.ProcessMode == Node.ProcessModeEnum.Disabled) {
			node.ProcessMode = Node.ProcessModeEnum.Inherit;
		}
	}
	public static void Disable(this Node2D node) {
		node.Visible = false;
		node.ProcessMode = Node.ProcessModeEnum.Disabled;
	}

	public static void SetEnabled(this Node2D node, bool enabled) {
		if (enabled) {
			node.Enable();
		}
		else {
			node.Disable();
		}
	}

	public static void Enable(this Node3D node) {
		node.Visible = true;
		if (node.ProcessMode == Node.ProcessModeEnum.Disabled) {
			node.ProcessMode = Node.ProcessModeEnum.Inherit;
		}
	}
	public static void Disable(this Node3D node) {
		node.Visible = false;
		node.ProcessMode = Node.ProcessModeEnum.Disabled;
	}

	public static void SetEnabled(this Node3D node, bool enabled) {
		if (enabled) {
			node.Enable();
		}
		else {
			node.Disable();
		}
	}

	public static void Enable(this Control node) {
		node.Visible = true;
		if (node.ProcessMode == Node.ProcessModeEnum.Disabled) {
			node.ProcessMode = Node.ProcessModeEnum.Inherit;
		}
	}
	public static void Disable(this Control node) {
		node.Visible = false;
		node.ProcessMode = Node.ProcessModeEnum.Disabled;
	}

	public static void SetEnabled(this Control node, bool enabled) {
		if (enabled) {
			node.Enable();
		}
		else {
			node.Disable();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T SafeReparentEditor<T>(this T child, Node? newParent, bool keepGlobalTransform = true) where T : Node {
		child.SafeReparentTo(newParent, keepGlobalTransform);

		if (! Engine.IsEditorHint()) return child;
		if (child.Owner == newParent?.Owner) return child;

		Reown(child, newParent?.Owner ?? newParent);

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
		child.SafeReparentTo(newParent, keepGlobalTransform);
		child.Owner = newParent?.Owner ?? newParent;
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T SafeReparentTo<T>(this T child, Node? newParent, bool keepGlobalTransform = true) where T : Node {
		if (child.GetParent() == newParent) return child;

		if (! child.IsInsideTree()) {
			child.Unparent();
		}
		if (child.GetParent() is null) {
			newParent?.AddChild(child);
			return child;
		}

		if (newParent is not null) {
			child.Reparent(newParent, keepGlobalTransform);
		}
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T AddChildAndSetOwner<T>(this T obj, Node child, bool forceReadableName = false) where T : Node {
		obj.AddChild(child, forceReadableName);
		child.Owner = obj.Owner ?? obj;
		return obj;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T AddSceneInstanceChild<T>(this T obj, Node child, bool forceReadableName = false) where T : Node {
		obj.AddChild(child, forceReadableName);
		child.Owner = obj.SceneFilePath.Length != 0 ? obj : obj.Owner ?? obj;
		return obj;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TNode ParentTo<TNode>(this TNode child, Node parent, bool forceReadableName = false) where TNode : Node {
		parent.AddChild(child, forceReadableName);
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TNode SetOwnerAndParent<TNode>(this TNode child, Node parent, bool forceReadableName = false) where TNode : Node {
		parent.AddChildAndSetOwner(child, forceReadableName);
		return child;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TNode SetParentToSceneInstance<TNode>(this TNode child, Node parent, bool forceReadableName = false) where TNode : Node {
		parent.AddSceneInstanceChild(child, forceReadableName);
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
		if (! obj.HasNode(nodePath))
			return false;

		if (obj.GetNodeOrNull(nodePath) is T tNode) {
			node = tNode;
			return true;
		}

		return false;
	}

	public static void ReconnectSignal<T>(this T node, StringName signalName, Callable method, GodotObject.ConnectFlags flags) where T : Node {
		if (node is null) return;

		if (node.IsConnected(signalName, method)) {
			node.Disconnect(signalName, method);
		}

		node?.Connect(signalName, method, (uint)flags);
	}

	public static void SwapSignalEmitter<T>(ref T? emitter, T? newEmitter, StringName signalName, Callable method, GodotObject.ConnectFlags flags = 0) where T : Node {
		if (emitter is not null && emitter.IsConnected(signalName, method)) {
			emitter.Disconnect(signalName, method);
		}

		emitter = newEmitter;

		if (emitter is not null && ! emitter.IsConnected(signalName, method)) { // IsConnected() sometimes does not work, not my fault https://github.com/godotengine/godot/issues/76690
			emitter.Connect(signalName, method, (uint)flags);
		}
	}

	public static void PropagateActionToChildren<T>(this Node parent, Action<T>? action, bool parentFirst = false) {
		if (parentFirst && parent is T tParent1) {
			action?.Invoke(tParent1);
		}

		foreach (T child in parent.GetChildren().OfType<T>()) {
			action?.Invoke(child);
		}

		if (! parentFirst && parent is T tParent2) {
			action?.Invoke(tParent2);
		}
	}

	public static void PropagateAction<T>(this Node parent, Action<T>? action, bool parentFirst = false) {
		if (parentFirst && parent is T tParent1) {
			action?.Invoke(tParent1);
		}

		foreach (Node child in parent.GetChildren()) {
			child.PropagateAction(action, parentFirst);
		}

		if (! parentFirst && parent is T tParent2) {
			action?.Invoke(tParent2);
		}
	}


	public static void MakeLocal(this Node node, Node owner) {
		node.SceneFilePath = string.Empty;
		node.Owner = owner;
		foreach (Node childNode in node.GetChildren()) {
			MakeLocal(childNode, owner);
		}
	}
}