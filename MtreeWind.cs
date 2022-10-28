using UnityEngine;

[ExecuteInEditMode]
public class MtreeWind : MonoBehaviour
{
	[Header("Global Windzone")]
	public WindZone windZone;

	[Header("Mtree Wind Offset")]
	public float windStrength = 0f;

	public float windDirection = 0f;

	public float windPulse = 0f;

	public float windTurbulence = 0f;

	[Range(0f, 1f)]
	public float windRandomness = 1f;

	[Header("Billboard")]
	public bool BillboardWind = false;

	[Range(0f, 1f)]
	public float BillboardWindInfluence = 0.5f;

	private float m_windStrength;

	private float m_windDirection;

	private float m_windPulse;

	private float m_windTurbulence;

	private void Awake()
	{
		windZone = (WindZone)Object.FindObjectOfType(typeof(WindZone));
		if (!windZone)
		{
			windZone = (WindZone)base.gameObject.AddComponent(typeof(WindZone));
		}
	}

	private void Update()
	{
		if ((bool)windZone && (m_windStrength != windZone.windMain || m_windDirection != windZone.transform.rotation.eulerAngles.y || m_windPulse != windZone.windPulseFrequency || m_windTurbulence != windZone.windTurbulence))
		{
			UpdateWindZone();
			m_windStrength = windZone.windMain;
			m_windDirection = windZone.transform.rotation.eulerAngles.y;
			m_windPulse = windZone.windPulseFrequency;
			m_windTurbulence = windZone.windTurbulence;
		}
	}

	public void UpdateWind()
	{
		Shader.SetGlobalFloat("_WindStrength", windStrength);
		Shader.SetGlobalFloat("_WindDirection", windDirection);
		Shader.SetGlobalFloat("_WindPulse", windPulse);
		Shader.SetGlobalFloat("_WindTurbulence", windTurbulence);
	}

	public void UpdateWindZone()
	{
		Shader.SetGlobalFloat("_WindStrength", windZone.windMain + windStrength);
		Shader.SetGlobalFloat("_WindDirection", windZone.transform.localRotation.eulerAngles.y + windDirection);
		Shader.SetGlobalFloat("_WindPulse", windZone.windPulseFrequency + windPulse);
		Shader.SetGlobalFloat("_WindTurbulence", windZone.windTurbulence + windTurbulence);
	}

	public void OnValidate()
	{
		Shader.SetGlobalFloat("_RandomWindOffset", windRandomness);
		if ((bool)windZone)
		{
			UpdateWindZone();
		}
		if (!windZone)
		{
			UpdateWind();
		}
		if (BillboardWind)
		{
			Shader.SetGlobalInt("BillboardWindEnabled", 0);
			Shader.SetGlobalFloat("Billboard_WindStrength", BillboardWindInfluence);
		}
		if (!BillboardWind)
		{
			Shader.SetGlobalInt("BillboardWindEnabled", 1);
		}
	}

	public void ResetToZero()
	{
		windStrength = 0f;
		windDirection = 0f;
		windTurbulence = 0f;
		windPulse = 0f;
		UpdateWind();
	}

	public void OnDisable()
	{
		ResetToZero();
	}

	public void OnDestroy()
	{
		ResetToZero();
	}

	public void OnEnable()
	{
		if ((bool)windZone)
		{
			UpdateWindZone();
		}
		else
		{
			UpdateWind();
		}
	}
}
