## Blit Renderer Feature
- Used to apply fullscreen image effects to camera (or other source/destination) using a shader/material.
- Tested with **2021.2.5f1 / URP v12.1.2**. Should still work with older versions, otherwise see branches for 2019-2021.1 version.
- This version uses CommandBuffer.Blit so will not work in Single-Pass Instanced VR. Try using [cmd-DrawMesh](https://github.com/Cyanilux/URP_BlitRenderFeature/tree/cmd-drawMesh) version instead.

### Setup:
- Install via Package Manager → Add package via git URL : 
  - `https://github.com/Cyanilux/URP_BlitRenderFeature.git`
- Alternatively, download and put the folder in your Assets

### Usage :
- Adds "Blit" option to Renderer Features list on Forward/Universal asset (and 2D Renderer if in 2021.2+)
- The shader/material used should sample `_MainTex` to obtain source. Will work with Shader Graphs too (as long as you set the texture Reference in the Blackboard / Node Settings)
- Feature allows specific access to selecting the **source** and **destination** targets (via **Camera**, **TextureID** or **Render Texture** object)
- Some usage examples :
  - Could be used with Camera for both source/destination to apply an shader/material as an image effect / post process
  - Could be used to copy the camera source to a TextureID. Similar to what the Opaque Texture / Scene Color does. (When TextureID is used in destination it automatically creates a Temporary Render Texture, and should also set it as a global texture so it can be obtained in shaders rendered later. May want to avoid using IDs that already exist)
  - Could be used with Render Texture object source (rendered by a second camera) and Camera destination to apply it to the Main Camera
 
### Additional Features :
- Option to set an `_InverseView` matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. For example, reconstructing world position from depth, see : https://twitter.com/Cyanilux/status/1269353975058501636
- (2020.2/v10+) Enabling generation of DepthNormals `(_CameraNormalsTexture)`

### Author / Sources :
- [@Cyanilux](https://twitter.com/Cyanilux)
- Based on the Blit (now renamed to FullscreenFeature/Pass) from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses)
