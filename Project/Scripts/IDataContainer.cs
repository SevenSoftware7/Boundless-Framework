


using Godot;

namespace LandlessSkies.Core;

public interface IDataContainer<T> where T : Resource {
    T Data { get; }
}