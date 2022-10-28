using PenetrationTech;
using UnityEngine;

public class Dildo : MonoBehaviour
{
	public delegate void PenetrateAction(Penetrator penetrator, Penetrable penetrable);

	private Penetrator listenPenetrator;

	public static event PenetrateAction dildoPenetrateStart;

	public static event PenetrateAction dildoPenetrateEnd;

	private void Awake()
	{
		listenPenetrator = GetComponentInChildren<Penetrator>();
	}

	private void OnEnable()
	{
		listenPenetrator.penetrationStart += OnPenetrationStart;
		listenPenetrator.penetrationEnd += OnPenetrationEnd;
	}

	private void OnDisable()
	{
		listenPenetrator.penetrationStart -= OnPenetrationStart;
		listenPenetrator.penetrationEnd -= OnPenetrationEnd;
	}

	private void OnPenetrationStart(Penetrable penetrable)
	{
		Dildo.dildoPenetrateStart?.Invoke(listenPenetrator, penetrable);
	}

	private void OnPenetrationEnd(Penetrable penetrable)
	{
		Dildo.dildoPenetrateEnd?.Invoke(listenPenetrator, penetrable);
	}
}
