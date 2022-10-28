using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ReflectionProbeUpdater : MonoBehaviour
{
	public List<ReflectionProbe> probes = new List<ReflectionProbe>();

	public void Start()
	{
		StartCoroutine(UpdateProbes());
	}

	private IEnumerator UpdateProbes()
	{
		foreach (ReflectionProbe p in probes)
		{
			p.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
			p.RenderProbe();
			yield return new WaitForEndOfFrame();
		}
	}
}
