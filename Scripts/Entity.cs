using Godot;
using System;


namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class Entity : CharacterBody3D {
    
    [Export] public EntityBehaviourManager BehaviourManager { 
        get {
            _behaviourManager ??= GetNodeOrNull<EntityBehaviourManager>(nameof(EntityBehaviourManager));
            _behaviourManager ??= new(this);
            return _behaviourManager;
        }
        private set => _behaviourManager = value;
    }
    private EntityBehaviourManager _behaviourManager;


    [Export] public Character CharacterInstance { 
        get {
            _character ??= GetNodeOrNull<Character>(nameof(Character));
            _character ??= new(this);
            return _character;
        }
        private set => _character = value;
    }
    private Character _character;


    public void HandleInput(Player player) {

    }

	public override void _Ready() {
        base._Ready();

        if ( Engine.IsEditorHint() ) {
            _ReadyEditor();
        } else {
            _ReadyGame();
        }
	}

    private void _ReadyEditor() {
    }
    private void _ReadyGame() {
        BehaviourManager.SetBehaviour<TestBehaviour>();
    }


}
