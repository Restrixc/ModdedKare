using System.Collections.ObjectModel;
using Photon.Pun;
using Vilar.AnimationStation;

public interface IAnimationStationSet
{
	PhotonView photonView { get; }

	ReadOnlyCollection<AnimationStation> GetAnimationStations();
}
