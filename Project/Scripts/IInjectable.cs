

namespace LandlessSkies.Core;

public interface IInjectable<T> {
	public void Inject(T value);
}