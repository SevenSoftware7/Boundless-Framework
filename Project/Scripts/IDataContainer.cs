


using Godot;

namespace LandlessSkies.Core;

public interface IDataContainer<T, TCostume> : ICostumable<TCostume> where T : Resource where TCostume : Resource {
    T Data { get; }
}