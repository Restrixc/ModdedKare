using UnityEngine;

public interface IAdvancedInteractable
{
	Transform transform { get; }

	GameObject gameObject { get; }

	void InteractTo(Vector3 worldPosition, Quaternion worldRotation);

	void OnInteract(Kobold k);

	void OnEndInteract();

	bool PhysicsGrabbable();
}
