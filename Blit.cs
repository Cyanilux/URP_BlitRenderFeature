using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*
 * Blit Renderer Feature                                                https://github.com/Cyanilux/URP_BlitRenderFeature
 * ------------------------------------------------------------------------------------------------------------------------
 * Based on the Blit from the UniversalRenderingExamples :
 * https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses
 * And Blit in XR from URP docs :
 * https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.1/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html
 * 
 * ------------------------------------------------------------------------------------------------------------------------
 * Important : Must put BlitDirectly.shader in the 'Always Included Shaders' section under Project Settings -> Graphics for this to work in builds!
 * ------------------------------------------------------------------------------------------------------------------------
 *
 * - Extended to allow for specific access to selecting a source and destination (via current camera's color / texture id / render texture object
 * - Now uses cmd.DrawMesh with fullscreen quad instead of cmd.Blit to avoid issues with single pass instanced VR/XR
		(Note should also use TEXTURE2D_X and SAMPLE_TEXTURE2D_X to properly support both eyes, BlitDirectly.shader provides an example)
 * - Shader should use global texture _MainTex to recieve source. If you use Shader Graph, make sure to change Reference and untick Exposed
 * - (Pre-2021.2/v12) Automatic switching to using _AfterPostProcessTexture for After Rendering event, in order to correctly handle the blit after post processing is applied
 * - Option to set a _InverseView matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world.
 * 		e.g. Reconstruct world pos from depth : https://www.cyanilux.com/tutorials/depth/#blit-perspective 
 * - (2020.1/v10 +) Option to enable generation of DepthNormals (_CameraNormalsTexture)
 * 		This will only include shaders who have a DepthNormals pass (mostly Lit Shaders / Graphs)
 		(workaround for Unlit Shaders / Graphs: https://gist.github.com/Cyanilux/be5a796cf6ddb20f20a586b94be93f2b)
 * ------------------------------------------------------------------------------------------------------------------------
 * @Cyanilux
*/

namespace Cyan {

	[CreateAssetMenu(menuName = "Feature/Blit")]
	public class Blit : ScriptableRendererFeature {

		public class BlitPass : ScriptableRenderPass {

			BlitSettings settings;
			
			private RenderTargetIdentifier source { get; set; }
			private RenderTargetIdentifier destination { get; set; }

			private RenderTargetIdentifier cameraDepthTarget;

			private RenderTargetHandle m_TemporaryColorTexture;
			private RenderTargetHandle m_DestinationTexture;
			private string m_ProfilerTag;
			private int blitTextureID = Shader.PropertyToID("_MainTex");
			private Material blitDirectlyMaterial;
			private bool hasPrintedError;

			public BlitPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag) {
				this.renderPassEvent = renderPassEvent;
				this.settings = settings;
				m_ProfilerTag = tag;
				m_TemporaryColorTexture.Init("_TemporaryColorTexture");
				if (settings.dstType == Target.TextureID) {
					m_DestinationTexture.Init(settings.dstTextureId);
				}
			}

			public void Setup() {
	#if UNITY_2020_1_OR_NEWER
				if (settings.requireDepthNormals)
					ConfigureInput(ScriptableRenderPassInput.Normal);
	#endif
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
				CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
				RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
				opaqueDesc.depthBufferBits = 0;

				// Set Source / Destination
				// note : Seems this has to be done in here rather than in AddRenderPasses to work correctly in 2021.2+
				var renderer = renderingData.cameraData.renderer;
				cameraDepthTarget = renderer.cameraDepthTarget;

				if (settings.srcType == Target.CameraColor) {
					source = renderer.cameraColorTarget;
				} else if (settings.srcType == Target.TextureID) {
					source = new RenderTargetIdentifier(settings.srcTextureId);
				} else if (settings.srcType == Target.RenderTextureObject) {
					source = new RenderTargetIdentifier(settings.srcTextureObject);
				}

				if (settings.dstType == Target.CameraColor) {
					destination = renderer.cameraColorTarget;
				} else if (settings.dstType == Target.TextureID) {
					destination = new RenderTargetIdentifier(settings.dstTextureId);
				} else if (settings.dstType == Target.RenderTextureObject) {
					destination = new RenderTargetIdentifier(settings.dstTextureObject);
				}

				// Set Inverse Matrix
				if (settings.setInverseViewMatrix) {
					Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
				}

				// Setup Destination TempRT
				if (settings.dstType == Target.TextureID) {
					if (settings.overrideGraphicsFormat) {
						opaqueDesc.graphicsFormat = settings.graphicsFormat;
					}
					cmd.GetTemporaryRT(m_DestinationTexture.id, opaqueDesc, settings.filterMode);
				}

				//Debug.Log($"src = {source},     dst = {destination} ");
				/*
				// Older blit method, this doesn't work with VR/XR and some other issues so I've switched to using DrawMesh instead~
				if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor)) {
					cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, settings.filterMode);
					Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, settings.blitMaterialPassIndex);
					Blit(cmd, m_TemporaryColorTexture.Identifier(), destination);
				} else {
					Blit(cmd, source, destination, blitMaterial, settings.blitMaterialPassIndex);
				}
				*/

				// Handle DrawFullScreenMesh
				if (settings.overrideViewProjection) OverrideCameraViewProjection(cmd, renderingData.cameraData);
				if (source == destination) {
					// If same source/destination, we cannot read & write to the same target so use a TemporaryRT
					cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, settings.filterMode);

					DrawFullscreenMesh(cmd, source, m_TemporaryColorTexture.Identifier(), settings.blitMaterial, settings.blitMaterialPassIndex);	
					if (settings.overrideViewProjection) RestoreCameraViewProjection(cmd, renderingData.cameraData);

					if (blitDirectlyMaterial == null) {
						Shader blitDirectlyShader = Shader.Find("Hidden/Cyan/BlitDirectly");
						if (blitDirectlyShader == null){
							if (!hasPrintedError){ // Just to prevent spamming the console
								Debug.LogError("BlitRenderFeature : BlitDirectly.shader (Hidden/Cyan/BlitDirectly) is not included in build! Please put it in 'Always Included Shaders' section under Project Settings -> Graphics.");
								hasPrintedError = true;
							}
						}else{
							blitDirectlyMaterial = new Material(blitDirectlyShader);
						}
					}
					if (blitDirectlyMaterial != null) {
						DrawFullscreenMesh(cmd, m_TemporaryColorTexture.Identifier(), destination, blitDirectlyMaterial, 0);
					}
				} else {
					// Different targets, can draw single quad
					DrawFullscreenMesh(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
					if (settings.overrideViewProjection) RestoreCameraViewProjection(cmd, renderingData.cameraData);
				}
				
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				CommandBufferPool.Release(cmd);
			}

			public void DrawFullscreenMesh(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material blitMaterial, int materialIndex) {
				// Send Source Texture into Shader
				cmd.SetGlobalTexture(blitTextureID, source);

				// Set Render Target to Destination
				if (settings.dstType == Target.CameraColor){
					cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1), cameraDepthTarget);
				}else{
					cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1));
				}
				
				// Draw Fullscreen Quad
				cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitMaterial, 0, materialIndex);
			}

			public void OverrideCameraViewProjection(CommandBuffer cmd, CameraData cameraData) {
				// Clear View/Projection
				/* 
				- Technically this could be optional - whether it's neccessary depends on if the shader uses the transformation matrices or not
				- Code-written shaders can just do "output.positionCS = float4(input.positionOS.xyz, 1.0);" so no need to override matrices,
				- But I'd also like to be able to still use Shader Graphs with this feature and there's no easy way to output into clip space for those :\
				*/
				Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.identity, cameraData.IsCameraProjectionMatrixFlipped());
				RenderingUtils.SetViewAndProjectionMatrices(cmd, Matrix4x4.identity, projectionMatrix, false);
			}

			public void RestoreCameraViewProjection(CommandBuffer cmd, CameraData cameraData) {
				RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
			}

			public override void FrameCleanup(CommandBuffer cmd) {
				if (settings.dstType == Target.TextureID) {
					cmd.ReleaseTemporaryRT(m_DestinationTexture.id);
				}
				if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor)) {
					cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
				}
			}
		}

		// Settings

		[System.Serializable]
		public class BlitSettings {
			public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
			public Material blitMaterial = null;
			public int blitMaterialPassIndex = 0;

			[Tooltip("Can be false if shader outputs directly to clip space without using view & projection matrices. "
			+ "For shaders that do transformations (and Shader Graphs where it's not really possible to override) "
			+ "set this to true so quad is rendered to screen.")]
			public bool overrideViewProjection = true;
			[Tooltip("Sets a global _InverseView matrix, set to camera.cameraToWorldMatrix. "
			+ "Can be used in shader to transform from view space to world space. "
			+ "Useful for reconstructing world positions from depth texture.")]
			public bool setInverseViewMatrix = false;
			[Tooltip("(2020.1+) Enables the generation of URP's CameraNormalsTexture")]
			public bool requireDepthNormals = false;

			public Target srcType = Target.CameraColor;
			public string srcTextureId = "_CameraColorTexture";
			public RenderTexture srcTextureObject;

			public Target dstType = Target.CameraColor;
			public string dstTextureId = "_BlitPassTexture";
			public RenderTexture dstTextureObject;

			public bool overrideGraphicsFormat = false;
			public UnityEngine.Experimental.Rendering.GraphicsFormat graphicsFormat;

			public FilterMode filterMode = FilterMode.Bilinear;
		}

		public enum Target {
			CameraColor,
			TextureID,
			RenderTextureObject
		}

		// Feature Setup

		public BlitSettings settings = new BlitSettings();
		public BlitPass blitPass;

		public override void Create() {
			var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
			settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
			blitPass = new BlitPass(settings.Event, settings, name);

	#if !UNITY_2021_2_OR_NEWER
			if (settings.Event == RenderPassEvent.AfterRenderingPostProcessing) {
				Debug.LogWarning("Note that the \"After Rendering Post Processing\"'s Color target doesn't seem to work? (or might work, but doesn't contain the post processing) :( -- Use \"After Rendering\" instead!");
			}
	#endif

			if (settings.graphicsFormat == UnityEngine.Experimental.Rendering.GraphicsFormat.None) {
				settings.graphicsFormat = SystemInfo.GetGraphicsFormat(UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
			}
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {

			if (settings.blitMaterial == null) {
				Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
				return;
			}

	#if !UNITY_2021_2_OR_NEWER
			// AfterRenderingPostProcessing event is fixed in 2021.2+ so this workaround is no longer required

			if (settings.Event == RenderPassEvent.AfterRenderingPostProcessing) {
			} else if (settings.Event == RenderPassEvent.AfterRendering && renderingData.postProcessingEnabled) {
				// If event is AfterRendering, and src/dst is using CameraColor, switch to _AfterPostProcessTexture instead.
				if (settings.srcType == Target.CameraColor) {
					settings.srcType = Target.TextureID;
					settings.srcTextureId = "_AfterPostProcessTexture";
				}
				if (settings.dstType == Target.CameraColor) {
					settings.dstType = Target.TextureID;
					settings.dstTextureId = "_AfterPostProcessTexture";
				}
			} else {
				// If src/dst is using _AfterPostProcessTexture, switch back to CameraColor
				if (settings.srcType == Target.TextureID && settings.srcTextureId == "_AfterPostProcessTexture") {
					settings.srcType = Target.CameraColor;
					settings.srcTextureId = "";
				}
				if (settings.dstType == Target.TextureID && settings.dstTextureId == "_AfterPostProcessTexture") {
					settings.dstType = Target.CameraColor;
					settings.dstTextureId = "";
				}
			}
	#endif

			blitPass.Setup();
			renderer.EnqueuePass(blitPass);
		}
	}

}