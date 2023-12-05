


using Godot;

namespace LandlessSkies.Core;

public interface IInterfaceWrapper<T> {
    public T? Get(Node root);
    public void Set(Node root, T? value);
}