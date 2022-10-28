using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
	private GameObject _player = null;

	private void Update()
	{
		if (_player == null)
		{
			_player = GameObject.FindGameObjectWithTag("Player");
		}
		else
		{
			base.transform.position = _player.transform.position;
		}
	}
}
