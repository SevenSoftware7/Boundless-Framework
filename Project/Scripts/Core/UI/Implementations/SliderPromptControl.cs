namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class SliderPromptControl : PromptControl {
	[Export] private bool shrinkInView;
	[Export] private RichTextLabel Label = null!;
	[Export] private TextureRect Key = null!;

	private float velocity;


	public override void SetText(string text) {
		Label.Text = text;
	}
	public override void SetKey(Texture2D image) {
		Key.Texture = image;
	}


	public override void _Process(double delta) {
		base._Process(delta);

		float floatDelta = (float)delta;

		Vector2 targetPosition = Position with {
			X = Enabled ? 0 : - Size.X
		};
		Position = Position.Lerp(targetPosition, 15f * floatDelta);

		Vector2 targetScale = Scale with {
			Y = ! Enabled && (shrinkInView || Position.IsEqualApprox(targetPosition)) ? Mathf.Epsilon : 1f
		};
		Scale = Scale.Lerp(targetScale, 25f * floatDelta);


		bool wasNotVisible = ! Visible;
		Visible = ! Mathf.IsEqualApprox(Scale.Y, Mathf.Epsilon);
		if (Visible && wasNotVisible) {
			GetParent()?.MoveChild(this, 0);
		}

		UpdateMinimumSize();
	}

	public override Vector2 _GetMinimumSize() {
		return Size * Scale;
	}
}