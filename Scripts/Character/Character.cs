using Godot;
using SevenGame.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Character : Model {

    [Export] public Node3D? Collisions { get; private set; }
    [Export] public Skeleton3D? Armature { get; private set; }
    [Export] public CharacterModel? CharacterModel { get; private set; }


    [Export] public CharacterData Data { 
        get => _data;
        private set => _data ??= value;
    }
    private CharacterData _data;

    [Export] public CharacterCostume? CharacterCostume {
        get => _characterCostume ??= CharacterModel?.Costume;
        private set => this.CallDeferredIfTools( Callable.From(() => SetCostume(value)) );
    }
    private CharacterCostume? _characterCostume;


    public Basis CharacterRotation { get; private set; } = Basis.Identity;


    [Signal] public delegate void CostumeChangedEventHandler(CharacterCostume? newCostume, CharacterCostume? oldCostume);



    public Character() : base() {
        _data ??= null !;

        Name = nameof(Character);
    }
    public Character(Node3D? root, CharacterData data) : base(root) {
        ArgumentNullException.ThrowIfNull(data);

        _data = data;

        Name = nameof(Character);
    }



    public void SetCostume(CharacterCostume? costume, bool forceLoad = false) {
        if ( this.IsInvalidTreeCallback() ) return;
        if ( CharacterCostume == costume ) return;

        CharacterModel?.QueueFree();
        CharacterModel = null !;

        _characterCostume = costume;
        EmitSignal(SignalName.CostumeChanged, costume!);

        if ( costume is null ) return;

        CharacterModel = costume.Instantiate(this, Armature);
        CharacterModel?.LoadModel();
    }



    protected override bool LoadModelImmediate() {
        Node parent = GetParent();
        if ( parent is null ) return false;
        if ( Data is null ) return false;

        Collisions = Data.CollisionScene?.Instantiate() as Node3D;
        if ( Collisions is not null ) {
            Collisions.Name = nameof(Collisions);
            parent.AddChildSetOwner(Collisions);
        }

        Armature = Data.SkeletonScene?.Instantiate() as Skeleton3D;
        if ( Armature is not null ) {
            Armature.Name = nameof(Armature);
            this.AddChildSetOwner(Armature);
        }

        CharacterModel = _characterCostume?.Instantiate(this, Armature);
        CharacterModel?.LoadModel();

        RefreshRotation();

        return true;
    }

    protected override bool UnloadModelImmediate() {
        Collisions?.QueueFree();
        Collisions = null !;

        Armature?.QueueFree();
        Armature = null !;

        CharacterModel?.QueueFree();
        CharacterModel = null !;

        return true;
    }

    public void RotateTowards(Basis target, double delta, float speed = 12f) {
        CharacterRotation = CharacterRotation.SafeSlerp(target, (float)delta * speed);

        RefreshRotation();
    }

    protected virtual void RefreshRotation() {
        Transform = new(CharacterRotation, Transform.Origin);

        if ( Collisions is null ) return;

        Collisions.Transform = new(CharacterRotation, Collisions.Transform.Origin);
    }


// #if TOOLS
//     public override string[] _GetConfigurationWarnings() {
//         string[] baseWarnings = base._GetConfigurationWarnings();

//         if ( Collisions is null ) {
//             baseWarnings ??= Array.Empty<string>();
//             Array.Resize(ref baseWarnings, baseWarnings.Length + 1);
//             baseWarnings[^1] = $"{nameof(Collisions)} is null.";
//         }

//         if ( Armature is null ) {
//             baseWarnings ??= Array.Empty<string>();
//             Array.Resize(ref baseWarnings, baseWarnings.Length + 1);
//             baseWarnings[^1] = $"{nameof(Armature)} is null.";
//         }

//         if ( CharacterModel is null ) {
//             baseWarnings ??= Array.Empty<string>();
//             Array.Resize(ref baseWarnings, baseWarnings.Length + 1);
//             baseWarnings[^1] = $"{nameof(CharacterModel)} is null.";
//         }

//         return baseWarnings;
//     }
// #endif


}
