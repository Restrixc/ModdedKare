using UnityEngine;

public interface INetworkedEventShooter
{
	Transform transform { get; }

	void FireEvent();
}
