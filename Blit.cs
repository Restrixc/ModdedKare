using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Blit : ScriptableRendererFeature
{
	public class BlitPass : ScriptableRenderPass
	{
		public Material blitMaterial = null;

		private BlitSettings settings;

		private RenderTargetHandle m_TemporaryColorTexture;

		private RenderTargetHandle m_DestinationTexture;

		private string m_ProfilerTag;

		public FilterMode filterMode { get; set; }

		private RenderTargetIdentifier source { get; set; }

		private RenderTargetIdentifier destination { get; set; }

		public BlitPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
		{
			base.renderPassEvent = renderPassEvent;
			this.settings = settings;
			blitMaterial = settings.blitMaterial;
			m_ProfilerTag = tag;
			m_TemporaryColorTexture.Init("_TemporaryColorTexture");
			if (settings.dstType == Target.TextureID)
			{
				m_DestinationTexture.Init(settings.dstTextureId);
			}
		}

		public void Setup(RenderTargetIdentifier source, RenderTargetIdentifier destination)
		{
			this.source = source;
			this.destination = destination;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
			RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
			opaqueDesc.depthBufferBits = 0;
			if (settings.setInverseViewMatrix)
			{
				Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
			}
			if (settings.dstType == Target.TextureID)
			{
				cmd.GetTemporaryRT(m_DestinationTexture.id, opaqueDesc, filterMode);
			}
			if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
			{
				cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
				Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, settings.blitMaterialPassIndex);
				Blit(cmd, m_TemporaryColorTexture.Identifier(), destination);
			}
			else
			{
				Blit(cmd, source, destination, blitMaterial, settings.blitMaterialPassIndex);
			}
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
			if (settings.dstType == Target.TextureID)
			{
				cmd.ReleaseTemporaryRT(m_DestinationTexture.id);
			}
			if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
			{
				cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
			}
		}
	}

	[Serializable]
	public class BlitSettings
	{
		public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

		public Material blitMaterial = null;

		public int blitMaterialPassIndex = 0;

		public bool setInverseViewMatrix = false;

		public Target srcType = Target.CameraColor;

		public string srcTextureId = "_CameraColorTexture";

		public RenderTexture srcTextureObject;

		public Target dstType = Target.CameraColor;

		public string dstTextureId = "_BlitPassTexture";

		public RenderTexture dstTextureObject;
	}

	public enum Target
	{
		CameraColor,
		TextureID,
		RenderTextureObject
	}

	public BlitSettings settings = new BlitSettings();

	private BlitPass blitPass;

	private RenderTargetIdentifier srcIdentifier;

	private RenderTargetIdentifier dstIdentifier;

	public override void Create()
	{
		int passIndex = ((!(settings.blitMaterial != null)) ? 1 : (settings.blitMaterial.passCount - 1));
		settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
		blitPass = new BlitPass(settings.Event, settings, base.name);
		if (settings.Event == RenderPassEvent.AfterRenderingPostProcessing)
		{
			Debug.LogWarning("Note that the \"After Rendering Post Processing\"'s Color target doesn't seem to work? (or might work, but doesn't contain the post processing) :( -- Use \"After Rendering\" instead!");
		}
		UpdateSrcIdentifier();
		UpdateDstIdentifier();
	}

	private void UpdateSrcIdentifier()
	{
		srcIdentifier = UpdateIdentifier(settings.srcType, settings.srcTextureId, settings.srcTextureObject);
	}

	private void UpdateDstIdentifier()
	{
		dstIdentifier = UpdateIdentifier(settings.dstType, settings.dstTextureId, settings.dstTextureObject);
	}

	private RenderTargetIdentifier UpdateIdentifier(Target type, string s, RenderTexture obj)
	{
		return type switch
		{
			Target.RenderTextureObject => obj, 
			Target.TextureID => s, 
			_ => default(RenderTargetIdentifier), 
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (settings.blitMaterial == null)
		{
			Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
			return;
		}
		if (settings.Event != RenderPassEvent.AfterRenderingPostProcessing)
		{
			if (settings.Event == RenderPassEvent.AfterRendering && renderingData.postProcessingEnabled)
			{
				if (settings.srcType == Target.CameraColor)
				{
					settings.srcType = Target.TextureID;
					settings.srcTextureId = "_AfterPostProcessTexture";
					UpdateSrcIdentifier();
				}
				if (settings.dstType == Target.CameraColor)
				{
					settings.dstType = Target.TextureID;
					settings.dstTextureId = "_AfterPostProcessTexture";
					UpdateDstIdentifier();
				}
			}
			else
			{
				if (settings.srcType == Target.TextureID && settings.srcTextureId == "_AfterPostProcessTexture")
				{
					settings.srcType = Target.CameraColor;
					settings.srcTextureId = "";
					UpdateSrcIdentifier();
				}
				if (settings.dstType == Target.TextureID && settings.dstTextureId == "_AfterPostProcessTexture")
				{
					settings.dstType = Target.CameraColor;
					settings.dstTextureId = "";
					UpdateDstIdentifier();
				}
			}
		}
		RenderTargetIdentifier src = ((settings.srcType == Target.CameraColor) ? renderer.cameraColorTarget : srcIdentifier);
		RenderTargetIdentifier dest = ((settings.dstType == Target.CameraColor) ? renderer.cameraColorTarget : dstIdentifier);
		blitPass.Setup(src, dest);
		renderer.EnqueuePass(blitPass);
	}
}
