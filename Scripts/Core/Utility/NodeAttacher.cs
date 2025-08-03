namespace Seven.Boundless;

using Godot;
using System;

[Tool]
[GlobalClass]
public partial class NodeAttacher : Node {
	[Export] public Node3D? Follower;
	[Export] public Node3D? TargetNode;
	public Func<Node3D, Transform3D> TransformFunction = node => node.GlobalTransform;

	public void Execute() {
		if (Follower is null) return;

		Follower.GlobalTransform = TransformFunction(TargetNode ?? Follower);
	}

	public override void _Ready() {
		base._Ready();
		Execute();
	}
	public override void _Process(double delta) {
		base._Process(delta);
		Execute();
	}
}