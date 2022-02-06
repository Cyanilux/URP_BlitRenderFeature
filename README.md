## Blit Renderer Feature
- Tested with **2021.2.5f1 / URP v12.1.2**. Should still work with older versions, otherwise see branches for 2019-2021.1 version.
- Based on the Blit (now renamed to FullscreenFeature/Pass) from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses)

### Setup:
- Install via Package Manager → Add package via git URL : `https://github.com/Cyanilux/URP_BlitRenderFeature.git`
- Alternatively, download and put the folder in your Assets

### Features :
- Applies fullscreen effects via shader/material
- Specific access to selecting the **source** and **destination** targets (via Camera, TextureID or Render Texture object)<br />
- Setting an **_InverseView** matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. For example, reconstructing world position from depth, see : https://twitter.com/Cyanilux/status/1269353975058501636<br />
- (2021.1/v10+) Enabling generation of DepthNormals (_CameraNormalsTexture)

<br /><br />
@Cyanilux<br />
:)