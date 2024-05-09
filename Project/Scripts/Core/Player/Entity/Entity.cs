namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenGame.Utility;

[Tool]
[GlobalClass]
public partial class Entity : LoadableCharacterBody3D, IInputReader, IUIObject {
	[Export] public Godot.Collections.Array<AttributeModifier> Attributes { get; private set; } = [];
	public EntityAction? CurrentAction { get; private set; }
	public EntityBehaviour? CurrentBehaviour { get; private set; }

	public override bool IsLoaded {
		get => _isLoaded;
		set => this.BackingFieldLoadUnload(ref _isLoaded, value);
	}
	private bool _isLoaded = false;


	private readonly List<Vector3> lastStandableSurfaces = [];

	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;
	[Export] public string DisplayName { get; private set; } = string.Empty;
	[Export] public Handedness Handedness { get; private set; }
	[Export] public CharacterStats Stats { get; private set; } = new();


	[ExportGroup("Costume")]
	[Export] public CharacterCostume? Costume {
		get => _costume;
		private set {
			if (this.IsInitializationSetterCall()) {
				_costume = value;
				return;
			}

			SetCostume(value);
		}
	}
	private CharacterCostume? _costume;

	[Export] protected Model? Model {
		get => _model;
		private set => _model = value;
	}
	private Model? _model;


	[ExportGroup("Weapon")]
	[Export] public Weapon? Weapon {
		get => _weapon;
		set {
			if (this.IsInitializationSetterCall()) {
				_weapon = value;
				return;
			}

			_weapon?.Inject(null);
			_weapon = value;
			if (_weapon is not null) {
				_weapon.Inject(this);
				_weapon.SetParentSkeleton(Skeleton);
				_weapon.SetHandedness(Handedness);
				_weapon.Name = PropertyName.Weapon;
			}
		}
	}
	private Weapon? _weapon;


	[ExportGroup("Dependencies")]
	[Export] public AnimationPlayer? AnimationPlayer {
		get => _animationPlayer;
		private set {
			_animationPlayer = value;

			if (this.IsInitializationSetterCall())
				return;

			if (_animationPlayer is not null) {
				_animationPlayer.RootNode = _animationPlayer.GetPathTo(this);
			}
		}
	}
	private AnimationPlayer? _animationPlayer;

	[Export] public Skeleton3D? Skeleton { get; private set; }

	[Export] public Health? Health {
		get => _health;
		set {
			if (_health == value) return;
			if (this.IsInitializationSetterCall()) {
				_health = value;
				return;
			}

			Callable onKill = new(this, MethodName.OnKill);
			_health?.Disconnect(Health.SignalName.Death, onKill);

			_health = value;
			_health?.Connect(Health.SignalName.Death, onKill, (uint)ConnectFlags.Persist);
		}
	}
	private Health? _health;



	[ExportGroup("Movement")]
	[Export] public Vector3 Inertia = Vector3.Zero;
	[Export] public Vector3 Movement = Vector3.Zero;




	/// <summary>
	/// The forward direction in absolute space of the Entity.
	/// </summary>
	/// <remarks>
	/// Editing this value also changes <see cref="RelativeForward"/> to match.
	/// </remarks>
	[Export] public Vector3 AbsoluteForward {
		get => _absoluteForward;
		set => _absoluteForward = value.Normalized();
	}
	private Vector3 _absoluteForward = Vector3.Forward;


	/// <summary>
	/// The forward direction in relative space of the Entity.
	/// </summary>
	/// <remarks>
	/// Editing this value also changes <see cref="AbsoluteForward"/> to match.
	/// </remarks>
	[Export] public Vector3 RelativeForward {
		get => Transform.Basis.Inverse() * _absoluteForward;
		set => _absoluteForward = Transform.Basis * value;
	}




	public Entity() : base() {
		CollisionLayer = MathUtility.EntityCollisionLayer;

		RelativeForward = Vector3.Forward;
	}



	public void SetCostume(CharacterCostume? newCostume) {
		CharacterCostume? oldCostume = _costume;
		if (newCostume == oldCostume)
			return;

		_costume = newCostume;

		AsILoadable().Reload();

		EmitSignal(SignalName.LoadedUnloaded, newCostume!, oldCostume!);
	}

	public bool CanCancelAction() {
		return CurrentAction is null || CurrentAction.IsCancellable;
	}

	public void ExecuteAction(EntityActionInfo action, bool forceExecute = false) {
		if (!forceExecute && !CanCancelAction())
			return;

		CurrentAction?.QueueFree();
		CurrentAction = null;

		action.AfterExecute += () => CurrentAction = null;
		CurrentAction = action.Execute();

		Callable.From(() => CurrentAction.ParentTo(this)).CallDeferred();
	}


	public void SetBehaviour<TBehaviour>(TBehaviour? behaviour) where TBehaviour : EntityBehaviour {
		behaviour?.Start(CurrentBehaviour);
		CurrentBehaviour?.Stop();

		CurrentBehaviour = behaviour;
	}

	public void SetBehaviour<TBehaviour>(Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {
		SetBehaviour(typeof(TBehaviour).Name, creator);
	}

	public void SetBehaviour<TBehaviour>(NodePath behaviourPath, Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {
		TBehaviour? behaviour = GetNodeOrNull<TBehaviour>(behaviourPath);
		if (behaviour is null && creator is null)
			return;

		SetBehaviour(creator?.Invoke());
	}


	public MultiAttributeModifier GetModifiers(StringName attributeName) {
		return new(Attributes.Where(a => a.Name == attributeName));
	}


	public void SplitInertia(out Vector3 vertical, out Vector3 horizontal) {
		vertical = Inertia.Project(UpDirection);
		horizontal = Inertia - vertical;
	}


	private bool MoveStep(double delta) {
		if (Mathf.IsZeroApprox(Movement.LengthSquared()) || !IsOnFloor())
			return false;

		Vector3 movement = Movement * (float)delta;
		Vector3 destination = GlobalTransform.Origin + movement;

		KinematicCollision3D? stepObstacleCollision = MoveAndCollide(movement, true);

		float margin = Mathf.Epsilon;

		// // Down Step
		// if (stepObstacleCollision is null) {
		// 	margin += 4.5f;
		// }

		Vector3 sweepStart = destination;
		Vector3 sweepMotion = (Stats.StepHeight + margin) * -UpDirection;

		// Up Step
		if (stepObstacleCollision is not null) {
			sweepStart -= sweepMotion;
		}

		PhysicsTestMotionResult3D stepTestResult = new();
		bool findStep = PhysicsServer3D.BodyTestMotion(
			GetRid(),
			new() {
				From = GlobalTransform with { Origin = sweepStart },
				Motion = sweepMotion,
			},
			stepTestResult
		);

		if (!findStep)
			return false;

		if (stepObstacleCollision is not null && stepTestResult.GetColliderRid() != stepObstacleCollision.GetColliderRid())
			return false;


		Vector3 point = stepTestResult.GetCollisionPoint();

		Vector3 destinationHeight = destination.Project(UpDirection);
		Vector3 pointHeight = point.Project(UpDirection);

		float stepHeightSquared = destinationHeight.DistanceSquaredTo(pointHeight);
		if (stepHeightSquared >= sweepMotion.LengthSquared())
			return false;


		GlobalTransform = GlobalTransform with { Origin = destination - destinationHeight + pointHeight };
		// if (stepHeightSquared >= Mathf.Pow(stepHeight, 2f) / 4f) {
		// 	GD.Print("Step Found");
		// }

		return true;
	}

	private void Move(double delta) {
		if (MotionMode == MotionModeEnum.Grounded) {

			if (/* surfaceTimer.IsDone &&  */IsOnFloor() && GetPlatformVelocity().IsZeroApprox()) {
				lastStandableSurfaces.Add(GlobalPosition);
				if (lastStandableSurfaces.Count >= 20) {
					lastStandableSurfaces.RemoveRange(0, lastStandableSurfaces.Count - 20);
				}
				// surfaceTimer.Start();
			}

			SplitInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia);

			if (IsOnFloor()) {
				verticalInertia = verticalInertia.SlideOnFace(UpDirection);
			}
			if (IsOnCeiling()) {
				verticalInertia = verticalInertia.SlideOnFace(-UpDirection);
			}

			Inertia = horizontalInertia + verticalInertia;

			if (MoveStep(delta)) {
				Movement = Vector3.Zero;
			}
		}

		Velocity = Inertia + Movement;
		MoveAndSlide();
	}

	protected override bool LoadBehaviour() {
		if (!base.LoadBehaviour())
			return false;

		new LoadableUpdater<Model>(ref _model, () => Costume?.Instantiate())
			.BeforeLoad(m => {
				if (m is ISkeletonAdaptable mSkeleton) mSkeleton.SetParentSkeleton(Skeleton);
				if (m is IHandAdaptable mHanded) mHanded.SetHandedness(Handedness.Right); // TODO: get handedness
				m.SafeReparentEditor(this);
				m.AsIEnablable().EnableDisable(IsEnabled);
			})
			.Execute();

		OnLoadedUnloaded(true);
		_isLoaded = true;

		return true;
	}
	protected override void UnloadBehaviour() {
		base.UnloadBehaviour();

		OnLoadedUnloaded(false);

		new LoadableDestructor<Model>(ref _model)
			.AfterUnload(w => w.QueueFree())
			.Execute();

		_isLoaded = false;
	}


	private void OnLoadedUnloaded(bool isLoaded) {
		// EmitSignal(SignalName.CharacterLoadedUnloaded, isLoaded);

		// We don't use PropagateCall here because we need to Call Deferredly.
		SetParentRecursive(this, isLoaded ? Skeleton : null);

		void SetParentRecursive(Node parent, Skeleton3D? skeleton) {
			foreach (Node child in parent.GetChildren()) {
				SetParentRecursive(child, skeleton);
			}

			if (parent is ISkeletonAdaptable skeletonAdaptable) {
				Callable.From(() => skeletonAdaptable.SetParentSkeleton(skeleton)).CallDeferred();
			}
		}
	}

	private void OnKill() {
		GlobalPosition = lastStandableSurfaces[0];
		GD.Print($"Entity {Name} died.");
	}

	public void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice) {
		cameraController.SetEntityAsSubject(this);
		cameraController.MoveCamera(
			inputDevice.GetVector("look_left", "look_right", "look_down", "look_up") * inputDevice.Sensitivity
		);

		if (_weapon is not null) {
			if (inputDevice.IsActionJustPressed("switch_weapon_primary")) {
				_weapon.Style = 0;
			}
			else if (inputDevice.IsActionJustPressed("switch_weapon_secondary")) {
				_weapon.Style = 1;
			}
			else if (inputDevice.IsActionJustPressed("switch_weapon_ternary")) {
				_weapon.Style = 2;
			}
		}

		Interactable? interactionCandidate = CurrentBehaviour?.GetInteractionCandidate();
		if (inputDevice.IsActionJustPressed("interact") && interactionCandidate is not null && interactionCandidate.IsInteractable(entity)) {
			interactionCandidate.Interact(entity);
		}
	}

	public Interactable? GetInteractionCandidate() => CurrentBehaviour?.GetInteractionCandidate();


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint())
			return;

		Move(delta);
	}

	public override void _Ready() {
		base._Ready();

		if (Engine.IsEditorHint())
			return;


		SetBehaviour(new BipedBehaviour(this));
	}
}