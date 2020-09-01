## Blit Renderer Feature
Based on the Blit from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses)<br />
<br />
Extended to allow options for :<br />
• Specific access to selecting a source and destination (via current camera's color / texture id / render texture object<br />
• Automatic switching to using _AfterPostProcessTexture for After Rendering event, in order to correctly handle the blit after post processing is applied<br />
• Setting a _InverseView matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. e.g. reconstruct world pos from depth : https://twitter.com/Cyanilux/status/1269353975058501636<br />
<br />
@Cyanilux<br />
:)
