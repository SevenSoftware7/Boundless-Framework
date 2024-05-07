using Godot;
using System;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class PromptControl : PanelContainer {
	[Export] public bool IsVisible;
	[Export] public RichTextLabel Label { get; private set; } = null!;

	public override void _Process(double delta) {
		base._Process(delta);

		float X = IsVisible ? 0f : - Size.X;
		Visible = Position.X != - Size.X;

		if (Position.X == X)
			return;


		Position = Position with {
			X = Mathf.Lerp(Position.X, X, 15f * (float)delta)
		};
	}
}
