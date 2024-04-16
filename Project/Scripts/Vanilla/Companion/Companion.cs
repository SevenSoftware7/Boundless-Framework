namespace LandlessSkies.Vanilla;

using LandlessSkies.Core;
using Godot;
using System;
using SevenGame.Utility;

[Tool]
[GlobalClass]
public partial class Companion : Loadable3D, IInputReader, ICustomizable {
	public override bool IsLoaded {
		get => CompanionModel?.IsLoaded ?? false;
		set => CompanionModel?.AsILoadable().SetLoaded(value);
	}


	[Export] public CompanionData Data {
		get => _data;
		private set {
			_data = value;

			if (this.IsInitializationSetterCall())
				return;
			if (Costume is not null)
				return;

			SetCostume(_data?.BaseCostume);
		}
	}
	private CompanionData _data = null!;

	[ExportGroup("Costume")]
	[Export] protected Model? CompanionModel {
		get => _companionModel;
		private set => _companionModel = value;
	}
	private Model? _companionModel = null!;

	[Export] public CompanionCostume? Costume {
		get => CompanionModel?.Costume as CompanionCostume;
		set {
			if (this.IsInitializationSetterCall())
				return;
			SetCostume(value);
		}
	}

	public Basis CompanionRotation { get; private set; } = Basis.Identity;


	public virtual IUIObject UIObject => Data;
	public virtual ICustomizable[] Children => CompanionModel is Model model ? [model] : [];
	public virtual ICustomizationParameter[] Customizations => [];


	[Signal] public delegate void CostumeChangedEventHandler(CompanionCostume? newCostume, CompanionCostume? oldCostume);



	public Companion() : base() { }
	public Companion(CompanionData data, CompanionCostume? costume) : this() {
		ArgumentNullException.ThrowIfNull(data);

		_data = data;
		SetCostume(costume ?? data.BaseCostume);
		Name = $"{nameof(Companion)} - {Data.DisplayName}";
	}



	public void SetCostume(CompanionCostume? costume, bool forceLoad = false) {
		CompanionCostume? oldCostume = Costume;
		if (costume == oldCostume)
			return;

		new LoadableUpdater<Model>(ref _companionModel, () => costume?.Instantiate())
			.BeforeLoad(m => {
				// m.Inject(Skeleton);
				m.SafeReparentEditor(this);
			})
			.Execute();

		EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
	}


	public void RotateTowards(Basis target, double delta, float speed = 16f) {
		CompanionRotation = CompanionRotation.SafeSlerp(target, (float)delta * speed);

		RefreshRotation();
	}

	protected virtual void RefreshRotation() {
		Transform = Transform with {
			Basis = CompanionRotation,
		};
	}

	public virtual void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice) { }


	protected override bool LoadBehaviour() {
		if (!base.LoadBehaviour())
			return false;
		if (Data is null)
			return false;

		// CompanionModel.Inject(Skeleton);
		CompanionModel?.AsILoadable().Load();

		RefreshRotation();

		return true;
	}
	protected override void UnloadBehaviour() {
		base.UnloadBehaviour();

		CompanionModel?.AsILoadable().Unload();
	}

	protected override void EnableBehaviour() {
		base.EnableBehaviour();
		CompanionModel?.AsIEnablable().Enable();
	}
	protected override void DisableBehaviour() {
		base.DisableBehaviour();
		CompanionModel?.AsIEnablable().Disable();
	}
}