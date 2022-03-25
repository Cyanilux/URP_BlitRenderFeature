## Blit Renderer Feature
- Used to apply fullscreen image effects to camera (or other source/destination) using a shader/material.
- Tested with **2021.2.5f1 / URP v12.1.2**. Should still work with older versions, otherwise see branches for 2019-2021.1 version.
- While still named "Blit", this branch actually uses CommandBuffer.DrawMesh, as an attempt to support Single-Pass Instanced VR. (Open an issue if there is any problems). If you aren't working in VR you can alternatively use the master branch to continue with CommandBuffer.Blit.

### Setup:
- Install via Package Manager → Add package via git URL : 
  - `https://github.com/Cyanilux/URP_BlitRenderFeature.git#cmd-drawMesh` (sorry, package is not set up yet!)
- Alternatively, download and put the folder in your Assets
- **Note : You must put the `BlitDirectly.shader` in the 'Always Included Shaders' section under Project Settings → Graphics for this to work in builds!**

### Usage :
- Adds "Blit" option to Renderer Features list on Forward/Universal asset (and 2D Renderer if in 2021.2+)
- The shader/material used should sample **global texture** `_MainTex` to obtain source.
  - For Shader Graphs, there is a provided `Sample Main Texture` subgraph to handle this automatically (required to support Single Pass Instanced as it needs to use `TEXTURE2D_X` macros). Do not need to create the property in the blackboard as this will cause a redefinition error if using the subgraph.
  - For Shader Graphs and written shaders that do View/Projection transformations to the clipspace output (e.g. using `TransformObjectToHClip`), make sure to enable the **Override View Projection** option on the feature so the quad is drawn to the screen correctly. For other written shaders which output vertices directly in object space (e.g. see BlitDirectly.shader for example) you can untick this which may make the feature slightly cheaper.
  - To avoid editing parts of the screen with the VR occlusion mesh can use `ZTest NotEqual` and output `UNITY_NEAR_CLIP_VALUE` as the clipspace z coordinate. See the BlitDirectly.shader for an example.
- Feature allows specific access to selecting the **source** and **destination** targets (via **Camera**, **TextureID** or **Render Texture** object)
- Some usage examples :
  - Could be used with Camera for both source/destination to apply an shader/material as an image effect / post process
  - Could be used to copy the camera source to a TextureID. Similar to what the Opaque Texture / Scene Color does. (When TextureID is used in destination it automatically creates a Temporary Render Texture, and should also set it as a global texture so it can be obtained in shaders rendered later. May want to avoid using IDs that already exist)
  - Could be used with Render Texture object source (rendered by a second camera) and Camera destination to apply it to the Main Camera
 
### Additional Features :
- Option to set an `_InverseView` matrix (`_InverseViewStereo` while in Single Pass Instanced). Use the provided `Get Inverse View Matrix` subgraph to obtain this. For written shaders see FullscreenHelperFunctions.hlsl. This is mostly for shaders that need to reconstruct world position from the depth texture, see Examples folder.
- (2020.2/v10+) Enabling generation of DepthNormals `(_CameraNormalsTexture)`

### Author / Sources :
- [@Cyanilux](https://twitter.com/Cyanilux)
- Based on the Blit (now renamed to FullscreenFeature/Pass) from the [UniversalRenderingExamples](https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses)
- DrawMesh changes and BlitDirectly.shader based on [Blit in XR](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.1/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html) page from URP docs
