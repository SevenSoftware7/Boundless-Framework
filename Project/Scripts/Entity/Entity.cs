using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Godot;
using Godot.Collections;
using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Entity : CharacterBody3D, IInputReader {

	public EntityAction? CurrentAction { get; private set; }
	public EntityBehaviourManager? BehaviourManager { get; private set; }


	[Export] public bool GenerateScene {
		get => false;
		set {
			if ( this.IsInitializationSetterCall() ) return;

			using PackedScene scene = new();
			scene.PackWithSubnodes(this);
			Dictionary dict = scene._Bundled;


			using PackedScene duplicate = new();
			duplicate._Bundled = dict;

			Node resulting = duplicate.Instantiate();
			resulting.ParentTo(GetParent());
			resulting.MakeLocal(Owner);
		}
	}


	[Export] public AnimationPlayer? AnimationPlayer {
		get => _animationPlayer;
		private set {
			if (value is null) return;
			_animationPlayer = value;

			if ( this.IsInitializationSetterCall() ) return;
			_animationPlayer.RootNode = GetTree().EditedSceneRoot.GetPathTo(this);
		}
	}
	private AnimationPlayer? _animationPlayer;

	[Export] public Character? Character {
		get => _character;
		private set {
			if (value is null) return;
			_character = value;

			if ( this.IsInitializationSetterCall() ) return;
			_character.Name = PropertyName.Character;
		}
	}
	private Character? _character;

	[Export] public CharacterData? CharacterData {
		get => Character?.Data;
		private set {
			if ( this.IsInitializationSetterCall() ) return;
			SetCharacter(value);
		}
	}

	[Export] public CharacterCostume? CharacterCostume {
		get => Character?.Costume;
		private set {
			if ( this.IsInitializationSetterCall() ) return;
			SetCostume(value);
		}
	}


	[ExportGroup("Dependencies")]
	[Export] public Health? Health;

	[Export] public Weapon? Weapon {
		get => _weapon;
		set {
			bool isInitialization = this.IsInitializationSetterCall();
			if ( ! isInitialization ) {
				_weapon?.Inject(null);
			}

			_weapon = value;

			if ( ! isInitialization && _weapon is not null) {
				_weapon.Inject(this);
				_weapon.Name = PropertyName.Weapon;
			}
		}
	}
	private Weapon? _weapon;



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
		set {
			_absoluteForward = value.Normalized();
			_relativeForward = Transform.Basis.Inverse() * _absoluteForward;
		}
	}
	private Vector3 _absoluteForward;


	/// <summary>
	/// The forward direction in relative space of the Entity.
	/// </summary>
	/// <remarks>
	/// Editing this value also changes <see cref="AbsoluteForward"/> to match.
	/// </remarks>
	[Export] public Vector3 RelativeForward {
		get => _relativeForward;
		set {
			_relativeForward = value.Normalized();
			_absoluteForward = Transform.Basis * _relativeForward;
		}
	}
	private Vector3 _relativeForward;
	[ExportGroup("")]


	public Skeleton3D? Skeleton => Character?.Skeleton;


	[Signal] public delegate void CharacterChangedEventHandler(CharacterData? newCharacter, CharacterData? oldCharacter);
	[Signal] public delegate void CharacterLoadedUnloadedEventHandler(bool isLoaded);



	public Entity() : base() {
		CollisionLayer = 1 << 1;
	}



	public void SetCharacter(CharacterData? data, CharacterCostume? costume = null) {
		CharacterData? oldData = CharacterData;
		if ( data == oldData ) return;

		new LoadableUpdater<Character>(ref _character, () => data?.Instantiate(costume))
			.BeforeLoad(c => {
				c.Name = PropertyName.Character;
				c.SafeReparentEditor(this);
				c.Connect(Character.SignalName.LoadedUnloaded, new Callable(this, MethodName.OnCharacterLoadedUnloaded), (uint)ConnectFlags.Persist);
			})
			.Execute();

		EmitSignal(SignalName.CharacterChanged, data!, oldData!);
	}

	public void SetCostume(CharacterCostume? costume) {
		Character?.SetCostume(costume);
	}

	public void ExecuteAction(EntityAction.IInfo action, bool ignoreCancellability = false) {
		if ( CurrentAction is EntityAction currentAction && ! currentAction.IsCancellable && ! ignoreCancellability ) return;

		try {
			CurrentAction?.Dispose();
			CurrentAction = null;
		} catch (ObjectDisposedException) {}

		action.AfterExecute += () => CurrentAction = null;
		CurrentAction = action.Execute();
	}



	public void HandleInput(Player.InputInfo inputInfo) {
		BehaviourManager?.CurrentBehaviour?.HandleInput(inputInfo);

		Weapon?.HandleInput(inputInfo);
	}

	public void SplitInertia(out Vector3 vertical, out Vector3 horizontal) {
		vertical = Inertia.Project( UpDirection );
		horizontal = Inertia - vertical;
	}



	private void OnCharacterLoadedUnloaded(bool isLoaded) {
		EmitSignal(SignalName.CharacterLoadedUnloaded, isLoaded);
	}

	private void OnCharacterChanged(CharacterData? newCharacter, CharacterData? oldCharacter) {
		OnCharacterLoadedUnloaded(Character?.IsLoaded ?? false);
	}



	public override void _Process(double delta) {
		base._Process(delta);

		if ( Engine.IsEditorHint() ) return;

		// if ( GravityMultiplier != 0f & Weight != 0f ) {
			SplitInertia(out Vector3 verticalInertia, out Vector3 horizontalInertia);
			
			if ( MotionMode == MotionModeEnum.Grounded ) {
				if ( IsOnFloor() ) verticalInertia = verticalInertia.FlattenInDirection( -UpDirection );
				if ( IsOnCeiling() ) verticalInertia = verticalInertia.FlattenInDirection( UpDirection );
			}
			
			Inertia = horizontalInertia + verticalInertia;
		// }

		Velocity = Movement + Inertia;
		MoveAndSlide();
	}

	public override void _Ready() {
		base._Ready();

		if ( ! Engine.IsEditorHint() ) {
			BehaviourManager ??= new(this);
			BehaviourManager?.SetBehaviour( new TestBehaviour(this) );
		}
	}


	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
			Callable.From(() => _weapon?.Inject(null)).CallDeferred();
		}
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();
		
		if (name == PropertyName.Character) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		
		} else if (
			name == PropertyName.CharacterCostume ||
			name == PropertyName.CharacterData
		) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);

		}
	}
}