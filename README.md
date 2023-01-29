## Blit Renderer Feature
- Used to apply fullscreen image effects to camera (or other source/destination) using a shader/material.
- Tested with **2022.1.20f1 / URP v13.1.8**. For older versions see branches on github.
- Untested with Single-Pass Instanced VR, but uses Blitter API so *should* support it?

### Issues/Info :
- Requires a specific vertex shader & uses `_BlitTexture` (not `_MainTex`)! See BlitExample.shader for example.
  - In 2022.1+, cannot use Shader Graphs with this.
  - In 2022.2+, can use the Fullscreen graph type & `URP Sample Buffer` node to obtain BlitSource texture. Note that 2022.2 also provides it's own "Fullscreen Pass Renderer Feature" which could replace this feature if you only need camera destination.
- Source : TextureID no longer provides string field to enter ID like in previous versions. But should be able to sample that global texture ID in the shader instead.
- Don't think After Post Processing or After Rendering events work correctly. Not sure how to fix this.

### Setup:
- Install via Package Manager → Add package via git URL : 
  - `https://github.com/Cyanilux/URP_BlitRenderFeature.git#2022.1`
- Alternatively, download and put the folder in your Assets

### Usage :
- Adds "Blit" option to Renderer Features list on Forward/Universal asset (and 2D Renderer if in 2021.2+)
- Requires specific shader (See BlitExample.shader). (Can use Fullscreen graph if in 2022.2+)
- Feature allows specific access to selecting the **source** and **destination** targets (via **Camera**, **TextureID** or **Render Texture** object)
- Some usage examples :
  - Could be used with Camera for both source/destination to apply an shader/material as an image effect / post process
  - Could be used to copy the camera source to a TextureID. Similar to what the Opaque Texture / Scene Color does. (TextureID destination is automatically allocated by feature, and should also set it as a global texture so it can be obtained in shaders rendered later. May want to avoid using IDs that already exist)
  - Could be used with Render Texture object source (rendered by a second camera) and Camera destination to apply it to the Main Camera
 
### Additional Features :
- Option to set an `_InverseView` matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world. For example, reconstructing world position from depth.
- Enabling generation of Camera Normals Texture (DepthNormals pass)

### Author / Sources :
- [@Cyanilux](https://twitter.com/Cyanilux)
- https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@15.0/manual/renderer-features/how-to-fullscreen-blit.html
- https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@13.1/manual/rthandle-system-using.html
