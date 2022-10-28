using UnityEngine;

public static class GameObjectExtension
{
	public static Type[] GetAllComponents<Type>(this GameObject obj)
	{
		return obj.transform.root.gameObject.GetComponentsInChildren<Type>();
	}

	public static Type[] GetAllComponents<Type>(this Collider obj)
	{
		return obj.transform.root.gameObject.GetComponentsInChildren<Type>();
	}
}
