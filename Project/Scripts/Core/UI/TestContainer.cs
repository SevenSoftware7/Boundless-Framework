namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;


[Tool]
[GlobalClass]
public partial class TestContainer : Container {
	public override Vector2 _GetMinimumSize() {
		int separation = /* GetThemeConstant("separation") */2; // TODO
		int childrenCount = GetChildCount();

		Vector2 size = Vector2.Zero;
		for (int i = 0; i < childrenCount; i++) {
			if (GetChild(i) is not Control control)
				continue;

			if (! control.Visible)
				continue;

			control.Position = new Vector2(control.Position.X, size.Y);
			Vector2 controlMinSize = control.GetCombinedMinimumSize();
			size = size with {
				X = Mathf.Max(size.X, controlMinSize.X),
				Y = size.Y + controlMinSize.Y
			};

			if (i != childrenCount - 1) {
				size.Y += separation * control.Scale.Y;
			}
		}

		return size;
	}
}
