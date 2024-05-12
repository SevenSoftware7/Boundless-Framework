namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using Godot;
using System;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class Companion : Loadable3D, IUIObject, IInputHandler, ICustomizable {
	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;

	public virtual IUIObject UIObject => this;
	public virtual ICustomizable[] Children => Model is Model model ? [model] : [];
	public virtual ICustomizationParameter[] Customizations => [];

	public override bool IsLoaded {
		get => _isLoaded;
		set => this.BackingFieldLoadUnload(ref _isLoaded, value);
	}
	private bool _isLoaded = false;


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

	[Export] protected Model? Model {
		get => _model;
		private set => _model = value;
	}
	private Model? _model;

	public Basis CompanionRotation { get; private set; } = Basis.Identity;


	[Signal] public delegate void CostumeChangedEventHandler(CompanionCostume? newCostume, CompanionCostume? oldCostume);



	public Companion() : base() { }
	public Companion(CompanionCostume? costume) : this() {
		SetCostume(costume);
		Name = $"{nameof(Companion)} - {DisplayName}";
	}



	public void SetCostume(CompanionCostume? newCostume, bool forceLoad = false) {
		CompanionCostume? oldCostume = _costume;
		if (newCostume == oldCostume)
			return;

		_costume = newCostume;

		AsILoadable().Reload(forceLoad);

		EmitSignal(SignalName.CostumeChanged, newCostume!, oldCostume!);
	}



	public virtual void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice, HudManager hud) { }


	protected override bool LoadBehaviour() {
		new LoadableUpdater<Model>(ref _model, () => Costume?.Instantiate())
			.BeforeLoad(m => {
				m.SafeReparentEditor(this);
				m.AsIEnablable().EnableDisable(IsEnabled);
			})
			.Execute();

		_isLoaded = true;

		return true;
	}
	protected override void UnloadBehaviour() {
		new LoadableDestructor<Model>(ref _model)
			.AfterUnload(w => w.QueueFree())
			.Execute();

		_isLoaded = false;
	}

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		Model?.AsIEnablable().Enable();
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		Model?.AsIEnablable().Disable();
	}


	public override void _Parented() {
		base._Parented();
		Callable.From(() => Model?.SafeReparentAndSetOwner(this)).CallDeferred();
	}
}