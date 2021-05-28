## Blit Renderer Feature
Tested with **2019.3 / URP v8.2** and **2020.3 / URP v10.3**.
<br /><br />
Extended to allow options for :<br />
• Specific access to selecting a **source** and **destination** (via current camera's color / texture id / render texture object)<br />
• Automatic switching to using **_AfterPostProcessTexture** for **After Rendering** event, in order to correctly handle the blit after post processing is applied<br />
• Setting a **_InverseView** matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. For example, reconstructing world position from depth, see : https://twitter.com/Cyanilux/status/1269353975058501636<br />
• (URP v10) Enabling generation of DepthNormals (_CameraNormalsTexture)
<br /><br />
Based on the Blit (now renamed to FullscreenFeature/Pass) from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses)
<br /><br />
@Cyanilux<br />
:)
