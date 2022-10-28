using UnityEngine;

public interface ISpoilable
{
	Transform transform { get; }

	void OnSpoil();
}
