## Blit Renderer Feature
Tested with **2021.2.5f1 / URP v12.1.2**. Should still work with older versions, otherwise see branches for 2019-2021.1 version.
<br /><br />
**Note : You must put the `BlitDirectly.shader` in the 'Always Included Shaders' section under Project Settings → Graphics for this to work in builds!**
<br /><br />
For blit materials, the shader should use a global texture `_MainTex` to recieve source. If you use Shader Graph, make sure to change **Reference** (not just name) and **untick Exposed**! For shader code, see BlitDirectly.shader as an example. If outputting directly in clip space, you can untick the "Override View Projection" on the feature.
<br /><br />
Extended to allow options for :<br />
• Specific access to selecting a **source** and **destination** (via current camera's color / texture id / render texture object)<br />
• Now uses **cmd.DrawMesh** with fullscreen quad instead of cmd.Blit to avoid issues with single pass instanced VR/XR (but not fully tested)
	(Note : Should also use TEXTURE2D_X and SAMPLE_TEXTURE2D_X to properly support both eyes, BlitDirectly.shader provides an example)
• Option to set an **_InverseView** matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. For example, reconstructing world position from depth, see : https://twitter.com/Cyanilux/status/1269353975058501636<br />
• (2021.1/v10+) Option to enable generation of DepthNormals (_CameraNormalsTexture)
<br /><br />
Based on the Blit (now renamed to FullscreenFeature/Pass) from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses) and [Blit in XR](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.1/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html) page from URP docs.
<br /><br />
@Cyanilux<br />
:)
