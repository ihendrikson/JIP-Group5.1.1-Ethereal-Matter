// Script that controls the visualization of the HPE model
// Made by JIP team: Ethereal Matter 
//  25/10/2024 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.BlazePose;
using System;

public class PoseVisuallizer3D : MonoBehaviour
{
    // Setup variables for the ORBBEC camera 
    [SerializeField] Camera mainCamera;
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage inputImageUI;
    [SerializeField] Shader shader;
    [SerializeField, Range(0, 1)] float humanExistThreshold = 0.5f;

    // Reference to the pipeserver script
    public PipeServer server;

    // Connect with the Pointcloud script and script controller
    Pointcloud_nocolor Pointcloud;
    ScriptToggle Script_Controller; 

    // Flag to control visualization
    public bool Enable_HPE_visualization = true;

    // Set scaling factors for the HPE visualization placement
    private float ScaleX = 1.5f;
    private float ScaleY = 0.6f;
    private float ScaleZ = 1f;

    // Camera settings
    private float Horizontal_angle = 75f;
    private float Vertical_angle = 65f;

    // Lower limit of the depth 
    private float Mid_Z_limit = 0.2f;
    private float lastknownz = 0f;

    //Dictionary for all landmark positions
    public Dictionary<string, Vector3> vector3Dictionary = new Dictionary<string, Vector3>();

    // Variables needed for the HPE model
    public Boolean flip_y;
    public Vector3 StaticTranform = new Vector3(0, 0, 0);
    Material material;
    BlazePoseDetecter detecter;
    private Vector3[] mapperJoints = new Vector3[33];
    const int BODY_LINE_NUM = 35;

    // Vectors 
    readonly List<Vector4> linePair = new List<Vector4>{
        new Vector4(11, 12),
        new Vector4(11, 13), new Vector4(13, 15), new Vector4(12, 14), new Vector4(14, 16), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24),
        new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27),
        new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28),
        new Vector4(11, 24), new Vector4(12, 23) // Cross as spine
    };

    // Exlusions of certain landmarks like the hands and feet
    float[] exclusionIndices = new float[16] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 18, 20, 22, 17, 19, 21};  
    int exclusionCount = 16;  

    void Start(){
        //Set materials for the HPE estimation
        material = new Material(shader);
        material.SetFloatArray("exclusionIndices", exclusionIndices);
        material.SetInt("exclusionCount", exclusionCount);
        material.SetVector("_predefinedTranslation", StaticTranform);
        
        // Set the HPE model to BlazePoseDetectors
        detecter = new BlazePoseDetecter();

        // Get the needed public variables from the PointCloud script 
        Pointcloud = FindObjectOfType<Pointcloud_nocolor>();
        Script_Controller = FindObjectOfType<ScriptToggle>();

    }

    void LateUpdate(){
        Texture texture = webCamInput.inputImageTexture;

        if (texture == null)
        {
            Debug.LogWarning("inputImageTexture is null in LateUpdate.");
            return; // Skip further processing if texture is not available
        }

        //inputImageUI.texture = texture;

        // Predict pose by neural network model.
        detecter.ProcessImage(texture);

        //Set origin point of hip based on 2D RGB image
        float Depthbuffer = 0f;

        // Choose the correct depth data to use during the calculation of positions
        if (Pointcloud.MidHip_Z > Mid_Z_limit && Pointcloud.enabled == true)
        {
            Depthbuffer = Pointcloud.MidHip_Z;
            lastknownz = Depthbuffer;
        }
        else if(Pointcloud.enabled == true) 
        {
            Depthbuffer = Pointcloud.average_z;
            lastknownz = Depthbuffer;
        }
        else 
        {
            Depthbuffer = lastknownz;
        }
        
        //Calculation of all the positions of the landmarks based on the depht 
        // Right part 
        vector3Dictionary["Right Ankle"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(28), Depthbuffer);
        vector3Dictionary["Right Knee"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(26), Depthbuffer);
        vector3Dictionary["Right Hip"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(24), Depthbuffer);
        vector3Dictionary["Right Shoulder"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(12), Depthbuffer);
        vector3Dictionary["Right Elbow"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(14), Depthbuffer);
        vector3Dictionary["Right Wrist"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(16), Depthbuffer);

        // Left part
        vector3Dictionary["Left Ankle"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(27), Depthbuffer);
        vector3Dictionary["Left Knee"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(25), Depthbuffer);
        vector3Dictionary["Left Hip"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(23), Depthbuffer);
        vector3Dictionary["Left Shoulder"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(11), Depthbuffer);
        vector3Dictionary["Left Elbow"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(13), Depthbuffer);
        vector3Dictionary["Left Wrist"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(15), Depthbuffer);

        // Vitual parts
        vector3Dictionary["Center Hip"] = Calculate_Position(((Vector3)detecter.GetPoseLandmark(23) + (Vector3)detecter.GetPoseLandmark(24)) / 2f, Depthbuffer);
        vector3Dictionary["Head"] = Calculate_Position((Vector3)detecter.GetPoseLandmark(0), Depthbuffer);
        vector3Dictionary["Center Shoulder"] = Calculate_Position(((Vector3)detecter.GetPoseLandmark(11) + (Vector3)detecter.GetPoseLandmark(12)) / 2f, Depthbuffer);
        vector3Dictionary["Spine 1"] = (3*vector3Dictionary["Center Hip"] +  vector3Dictionary["Center Shoulder"])/4f;
        vector3Dictionary["Spine 2"] = (vector3Dictionary["Center Hip"] +  vector3Dictionary["Center Shoulder"])/2f;


        // Output landmark values(33 values) and the score whether human pose is visible (1 values).
        for(int i = 0; i < detecter.vertexCount; i++){
            mapperJoints[i] = (Vector3)detecter.GetPoseWorldLandmark(i);
            if (flip_y)
            {
                mapperJoints[i].y = -mapperJoints[i].y;
            }
            mapperJoints[i] = mapperJoints[i] + new Vector3(0f,0f,5f);
        }
        server.joints = mapperJoints;
    } 

    // Function to calculate the position of landmarks based on the depth and FOV of ORBBEC camera 
    Vector3 Calculate_Position(Vector3 Origin, float depthdata)
    {
        Origin = Origin + new Vector3(-0.5f, -0.5f, depthdata);
        Origin = Vector3.Scale(Origin, new Vector3(2 * depthdata * Mathf.Tan(Vertical_angle / 2 * Mathf.Deg2Rad) * ScaleX, 2 * depthdata * Mathf.Tan(Horizontal_angle / 2 * Mathf.Deg2Rad) * ScaleY, 1f * ScaleZ));
        return Origin;
    }


    void OnRenderObject(){
        // Use predicted pose world landmark results on the ComputeBuffer (GPU) memory.
        material.SetBuffer("_worldVertices", detecter.worldLandmarkBuffer);
        
        // Set pose landmark counts.
        material.SetInt("_keypointCount", detecter.vertexCount);
        material.SetFloat("_humanExistThreshold", humanExistThreshold);
        material.SetVectorArray("_linePair", linePair);
        material.SetMatrix("_invViewMatrix", mainCamera.worldToCameraMatrix.inverse);

        // Set Hip position equal to the center position of the pointcloud
        if(vector3Dictionary.ContainsKey("Center Hip"))
        {
            material.SetVector("_predefinedTranslation", vector3Dictionary["Center Hip"]);
        }
        

        if(Script_Controller.HPE_visuallizer == true)
        {
            // Draw 35 world body topology lines.
            material.SetPass(2);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

            // Draw 17 world landmark points.
            material.SetPass(3);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, detecter.vertexCount);
        }

    }

    void OnApplicationQuit(){
        // Must call Dispose method when no longer in use.
        detecter.Dispose();
    }
}
