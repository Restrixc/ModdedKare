using UnityEngine;

public interface IJointData
{
	Joint Apply(Rigidbody connectedBodyOverride);
}
