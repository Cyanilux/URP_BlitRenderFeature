using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;
using System;

/*
 * @Cyanilux https://github.com/Cyanilux/URP_BlitRenderFeature
*/

namespace Cyan {
    public class Blit : ScriptableRendererFeature {

        [Serializable]
        public class FeatureSettings {

            public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;

            // (Same as ScriptableRenderPassInput, but renamed "Color" to "OpaqueTexture" for clarity)
            [Flags]
            public enum Requirements {
                None                = 0,
                DepthTexture        = 1 << 0,   // resourcesData.cameraDepthTexture
                NormalsTexture      = 1 << 1,   // resourcesData.cameraNormalsTexture
                OpaqueTexture       = 1 << 2,   // resourcesData.cameraOpaqueTexture
                MotionVectorTexture = 1 << 3,   // resourcesData.motionVectorColor & resourcesData.motionVectorDepth
            }

            [Header("Inputs")]
            public Requirements requirements;
            public string[] globalTextures = new string[]{};
            /*
            Declares textures that are used by the pass. I assume this keeps them "alive" if that resource is handled by RenderGraph.
            Note it's up to a custom material/shader to actually sample that global reference.
            */

            public enum Destination {
                CameraColor,
                GlobalTexture
            }
            /*
            - Compared to previous versions, RenderTexture objects/assets here has been removed
                - The original intended usecase was for blitting a RenderTexture drawn by a secondary camera to the screen. Both color and depth buffers are needed for camera "Output Texture" field or it throws a warning :
                    "In the render graph API, the output Render Texture must have a depth buffer. When you select a Render Texture in any camera's Output Texture property, the Depth Stencil Format property of the texture must be set to a value other than None."
                - Tried using RTHandles.Alloc(renderTexture) (& release in Dispose) into renderGraph.ImportTexture(). However if it has both color & depth, it errors.
                    "Exception: Invalid imported texture. Both a color and a depthStencil format are provided. The texture needs to either have a color format or a depth stencil format."
                - It seems sampling the RenderTexture directly in shader/material still works if needed. (I'd guess "importing" isn't important if the resource isn't handled by RenderGraph anyway? Not sure though.)
            */

            [Header("Destination")]
            public Destination dstType = Destination.CameraColor;
            [ShowIf("dstType", Destination.CameraColor)]
            public bool showInSceneView = true;
            [ShowIf("dstType", Destination.GlobalTexture)]
            [Indent] public string dstGlobalTexture = "_BlitPassTexture";
            public enum FormatMode { SameAsCamera, CameraFormatWithAlpha, GraphicsFormat }
            public FormatMode colorFormat;
            [ShowIf("colorFormat", FormatMode.GraphicsFormat)]
            [Indent] public GraphicsFormat format;
            public bool bindDepthStencilBuffer;

            [Header("Material")]
            public Material blitMaterial;
            [ShowIf("blitMaterial")]
            public int blitPassIndex;

        }

        public FeatureSettings settings;

        class BlitPass : ScriptableRenderPass {

            private FeatureSettings settings;
            private int dstGlobalTextureID;
            private int[] globalTextures;

            public BlitPass(FeatureSettings settings) {
                this.settings = settings;
                dstGlobalTextureID = Shader.PropertyToID(settings.dstGlobalTexture);

                int len = settings.globalTextures.Length;
                globalTextures = new int[len];
                for (int i = 0; i < len; i++) {
                    globalTextures[i] = Shader.PropertyToID(settings.globalTextures[i]);
                }

                requiresIntermediateTexture = true; // Ensures resourceData.cameraColor is created, as using the back buffer as an input isn't supported.
            }

            private class BlitPassData {
                internal TextureHandle source;
                internal Material material;
                internal int passIndex;
            }

            static void ExecuteBlitPass(BlitPassData data, RasterGraphContext context) {
                if (data.material == null) {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                } else {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, data.passIndex);
                }
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                TextureHandle cameraTex = resourceData.cameraColor;
                if (!cameraTex.IsValid()) return;

                using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>("Blit Pass", out var passData)) {
                    // Init PassData
                    passData.material = settings.blitMaterial;
                    passData.passIndex = settings.blitPassIndex;
                    passData.source = cameraTex;

                    // Declare Inputs
                    builder.UseTexture(passData.source);
                    /*
                    Assumes cameraColor is in use, even if shader doesn't sample it.
                    Somewhat a workaround as Blitter API apparently requires a source input or it errors.
                    Using a DrawProcedural instead could probably avoid this.
                    */

                    for (int i = 0; i < globalTextures.Length; i++) {
                        try
                        {
                            builder.UseGlobalTexture(globalTextures[i]);
                        }
                        catch(ArgumentException //e
                        ) {
                            /*
                            UseGlobalTexture will cause errors and break rendering if texture does not exist, which is a bit annoying.
                            Using this to fail silently instead.
                            
                            If you prefer, could at least expose as a warning :
                            */
                            //Debug.LogWarning("ArgumentException: " + e.Message + " (Global Texture '"+settings.globalTextures[i]+"')\n" + e.StackTrace);
                        }
                    }

                    if ((input & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None && resourceData.cameraOpaqueTexture.IsValid()) {
                        builder.UseTexture(resourceData.cameraOpaqueTexture);
                    }
                    if ((input & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None) {
                        Debug.Assert(resourceData.cameraDepthTexture.IsValid());
                        builder.UseTexture(resourceData.cameraDepthTexture);
                    }
                    if ((input & ScriptableRenderPassInput.Motion) != ScriptableRenderPassInput.None) {
                        Debug.Assert(resourceData.motionVectorColor.IsValid());
                        builder.UseTexture(resourceData.motionVectorColor);
                        Debug.Assert(resourceData.motionVectorDepth.IsValid());
                        builder.UseTexture(resourceData.motionVectorDepth);
                    }
                    if ((input & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None) {
                        Debug.Assert(resourceData.cameraNormalsTexture.IsValid());
                        builder.UseTexture(resourceData.cameraNormalsTexture);
                    }

                    // Create Offscreen Texture
                    var desc = renderGraph.GetTextureDesc(resourceData.cameraColor);
                    desc.depthBufferBits = 0;
                    if (settings.colorFormat == FeatureSettings.FormatMode.CameraFormatWithAlpha) {
                        desc.format = GraphicsFormatUtility.ConvertToAlphaFormat(desc.format);
                    } else if (settings.colorFormat == FeatureSettings.FormatMode.GraphicsFormat) {
                        GraphicsFormat compatibleFormat = SystemInfo.GetCompatibleFormat(settings.format, GraphicsFormatUsage.Render);
                        if (compatibleFormat != GraphicsFormat.None)
                            desc.format = compatibleFormat;
                    }
                    desc.name = settings.dstType == FeatureSettings.Destination.GlobalTexture ? settings.dstGlobalTexture : "_CameraColorFullScreenPass";
                    desc.clearBuffer = false;
                    TextureHandle customTex = renderGraph.CreateTexture(desc);
                    if (!customTex.IsValid()) return;

                    // Set Render Target
                    builder.SetRenderAttachment(customTex, 0);
                    if (settings.bindDepthStencilBuffer) {
                        builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
                    }

                    // Output Global Texture
                    if (settings.dstType == FeatureSettings.Destination.GlobalTexture) {
                        builder.SetGlobalTextureAfterPass(customTex, dstGlobalTextureID);
                    }

                    // Assign ExecutePass
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((BlitPassData data, RasterGraphContext context) => ExecuteBlitPass(data, context));

                    if (settings.dstType == FeatureSettings.Destination.CameraColor) {
                        // Swap Camera Texture
                        resourceData.cameraColor = customTex;
                    }
                }
            }
        }

        private BlitPass m_ScriptablePass;

        public override void Create() {
            settings ??= new();
            if (settings.blitMaterial != null) {
                settings.blitPassIndex = Math.Clamp(settings.blitPassIndex, -1, settings.blitMaterial.passCount - 1);
            }
            m_ScriptablePass = new BlitPass(settings) {
                renderPassEvent = settings.injectionPoint
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (renderingData.cameraData.isPreviewCamera) return;
            if (settings.dstType == FeatureSettings.Destination.CameraColor && !settings.showInSceneView && renderingData.cameraData.isSceneViewCamera) return;

            m_ScriptablePass.ConfigureInput((ScriptableRenderPassInput)settings.requirements);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}