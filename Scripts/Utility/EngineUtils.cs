using System.Runtime.CompilerServices;
using System.Text;
using Godot;


namespace LandlessSkies.Core;

public static class EngineUtils {

    private static readonly ulong buildFrame = 0;



    static EngineUtils() {
        buildFrame = Engine.GetProcessFrames();
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEditorGetSetter(this Node node) =>
        !node.IsNodeReady() || Engine.GetProcessFrames() == buildFrame;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEditorEnterTree(this Node node) =>
        node.IsNodeReady() || Engine.GetProcessFrames() == buildFrame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEditorExitTree(this Node node) =>
        !node.IsNodeReady() || Engine.GetProcessFrames() == buildFrame;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddChildAndSetOwner(this Node obj, Node child) {
        obj.AddChild(child);
        child.Owner = obj.Owner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNode ParentTo<TNode>(this TNode child, Node parent) where TNode : Node {
        parent.AddChild(child);
        return child;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNode SetOwnerAndParentTo<TNode>(this TNode child, Node parent) where TNode : Node {
        parent.AddChildAndSetOwner(child);
        return child;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnparentAndQueueFree(this Node obj) {
        obj.QueueFree();
        obj.GetParent()?.RemoveChild(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetNodeByTypeName<T>(this Node obj) where T : Node {
        return obj.GetNodeOrNull<T>(typeof(T).Name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNode<T>(this Node obj, NodePath nodePath, out T node) where T : class {
        if ( nodePath is null || ! obj.HasNode(nodePath) ) {
            node = default!;
            return false;
        }
        Node nodeOrNull = obj.GetNodeOrNull(nodePath);
        if ( nodeOrNull is T tNode ) {
            node = tNode;
            return true;
        }
        node = default!;
        return false;
    }


    public static void SetValueFromNode<T>(this Node obj, NodePath path, ref NodePath valToSet) where T : class {
        if ( ! obj.IsNodeReady() ) {
            valToSet = path;
            return;
        }
        if ( ! path.IsEmpty && ! obj.TryGetNode<T>(path, out _) ) {
            GD.PushWarning($"Reference ({path}) is not assignable to Class {typeof(T).Name}");
            return;
        }
        valToSet = path;
    }
}