namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class Companion : Node3D, IUIObject, IPlayerHandler, ICustomizable {
	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;

	public virtual IUIObject UIObject => this;
	public virtual ICustomizable[] Customizables => [];
	public virtual ICustomization[] Customizations => [];


	[ExportGroup("Costume")]
	[Export] public CompanionCostume? Costume {
		get => _costume;
		private set {
			if (this.IsInitializationSetterCall()) {
				_costume = value;
				return;
			}

			SetCostume(value);
		}
	}
	private CompanionCostume? _costume;

	[Export] protected Model? Model { get; private set; }


	[Signal] public delegate void CostumeChangedEventHandler(CompanionCostume? newCostume, CompanionCostume? oldCostume);



	public Companion() : base() { }
	public Companion(CompanionCostume? costume) : this() {
		SetCostume(costume);
		Name = $"{nameof(Companion)} - {DisplayName}";
	}



	public void SetCostume(CompanionCostume? newCostume) {
		CompanionCostume? oldCostume = _costume;
		if (newCostume == oldCostume) return;

		_costume = newCostume;

		Load(true);

		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);
	}



	public virtual void HandlePlayer(Player player) { }


	protected void Load(bool forceReload = false) {
		if (Model is not null && !forceReload) return;

		Model?.QueueFree();
		Model = Costume?.Instantiate()?.SetParentToSceneInstance(this);
	}
	protected void Unload() {
		Model?.QueueFree();
		Model = null;
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationSceneInstantiated:
				Callable.From(() => Load()).CallDeferred();
				break;
		}
	}
}