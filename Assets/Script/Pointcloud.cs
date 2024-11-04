// Script that controls the Pointcloud and components, for example the depht data, calibration and safetyzone data 
// Made by JIP team: Ethereal Matter 
//  25/10/2024 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Orbbec;
using OrbbecUnity;
using System.Linq;

public class Pointcloud_nocolor : MonoBehaviour
{
    // Pipeline variables 
    public OrbbecPipeline pipeline;
    private PointCloudFilter filter;
    private Format format;

    // Connection to the Posevisualizer script 
    PoseVisuallizer3D Greenman;

    // Create Dictionary for storing the data that will be accessible to the Safetyzone_script 
    public Dictionary<string, float> SafetyDictionary = new Dictionary<string, float>();
    
    // Variables that are neede to set scale 
    public float scale;
    public float average_z;
    public float calibrationtime;
    public float MidHip_Z;

    // Set offset for depth
    private float Depth_offset = 0f;
    private float Buffer_MidHip_Z = 1f;
    public float Depth_correction;

    // Variables needed for the calibration of the Point cloud
    private float[] thirdSecondDepthData;  
    private float elapsedTime = 0f;  
    public float depthThreshold = 0.1f;

    // Variable to store the depth data form the ORBBEC depth sensor 
    private byte[] data;

    // Flags to indicate statuses 
    private bool thirdSecondCaptured = false; 
    private bool newPointCloudDataReady = false;
    private bool Calibrate_z = false;
    private bool safetyzone = false;
    public bool Visualize_safetyzone = false;

    // Variables to store and generate the meshes to visualize the pointcloud
    private List<GameObject> meshObjects = new List<GameObject>();  // Store generated Mesh object
    private List<Mesh> pointCloudMeshes = new List<Mesh>();  // Store point cloud meshes
    private const int MAX_VERTICES_PER_MESH = 65535;  // Maximum number of vertices per Mesh

    void Start()
    {
        // Initialize the Orbbec pipeline and set up the callback
        pipeline.SetFramesetCallback(OnFrameset);
        pipeline.onPipelineInit.AddListener(() =>
        {
            pipeline.Config.SetAlignMode(AlignMode.ALIGN_D2C_SW_MODE);
            pipeline.StartPipeline();
            Debug.Log("get pipeline");
            filter = new PointCloudFilter();
            Debug.Log("get parameters");
            filter.SetCameraParam(pipeline.Pipeline.GetCameraParam());

        });


        // Adjust the camera position to view the PointCloud
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.transform.LookAt(Vector3.zero);
        Camera.main.nearClipPlane = 0.01f;
        Camera.main.farClipPlane = 100f;

        // Get the needed public variables from the PoseVisualizer script
        Greenman = FindObjectOfType<PoseVisuallizer3D>(); 
    }

    void Update()
    {
        // Update the timer used to activate calibration in x seconds
        elapsedTime += Time.deltaTime;

        // Activate certain parts of the script based on key inputs 
        if (Input.GetKeyDown(KeyCode.Space)) // Activate the capturing of the thiccness of the user by setting the flag true
        {
            if (thirdSecondCaptured == true)
            {
                Calibrate_z = true;
                Debug.Log("Capture human depth");
            } 
        }

        if (Input.GetKeyDown(KeyCode.Z))  // Measure the safetyzone by setting the flag true
        {
            if (thirdSecondCaptured == true)
            {
                safetyzone = true;
                Debug.Log("Generate safety zone");
            }
        } 
        // If new point cloud data is available, render the point cloud
        if (newPointCloudDataReady)
        {
            RenderPointCloud(data);
            newPointCloudDataReady = false;
        }
    }

    private void OnFrameset(Frameset frameset)
    {

        format = Format.OB_FORMAT_POINT; // Set ORBBEC format to point cloud with no colour data 
        filter.SetPointFormat(format);

        if (frameset == null) return;

        DepthFrame depthFrame = frameset.GetDepthFrame(); // Get Depth data from the ORBBEC

        if (depthFrame == null) // Check if there is data inside the depthframe
        {
            Debug.LogWarning("Depth frame is empty");
            return;
        }

        filter.SetPositionDataScaled(depthFrame.GetValueScale());
        var frame = filter.Process(frameset);
        if (frame != null)
        {
            var pointFrame = frame.As<PointsFrame>();
            var dataSize = pointFrame.GetDataSize();

            if (data == null || data.Length != dataSize)
            {
                data = new byte[dataSize];
            }

            pointFrame.CopyData(ref data);
            newPointCloudDataReady = true;

            // Capture depth data only at the 3rd second
            if (!thirdSecondCaptured && elapsedTime >= calibrationtime)
            {
                SaveThirdSecondDepthData(data);  // Save the depth data at the 3rd second
                thirdSecondCaptured = true;  // Mark that the depth data at the 3rd second has been captured
                Debug.Log("Captured depth data at the 3rd second.");
            }
            pointFrame.Dispose();
            frame.Dispose();
        }
        frameset.Dispose();
    }

    // Save the depth data at the 3rd second 
    private void SaveThirdSecondDepthData(byte[] data)
    {
        int pointSize = Marshal.SizeOf(typeof(Point));
        int pointsSize = data.Length / Marshal.SizeOf(typeof(Point));
        thirdSecondDepthData = new float[pointsSize];

        IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, dataPtr, data.Length);

        for (int i = 0; i < pointsSize; i++)
        {
            IntPtr pointPtr = new IntPtr(dataPtr.ToInt64() + i * pointSize);
            Point point = Marshal.PtrToStructure<Point>(pointPtr);
            thirdSecondDepthData[i] = point.z * scale;  // Store each point's depth value
        }

        Marshal.FreeHGlobal(dataPtr);
    }

    // Calculate the closest data point from the Pointcloud compared to the HPE model 
    void UpdateClosestPoint(ref Vector3 closestPoint, ref float minDistance, Point point, Vector3 target, float scale)
    {
        float distance = Mathf.Sqrt(Mathf.Pow(point.x * scale - target.x, 2) + 
                                    Mathf.Pow(-point.y * scale - target.y, 2));

        if (distance < minDistance)
        {
            minDistance = distance;
            closestPoint.x = point.x * scale;
            closestPoint.y = point.y * scale;
            closestPoint.z = point.z * scale;
        }
    }

    // Calculate the thickness
    void CalculateThickness(String lower, String upper, Vector3 Depth, Dictionary<string, Vector3> vector3D , ref Dictionary<string, float> SafetyDictionary)
    {
        SafetyDictionary[lower + ";" + upper] = Math.Abs(((vector3D[lower] + vector3D[upper])/2 - Depth).z);
    }

    // Render the point cloud and subtract the background data from the 3rd second
    private void RenderPointCloud(byte[] data)
    {
        int pointSize = Marshal.SizeOf(typeof(Point));
        int pointsSize = data.Length / Marshal.SizeOf(typeof(Point));
        Point[] points = new Point[pointsSize];

        Vector3[] vertices = new Vector3[pointsSize];
        int[] indices = new int[pointsSize];

        IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, dataPtr, data.Length);

        int validPointCount = 0;

        average_z = 0f;

        float min_distance_Hip = Mathf.Infinity;        
        Vector3 Hip = new Vector3();

        //Create variables the calculation of the closest data point from the Pointcloud compared to the HPE model 
        // Right part
        float min_distance_Rightlleg = Mathf.Infinity;        
        Vector3 Rightlleg = new Vector3();
        float min_distance_Rightuleg = Mathf.Infinity;        
        Vector3 Rightuleg = new Vector3();
        float min_distance_Rightlarm = Mathf.Infinity;        
        Vector3 Rightlarm = new Vector3();
        float min_distance_Rightuarm = Mathf.Infinity;        
        Vector3 Rightuarm = new Vector3();

        // Left part
        float min_distance_Leftlleg = Mathf.Infinity;        
        Vector3 Leftlleg = new Vector3();
        float min_distance_Leftuleg = Mathf.Infinity;        
        Vector3 Leftuleg = new Vector3();
        float min_distance_Leftlarm = Mathf.Infinity;        
        Vector3 Leftlarm = new Vector3();
        float min_distance_Leftuarm = Mathf.Infinity;        
        Vector3 Leftuarm = new Vector3();

        // Mid part
        float min_distance_lspine = Mathf.Infinity;        
        Vector3 lspine = new Vector3();
        float min_distance_mspine = Mathf.Infinity;        
        Vector3 mspine = new Vector3();
        float min_distance_uspine = Mathf.Infinity;        
        Vector3 uspine = new Vector3();



        // Iterate through each point and check the depth difference
        for (int i = 0; i < pointsSize; i++)
        {
            IntPtr pointPtr = new IntPtr(dataPtr.ToInt64() + i * pointSize);
            points[i] = Marshal.PtrToStructure<Point>(pointPtr);
            
            // Calculation of the average z 
            average_z += points[i].z * scale;

            // Calibrate for certain depth
            float currentDepth = points[i].z * scale;
            float backgroundDepth = thirdSecondDepthData != null && i < thirdSecondDepthData.Length ? thirdSecondDepthData[i] : 0f;

            // Keep the point if the depth difference exceeds the threshold
            if (Mathf.Abs(currentDepth - backgroundDepth) > depthThreshold)
            {
                vertices[validPointCount] = new Vector3(points[i].x * scale, -points[i].y * scale, points[i].z * scale - Depth_correction);
                indices[validPointCount] = validPointCount;
                validPointCount++;

                //Get the center point of the Hip to set the HPE model in line with the pointcloud
                if(Greenman.enabled){
                    UpdateClosestPoint(ref Hip, ref min_distance_Hip, points[i], Greenman.vector3Dictionary["Center Hip"], scale);
                }
                
                // Create the data for the safetyzone
                if(safetyzone == true && Greenman.enabled == true)
                {
                    // Right part thickness
                    UpdateClosestPoint(ref Rightlleg, ref min_distance_Rightlleg, points[i], (Greenman.vector3Dictionary["Right Ankle"] + Greenman.vector3Dictionary["Right Knee"])/2, scale);
                    UpdateClosestPoint(ref Rightuleg, ref min_distance_Rightuleg, points[i], (Greenman.vector3Dictionary["Right Hip"] + Greenman.vector3Dictionary["Right Knee"])/2, scale);
                    UpdateClosestPoint(ref Rightlarm, ref min_distance_Rightlarm, points[i], (Greenman.vector3Dictionary["Right Shoulder"] + Greenman.vector3Dictionary["Right Elbow"])/2, scale);
                    UpdateClosestPoint(ref Rightuarm, ref min_distance_Rightuarm, points[i], (Greenman.vector3Dictionary["Right Wrist"] + Greenman.vector3Dictionary["Right Elbow"])/2, scale);

                    // Left part thickness
                    UpdateClosestPoint(ref Leftlleg, ref min_distance_Leftlleg, points[i], (Greenman.vector3Dictionary["Left Ankle"] + Greenman.vector3Dictionary["Left Knee"])/2, scale);
                    UpdateClosestPoint(ref Leftuleg, ref min_distance_Leftuleg, points[i], (Greenman.vector3Dictionary["Left Hip"] + Greenman.vector3Dictionary["Left Knee"])/2, scale);
                    UpdateClosestPoint(ref Leftlarm, ref min_distance_Leftlarm, points[i], (Greenman.vector3Dictionary["Left Shoulder"] + Greenman.vector3Dictionary["Left Elbow"])/2, scale);
                    UpdateClosestPoint(ref Leftuarm, ref min_distance_Leftuarm, points[i], (Greenman.vector3Dictionary["Left Wrist"] + Greenman.vector3Dictionary["Left Elbow"])/2, scale);

                    // Middle part thickness
                    UpdateClosestPoint(ref lspine, ref min_distance_lspine, points[i], (Greenman.vector3Dictionary["Center Hip"] + Greenman.vector3Dictionary["Spine 1"])/2, scale);
                    UpdateClosestPoint(ref mspine, ref min_distance_mspine, points[i], (Greenman.vector3Dictionary["Spine 2"] + Greenman.vector3Dictionary["Spine 1"])/2, scale);
                    UpdateClosestPoint(ref uspine, ref min_distance_uspine, points[i], (Greenman.vector3Dictionary["Spine 2"] + Greenman.vector3Dictionary["Center Shoulder"])/2, scale);
                }
            }
        }

        //Set distance MidHip_z to corre
        MidHip_Z = Hip.z;

        // Set all the data from the safetyzone in the correct format
        if (safetyzone == true && Greenman.enabled == true)
        {
            // Right part thickness
            CalculateThickness("Right Ankle", "Right Knee", Rightlleg, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Right Knee", "Right Hip", Rightuleg, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Right Shoulder", "Right Elbow", Rightlarm, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Right Elbow", "Right Wrist", Rightuarm, Greenman.vector3Dictionary, ref SafetyDictionary);

            // Left part thickness
            CalculateThickness("Left Ankle", "Left Knee", Leftlleg, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Left Knee", "Left Hip", Leftuleg, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Left Shoulder", "Left Elbow", Leftlarm, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Left Elbow", "Left Wrist", Leftuarm, Greenman.vector3Dictionary, ref SafetyDictionary);

            // Mid part thickness
            CalculateThickness("Center Hip", "Spine 1", lspine, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Spine 1", "Spine 2", mspine, Greenman.vector3Dictionary, ref SafetyDictionary);
            CalculateThickness("Spine 2", "Center Shoulder", uspine, Greenman.vector3Dictionary, ref SafetyDictionary);
            safetyzone = false;
            Visualize_safetyzone = true; 

            //For debugging purposes
            //foreach (KeyValuePair<string, float> entry in SafetyDictionary)
            //{
            //    Debug.Log("Key: " + entry.Key + ", Value: " + entry.Value);
            //}

        }

        //Calibration of the depht of the center point of the hip to correct the depth of the HPE model
        if (Calibrate_z == true && Greenman.enabled == true)
        {
            List<float> xcoordinates = new List<float>();

            float? lowestX = null;
            float? highestX = null;

            foreach (var vector in vertices)
            {
                if (Mathf.Pow(vector.y - Hip.y, 2) < 0.01f)
                {
                    if (vector.x >= Hip.x - 0.5f && vector.x <= Hip.x + 0.5f)
                    {

                        // Update the lowest and highest x values
                        if (lowestX == null || vector.x < lowestX)
                        {
                            lowestX = vector.x;
                        }
                        if (highestX == null || vector.x > highestX)
                        {
                            highestX = vector.x;
                        }
                    }
                }
            }
            float? Depth_offset_buffer = highestX - lowestX;
            Depth_offset = Depth_offset_buffer ?? 0f;
            Debug.Log("Calibrated depht: " + Depth_offset);
            Calibrate_z = false;
        }

        MidHip_Z = MidHip_Z + Math.Abs(Depth_offset) / 2;
        average_z = average_z / pointsSize; //Finish the average_z calculation

    
        MidHip_Z = MidHip_Z * 0.5f + Buffer_MidHip_Z * 0.5f;
        Buffer_MidHip_Z = MidHip_Z;

        Marshal.FreeHGlobal(dataPtr);

        ClearPreviousMeshes();

        // Create the mesh for the pointcloud
        int numMeshes = Mathf.CeilToInt((float)validPointCount / MAX_VERTICES_PER_MESH); //Number of maximum points that can be in the pointcloud 
        for (int meshIndex = 0; meshIndex < numMeshes; meshIndex++)
        {
            // Devide the pointcloud in multiple meshes
            int start = meshIndex * MAX_VERTICES_PER_MESH;
            int end = Mathf.Min(start + MAX_VERTICES_PER_MESH, validPointCount);
            int meshVerticesCount = end - start;

            // Create the vertices andin
            Vector3[] meshVertices = new Vector3[meshVerticesCount];
            int[] meshIndices = new int[meshVerticesCount];

            Array.Copy(vertices, start, meshVertices, 0, meshVerticesCount);
            for (int i = 0; i < meshVerticesCount; i++)
            {
                meshIndices[i] = i;
            }

        GameObject meshObject;
        if (meshIndex < meshObjects.Count)
        {
            meshObject = meshObjects[meshIndex];
        }
        else
        {
            meshObject = new GameObject("PointCloudMesh_" + meshIndex);
            meshObject.transform.SetParent(transform);
            meshObjects.Add(meshObject);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.SetIndices(meshIndices, MeshTopology.Points, 0);
        mesh.RecalculateBounds();

        MeshFilter filter = meshObject.GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = meshObject.AddComponent<MeshFilter>();
        }
        filter.mesh = mesh;
        
        // Render the meshes 
        MeshRenderer renderer = meshObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = meshObject.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Particles/Standard Unlit"));
            material.color = new Color(0.8f, 0.8f, 0.8f); // Light gray color
            renderer.material = material;

            }

        pointCloudMeshes.Add(mesh);
    }
}

    // Clear previous meshes and GameObjects
    public void ClearPreviousMeshes()
    {
        foreach (var meshObject in meshObjects)
        {
            MeshFilter filter = meshObject.GetComponent<MeshFilter>();
            if (filter != null)
            {
                Destroy(filter.mesh);  // Release Mesh resources
            }
        }
        pointCloudMeshes.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
