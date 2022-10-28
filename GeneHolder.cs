using Photon.Pun;

public class GeneHolder : MonoBehaviourPun
{
	public delegate void GenesChangedAction(KoboldGenes newGenes);

	private KoboldGenes genes;

	public event GenesChangedAction genesChanged;

	public KoboldGenes GetGenes()
	{
		return genes;
	}

	public virtual void SetGenes(KoboldGenes newGenes)
	{
		genes = newGenes;
		this.genesChanged?.Invoke(newGenes);
	}
}
