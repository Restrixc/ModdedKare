using UnityEngine;

namespace Vilar.IK;

public interface IKSolver
{
	IKTargetSet targets { get; }

	void Solve();

	void Initialize();

	void SetTarget(int index, Vector3 position, Quaternion rotation);

	void ForceBlend(float value);

	void CleanUp();
}
