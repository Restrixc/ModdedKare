using UnityEngine;

public class BakeTexture : MonoBehaviour
{
	public RenderTexture ResultTexture;

	public Texture2D texture;

	public void Bake()
	{
		RenderTexture.active = ResultTexture;
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, ResultTexture.width, ResultTexture.height, 0f);
		Graphics.Blit(texture, ResultTexture);
		GL.PopMatrix();
		RenderTexture.active = null;
	}
}
