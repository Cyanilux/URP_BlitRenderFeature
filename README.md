## Blit Renderer Feature
Tested with **2021.2.5f1 / URP v12.1.2**. Should still work with older versions, otherwise see branches for 2019-2021.1 version.
<br /><br />
Extended to allow options for :<br />
• Specific access to selecting a **source** and **destination** (via current camera's color / texture id / render texture object)<br />
• Setting a **_InverseView** matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. For example, reconstructing world position from depth, see : https://twitter.com/Cyanilux/status/1269353975058501636<br />
• (2021.1/v10+) Enabling generation of DepthNormals (_CameraNormalsTexture)
<br /><br />
Based on the Blit (now renamed to FullscreenFeature/Pass) from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses)
<br /><br />
@Cyanilux<br />
:)
