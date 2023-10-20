using Godot;
using SevenGame.Utility;
using System;
using System.Linq;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Character : Model {

    [Export] public CharacterData Data { get; private set; }
    [Export] public Node3D Collisions { get; private set; }
    [Export] public Skeleton3D Armature { get; private set; }
    [Export] public CharacterModel CharacterModel { get; private set; }


    public Basis CharacterRotation { get; private set; } = Basis.Identity;


    [Export] public CharacterCostume CharacterCostume {
        get => CharacterModel?.Costume;
        private set => this.CallDeferredIfTools( Callable.From(() => SetCostume(value)) );
    }


    public Skeleton3D Skeleton => Armature;



    public Character() : base() {;}
    public Character(Node3D root, CharacterData data) : base(root) {
        Name = nameof(Character);

        Data = data;
    }


    public void SetCostume(CharacterCostume costume) {
        if ( this.IsInvalidTreeCallback() ) return;
        if ( CharacterModel?.Costume == costume ) return;

        CharacterModel?.QueueFree();
        CharacterModel = null;

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

        RefreshRotation();

        // CharacterModel?.LoadModel();

        return true;
    }

    protected override bool UnloadModelImmediate() {

        // CharacterModel?.UnloadModel();
        // CharacterModel?.QueueFree();
        // CharacterModel = null;

        Collisions?.QueueFree();
        Collisions = null;

        Armature?.QueueFree();
        Armature = null;

        return true;
    }

    public void RotateTowards(Basis target, double delta, float speed = 12f) {
        CharacterRotation = CharacterRotation.SafeSlerp(target, (float)delta * speed);

        RefreshRotation();
    }

    // public void RotateTowards(Vector3 newForward, Vector3 newUp, double delta, float speed = 12f) =>
    //     RotateTowards(Basis.LookingAt(newForward, newUp), delta, speed);

    protected virtual void RefreshRotation() {
        Transform = new(CharacterRotation, Transform.Origin);

        if ( Collisions is null ) return;

        Collisions.Transform = new(CharacterRotation, Collisions.Transform.Origin);
    }


}
