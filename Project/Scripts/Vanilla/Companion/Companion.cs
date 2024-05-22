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
		private set => SetCostume(value);
	}
	private CompanionCostume? _costume;

	protected Model? Model { get; private set; }


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
		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);

		if (Engine.IsEditorHint()) {
			Callable.From<bool>(Load).CallDeferred(true);
		} else {
			Load(true);
		}
	}



	public virtual void HandlePlayer(Player player) { }
	public virtual void DisavowPlayer(Player player) { }


	protected void Load(bool forceReload = false) {
		if (Model is not null && !forceReload) return;

		Model?.QueueFree();
		Model = Costume?.Instantiate()?.ParentTo(this);
	}
	protected void Unload() {
		Model?.QueueFree();
		Model = null;
	}

	public override void _ExitTree() {
		base._ExitTree();
		Unload();
	}
	public override void _EnterTree() {
		base._EnterTree();
		Load();
	}
}