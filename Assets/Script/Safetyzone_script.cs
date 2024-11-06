//// Script that controls the visualization of the safetyzone 
//// Made by JIP team: Ethereal Matter 
////  25/10/2024 

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SafetyZoneScripts : SafetyZone
//{ 
//    // Create Dictionaries for safetyzone
//    private Dictionary<string, float> SafetyDictionary = new Dictionary<string, float>();
//    private List<SafetyZoneSettings> safetyZones = new List<SafetyZoneSettings>();
//    private Dictionary<string, Transform> joints;

//    // Connect to pointcloud script to receive radius for all body segments
//    Pointcloud_nocolor Pointcloud;

//    // Set avatar transforms 
//    public Transform rightAnkle;
//    public Transform rightKnee;
//    public Transform rightHip;
//    public Transform rightShoulder;
//    public Transform rightElbow;
//    public Transform rightWrist;
//    public Transform leftAnkle;
//    public Transform leftKnee;
//    public Transform leftHip;
//    public Transform leftShoulder;
//    public Transform leftElbow;
//    public Transform leftWrist;
//    public Transform centerHip;
//    public Transform spine1;
//    public Transform spine2;
//    public Transform centerShoulder;

//    // Assing gameobject that will visualize the safetye zone 
//    public GameObject cylinderPrefab; 

//    // Variables to check is the safety zone is already visualized
//    private bool is_visualized = false;


//    [System.Serializable]
//    public class SafetyZoneSettings
//    {
//        public Transform jointA;
//        public Transform jointB;
//        public float radius;
//        public GameObject cylinderPrefab;
//        public GameObject cylinderInstance;

//        public SafetyZoneSettings(Transform jointA, Transform jointB, float radius, GameObject cylinderPrefab)
//        {
//            this.jointA = jointA;
//            this.jointB = jointB;
//            this.radius = radius;
//            this.cylinderPrefab = cylinderPrefab;
//        }

//        public void CreateCylinder()
//        {
//            Debug.Log("Creating Cylinders");
//            if (cylinderPrefab && jointA && jointB)
//            {
//                Vector3 position = (jointA.position + jointB.position) / 2;
//                Vector3 direction = jointB.position - jointA.position;
//                float distance = direction.magnitude;

//                // Instantiate the cylinder at the midpoint
//                cylinderInstance = Instantiate(cylinderPrefab, position, Quaternion.identity);

//                // Rotate to align with the direction
//                cylinderInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

//                // Adjust the cylinder's scale to match the radius and height, keeping it within the joints
//                Vector3 scale = new Vector3(radius * 2f, distance / 2f, radius * 2f);
//                cylinderInstance.transform.localScale = scale;
//            }
//        }
//    }

//    public override void Start()
//    {
//        Debug.Log("Start method called.");

//        //Get the public variables of the Pointcloud script 
//        Pointcloud = FindObjectOfType<Pointcloud_nocolor>();

//        //Create specific lookup dictionary for the choosen avatar (specific for every different avatar) 
//        joints = new Dictionary<string, Transform>
//        {
//            { "Right Ankle", rightAnkle },
//            { "Right Knee", rightKnee },
//            { "Right Hip", rightHip },
//            { "Right Shoulder", rightShoulder },
//            { "Right Elbow", rightElbow },
//            { "Right Wrist", rightWrist },
//            { "Left Ankle", leftAnkle },
//            { "Left Knee", leftKnee },
//            { "Left Hip", leftHip },
//            { "Left Shoulder", leftShoulder },
//            { "Left Elbow", leftElbow },
//            { "Left Wrist", leftWrist },
//            { "Center Hip", centerHip },
//            { "Spine 1", spine1 },
//            { "Spine 2", spine2 },
//            { "Center Shoulder", centerShoulder }
//        };

//        base.Start();
//    }

//    public override void Update()
//    {
//        if (Pointcloud.Visualize_safetyzone == true && is_visualized == false && Pointcloud.enabled == true)
//        {
//            // Loop through the dictionary to set the received data into the correct format 
//            foreach (var entry in Pointcloud.SafetyDictionary)
//            {
//                string[] jointsPair = entry.Key.Split(';');
//                if (joints.ContainsKey(jointsPair[0]) && joints.ContainsKey(jointsPair[1]))
//                {
//                    Transform jointA = joints[jointsPair[0]];
//                    Transform jointB = joints[jointsPair[1]];
//                    float radius = entry.Value * 0.1f;

//                    Debug.Log($"Creating safety zone between {jointA.name} and {jointB.name} with radius {radius}");

//                    SafetyZoneSettings safetyZone = new SafetyZoneSettings(jointA, jointB, radius, cylinderPrefab); 

//                    safetyZone.CreateCylinder(); // Call create cylinder function for every body segment
//                    safetyZones.Add(safetyZone);
//                }
//            }
//            is_visualized = true; // Ensure that only one visualization is done
//        }

//        // Set the position of the created cylinders to the position of the avatar 
//        if (is_visualized == true) 
//        {
//            foreach (var safetyZone in safetyZones)
//            {
//                if (safetyZone.cylinderInstance && safetyZone.jointA && safetyZone.jointB)
//                {
//                    Vector3 position = (safetyZone.jointA.position + safetyZone.jointB.position) / 2;
//                    Vector3 direction = safetyZone.jointB.position - safetyZone.jointA.position;
//                    float distance = direction.magnitude;

//                    // Update the cylinder position, rotation, and scale
//                    safetyZone.cylinderInstance.transform.position = position;
//                    safetyZone.cylinderInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
//                    Vector3 scale = new Vector3(safetyZone.radius * 2, distance / 2, safetyZone.radius * 2);
//                    safetyZone.cylinderInstance.transform.localScale = scale;
//                }
//            }
//        }

//        base.Update();
//    }

//}

using System.Collections.Generic;
using UnityEngine;

public class SafetyZoneScripts : SafetyZone
{
    // Create a dictionary for safety zone radii
    private Dictionary<string, float> safetyDictionary = new Dictionary<string, float>();
    private Dictionary<string, Transform> joints;

    // Connect to the Pointcloud script to receive radius data for all body segments
    private Pointcloud_nocolor pointcloud;

    // Set avatar transforms
    public Transform rightAnkle;
    public Transform rightKnee;
    public Transform rightHip;
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightWrist;
    public Transform leftAnkle;
    public Transform leftKnee;
    public Transform leftHip;
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftWrist;
    public Transform centerHip;
    public Transform spine1;
    public Transform spine2;
    public Transform centerShoulder;

    // Assign the GameObject that will visualize the safety zone
    public GameObject cylinderPrefab;

    // Variable to check if the safety zone has already been visualized
    private bool isVisualized = false;

    public override void Start()
    {
        Debug.Log("Start method called.");

        // Get the public variables from the Pointcloud script
        pointcloud = FindObjectOfType<Pointcloud_nocolor>();

        // Create a dictionary to map joint names to their transforms
        joints = new Dictionary<string, Transform>
        {
            { "Right Ankle", rightAnkle },
            { "Right Knee", rightKnee },
            { "Right Hip", rightHip },
            { "Right Shoulder", rightShoulder },
            { "Right Elbow", rightElbow },
            { "Right Wrist", rightWrist },
            { "Left Ankle", leftAnkle },
            { "Left Knee", leftKnee },
            { "Left Hip", leftHip },
            { "Left Shoulder", leftShoulder },
            { "Left Elbow", leftElbow },
            { "Left Wrist", leftWrist },
            { "Center Hip", centerHip },
            { "Spine 1", spine1 },
            { "Spine 2", spine2 },
            { "Center Shoulder", centerShoulder }
        };

        // Call the base class Start method
        base.Start();
    }

    public override void Update()
    {
        if (pointcloud.Visualize_safetyzone && !isVisualized && pointcloud.enabled)
        {
            // Loop through the dictionary to set the received data into the correct format
            foreach (var entry in pointcloud.SafetyDictionary)
            {
                string[] jointsPair = entry.Key.Split(';');
                if (joints.ContainsKey(jointsPair[0]) && joints.ContainsKey(jointsPair[1]))
                {
                    Transform jointA = joints[jointsPair[0]];
                    Transform jointB = joints[jointsPair[1]];
                    float radius = entry.Value * 0.1f; // Scale down the radius if necessary

                    Debug.Log($"Creating safety zone between {jointA.name} and {jointB.name} with radius {radius}");

                    SafetyZoneSettings safetyZone = new SafetyZoneSettings(jointA, jointB, radius, cylinderPrefab);
                    safetyZone.CreateOrUpdateCylinder(); // Call to create/update the cylinder
                    safetyZoneSettingsList.Add(safetyZone); // Add to the list of safety zones
                }
            }
            isVisualized = true; // Ensure that only one visualization is done
        }

        // Update the position and scale of the created cylinders
        if (isVisualized)
        {
            foreach (var safetyZone in safetyZoneSettingsList)
            {
                if (safetyZone.cylinderInstance && safetyZone.jointA && safetyZone.jointB)
                {
                    safetyZone.CreateOrUpdateCylinder(); // Update each safety zone
                }
            }
        }

        // Call the base class Update method
        base.Update();
    }
}


