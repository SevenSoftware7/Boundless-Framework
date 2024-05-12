using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class PromptControl : PanelContainer {
	[Export] public bool IsVisible;
	[Export] private RichTextLabel Label = null!;
	[Export] private TextureRect Prompt = null!;


	public void SetText(string text) {
		Label.Text = text;
	}
	public void SetPrompt(Texture2D image) {
		Prompt.Texture = image;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		Vector2 target = GlobalPosition with {
			X = IsVisible ? 0 : - Size.X
		};
		float speed = 15f * (float)delta;


		if (! IsVisible && GlobalPosition.IsEqualApprox(target)) {
			Scale = Scale with {
				Y = Mathf.Lerp(Scale.Y, Mathf.Epsilon, speed)
			};
		}
		else {
			Scale = Vector2.One;
		}

		GlobalPosition = GlobalPosition.Lerp(target, speed);

		// Visible = ! Mathf.IsEqualApprox(Scale.Y, Mathf.Epsilon); // This causes the scale and Position to reset for some reason.
	}
}
