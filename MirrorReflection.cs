using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class MirrorReflection : MonoBehaviour
{
	public bool m_DisablePixelLights = true;

	public int m_TextureSize = 256;

	public float m_ClipPlaneOffset = 0.07f;

	public LayerMask m_ReflectLayers = -1;

	private Hashtable m_ReflectionCameras = new Hashtable();

	private RenderTexture m_ReflectionTexture = null;

	private int m_OldReflectionTextureSize = 0;

	private static bool s_InsideRendering;

	public void OnWillRenderObject()
	{
		Renderer rend = GetComponent<Renderer>();
		if (!base.enabled || !rend || !rend.sharedMaterial || !rend.enabled)
		{
			return;
		}
		Camera cam = Camera.current;
		if (!cam || s_InsideRendering)
		{
			return;
		}
		s_InsideRendering = true;
		CreateMirrorObjects(cam, out var reflectionCamera);
		Vector3 pos = base.transform.position;
		Vector3 normal = base.transform.up;
		int oldPixelLightCount = QualitySettings.pixelLightCount;
		if (m_DisablePixelLights)
		{
			QualitySettings.pixelLightCount = 0;
		}
		UpdateCameraModes(cam, reflectionCamera);
		float d = 0f - Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
		Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
		Matrix4x4 reflection = Matrix4x4.zero;
		CalculateReflectionMatrix(ref reflection, reflectionPlane);
		Vector3 oldpos = cam.transform.position;
		Vector3 newpos = reflection.MultiplyPoint(oldpos);
		reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
		Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1f);
		Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
		reflectionCamera.projectionMatrix = projection;
		reflectionCamera.cullingMask = -17 & m_ReflectLayers.value;
		reflectionCamera.targetTexture = m_ReflectionTexture;
		GL.invertCulling = true;
		reflectionCamera.transform.position = newpos;
		Vector3 euler = cam.transform.eulerAngles;
		reflectionCamera.transform.eulerAngles = new Vector3(0f, euler.y, euler.z);
		reflectionCamera.Render();
		reflectionCamera.transform.position = oldpos;
		GL.invertCulling = false;
		Material[] materials = rend.sharedMaterials;
		Material[] array = materials;
		foreach (Material mat in array)
		{
			if (mat.HasProperty("_ReflectionTex"))
			{
				mat.SetTexture("_ReflectionTex", m_ReflectionTexture);
			}
		}
		if (m_DisablePixelLights)
		{
			QualitySettings.pixelLightCount = oldPixelLightCount;
		}
		s_InsideRendering = false;
	}

	private void OnDisable()
	{
		if ((bool)m_ReflectionTexture)
		{
			Object.Destroy(m_ReflectionTexture);
			m_ReflectionTexture = null;
		}
		foreach (DictionaryEntry reflectionCamera in m_ReflectionCameras)
		{
			Object.Destroy(((Camera)reflectionCamera.Value).gameObject);
		}
		m_ReflectionCameras.Clear();
	}

	private void UpdateCameraModes(Camera src, Camera dest)
	{
		if (dest == null)
		{
			return;
		}
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;
		if (src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if (!sky || !sky.material)
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}

	private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
	{
		reflectionCamera = null;
		if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
		{
			if ((bool)m_ReflectionTexture)
			{
				Object.DestroyImmediate(m_ReflectionTexture);
			}
			m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
			m_ReflectionTexture.name = "__MirrorReflection" + GetInstanceID();
			m_ReflectionTexture.isPowerOfTwo = true;
			m_ReflectionTexture.hideFlags = HideFlags.DontSave;
			m_OldReflectionTextureSize = m_TextureSize;
		}
		reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
		if (!reflectionCamera)
		{
			GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
			reflectionCamera = go.GetComponent<Camera>();
			reflectionCamera.enabled = false;
			reflectionCamera.transform.position = base.transform.position;
			reflectionCamera.transform.rotation = base.transform.rotation;
			reflectionCamera.gameObject.AddComponent<FlareLayer>();
			go.hideFlags = HideFlags.HideAndDontSave;
			m_ReflectionCameras[currentCamera] = reflectionCamera;
		}
	}

	private static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 i = cam.worldToCameraMatrix;
		Vector3 cpos = i.MultiplyPoint(offsetPos);
		Vector3 cnormal = i.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(cnormal.x, cnormal.y, cnormal.z, 0f - Vector3.Dot(cpos, cnormal));
	}

	private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
	}
}
