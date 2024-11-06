# JIP-Group5.1.1-Ethereal-Matter
This repository is for work done to complete the Joint Interdisciplinary Project. For this part, we worked in a team of Martijn, Yuke, Jelte and Ivar in order to help Ethereal Matter further develop their product: the immersive VR fitness experience.

Code within this repository is for the Unity game engine and mostly involves Human Pose Estimation, Avatar rigging, Point cloud, Safety zone, Motion analysis and Avatar customization. The main goal is to provide a more immersive and personalized experience for the user while keeping the machine safe to use.

## Usage
The Unity scene is all in the file “3DSampleScene”, and it is very easy to use this scene. There are already some objects in the Hierarchy
(Image)

### Main Camera
(Image)
- Camera: Set the Field of View and Clipping Plane
- Preview Camera: Track the test model
- Orbbec Pipeline: Ensure the Orbbec use the correct Orbbec Profiles settings
- Orbbec Device: Ensure Orbbec device is successfully connected
- Pointcloud_nocolor: Render the point cloud, also control the depth data, calibration and safetyzone data
- Script Toggle: Switch between different models

- **Webcaminput**
  (image)
- **Visualizer**
  (image)
- **Pipeserver**
  (image)
- **Cylinder**
  (image)
- **Canvas**
  (image)
- Slider: Control the size of different joints and color of test model
- Button: Switch between different skins and different sky environment
- Keys: Control the function of Point cloud, safety zone, avatar and HPE model.
- **Banana man**
  (image)
- Part Scalar: Control the model size
- Skin Changer: Control the skin
- Safety Zone: Show the safety zone
- Move Object: Calibrate the HPE, reset the center
- Angle Squat: Motion analysis to ensure guidance in promoting the correct form
- Avatar: Bind the Pose to arbitrary humanoid avatars

### Notes
1. When entering the play mode, first there’s a 3s to do the calibration to move all the background, so ensure the user is not in the background. (The calibration time can also be adjusted in the pointcloud component). If it works properly, there should be a message in the console.

2. If want to show the safety zone, please follow these steps:
   - Make sure the calibration(move the background) works
   - Keep the point cloud open
   - Let the user turn to the side view
   - When the hip point is in the middle of the  point cloud in the same horizontal plane, press space.
   - There should be a message in the console showing the calibrated depth
3.  It’s also possible to import some new characters, you just need to add the same components as in the existing model and record a new calibration data in the scene called “calibration scene”, How to do the calibration you may find this video helpful(https://www.youtube.com/watch?v=DFHDnALoiQE).
4. Remember if you want to use the terrain environment please first follow the steps:
   - Navigate to Project Settings > Graphics and point the Scriptable Render Pipeline Settings field to URP-HighQuality Pipeline Asset.
   - Navigate to Project Settings > Graphics > URP Global Settings, and pick the UniversalRenderPipelineGlobalSettings file located in Assets > TerrainDemoScene_URP > Settings > URP folder.
   - Make sure the shader of all the materials you use like the skins should be the same with the render pipeline settings.
5. Any new skin materials should be stored under the folder Resources/Materials/Skinstyles and also check the path in the “Skin changer” component, same for the skybox.
