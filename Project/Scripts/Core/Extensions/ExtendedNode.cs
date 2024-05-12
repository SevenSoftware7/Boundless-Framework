namespace LandlessSkies.Core;

using Godot;

public abstract partial class ExtendedNode : Node {
	public virtual void _Predelete() { }
	public virtual void _Disabled() { }
	public virtual void _Enabled() { }
	public virtual void _Parented() { }
	public virtual void _Unparented() { }
	public virtual void _PathRenamed() { }
	public virtual void _ChildOrderChanged() { }

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((long)what) {
			case NotificationPredelete:
				_Predelete();
				break;
			case NotificationDisabled:
				_Disabled();
				break;
			case NotificationEnabled:
				_Enabled();
				break;
			case NotificationParented:
				_Parented();
				break;
			case NotificationUnparented:
				_Unparented();
				break;
			case NotificationPathRenamed:
				_PathRenamed();
				break;
			case NotificationChildOrderChanged:
				_ChildOrderChanged();
				break;
		}
	}
}