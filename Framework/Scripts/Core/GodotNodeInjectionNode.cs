namespace Seven.Boundless;

using System;
using System.Collections.Generic;
using Seven.Boundless.Injection;

/// <summary>
/// Represents a Godot node in an injection hierarchy.
/// </summary>
/// <seealso cref="IInjectionNode"/>
/// <seealso cref="Godot.Node"/>
public readonly struct GodotNodeInjectionNode : IInjectionNode {
	public readonly Godot.Node GodotNode;


	public GodotNodeInjectionNode(Godot.Node godotNode) {
		ArgumentNullException.ThrowIfNull(godotNode);
		GodotNode = godotNode;
	}


	/// <summary>
	/// Returns the underlying Godot.Node of the current node.
	/// </summary>
	public readonly Godot.Node UnderlyingObject => GodotNode;
	readonly object IInjectionNode.UnderlyingObject => UnderlyingObject;

	/// <inheritdoc/>
	public readonly IInjectionNode? Parent => GodotNode.GetParent() is Godot.Node parent ? new GodotNodeInjectionNode(parent) : null;

	/// <inheritdoc/>
	public readonly IEnumerable<IInjectionNode> Children {
		get {
			foreach (Godot.Node child in GodotNode.GetChildren()) {
				yield return new GodotNodeInjectionNode(child);
			}
		}
	}

	/// <inheritdoc/>
	public readonly bool IsTreeReady =>
		(GodotNode.GetParent()?.IsNodeReady() ?? false) ||
		(GodotNode.GetTree()?.Root?.IsNodeReady() ?? false);

	/// <inheritdoc/>
	public readonly string NodeName => GodotNode.Name;
}