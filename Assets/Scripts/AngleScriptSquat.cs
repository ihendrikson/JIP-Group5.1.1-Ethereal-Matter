// //////////////////////////////////////////////////////////////////////////////// Script that colors the joints in the case an exercise is performed wrong.
// //                  //              Written by Ethereal-Matter group 5.1.1 JIP.
// //////////////////////////////////////////////////////////////////////////////
// using UnityEngine;
// using System.Collections.Generic;

// public class HumanoidCalculator : MonoBehaviour
// {
//     //initialize options in unity
//     public Color targetColor = Color.red;
//     public Color defaultColor = new Color(1, 1, 1, 0);
//     public GameObject ballPrefab;
//     public float ballRadius = 0.5f;
//     public Material ballMaterial; // New field for ball material

//     //make it Serializable so Multiple angles can be chosen.
//     [System.Serializable]
//     public class AngleCalculator
//     {
//         public string name;
//         public Transform top;
//         public Transform middle;
//         public Transform bottom;
//         public float minAngle;
//         public float maxAngle;
//         public GameObject ballInstance;

//         public AngleCalculator(string name, Transform top, Transform middle, Transform bottom, float minAngle, float maxAngle)
//         {
//             this.name = name;
//             this.top = top;
//             this.middle = middle;
//             this.bottom = bottom;
//             this.minAngle = minAngle;
//             this.maxAngle = maxAngle;
//         }

//         // Calculate angle between top, middle, and bottom points
//         public float CalculateAngle()
//         {
//             if (top && middle && bottom)
//             {
//                 Vector3 upperVec = top.position - middle.position;
//                 Vector3 lowerVec = bottom.position - middle.position;
//                 return Vector3.Angle(upperVec, lowerVec);
//             }
//             return 0f;
//         }

//         // Check if the angle is within the valid range
//         public bool IsAngleInRange(float angle) => angle >= minAngle && angle <= maxAngle;

//         // Determine the red intensity based on how far the angle is from the valid range
//         public float GetRedIntensity(float angle)
//         {
//             if (angle < minAngle) return Mathf.Clamp01((minAngle - angle) / 50f);
//             if (angle > maxAngle) return Mathf.Clamp01((angle - maxAngle) / 50f);
//             return 0f;
//         }
//     }

//     //make it Serializable So there can be more distances on one avatar
//     [System.Serializable]
//     public class DistanceCalculator
//     {
//         public string name;
//         public Transform jointA;
//         public Transform jointB;
//         public float minDistance;
//         public GameObject ballInstance; //create ball instance of the ball that fits around the misaligned joint

//         public DistanceCalculator(string name, Transform jointA, Transform jointB, float minDistance)
//         {
//             this.name = name;
//             this.jointA = jointA;
//             this.jointB = jointB;
//             this.minDistance = minDistance;
//         }

//         // Calculate the x-distance between jointA and jointB
//         public float CalculateDistance()
//         {
//             if (jointA && jointB)
//             {
//                 return (jointA.position.z - jointB.position.z);
//             }
//             return 0f;
//         }

//         // Check if the distance is within the valid range
//         public bool IsDistanceInRange(float distance) => distance >= minDistance;
//     }

//     public List<AngleCalculator> angleCalculators = new List<AngleCalculator>();
//     public List<DistanceCalculator> distanceCalculators = new List<DistanceCalculator>();

//     void Start()
//     {
//         // Instantiate ball instances for each angle calculator
//         foreach (var angleCalculator in angleCalculators)
//         {
//             if (ballPrefab)
//             {
//                 angleCalculator.ballInstance = Instantiate(ballPrefab, angleCalculator.middle.position, Quaternion.identity);
//                 angleCalculator.ballInstance.transform.SetParent(angleCalculator.middle);
//                 // Set the initial scale based on ballRadius
//                 angleCalculator.ballInstance.transform.localScale = new Vector3(ballRadius, ballRadius, ballRadius);

//                 // Apply the ball material
//                 var renderer = angleCalculator.ballInstance.GetComponent<Renderer>();
//                 if (renderer && ballMaterial)
//                 {
//                     renderer.material = ballMaterial;
//                 }
//             }
//         }

//         // Instantiate ball instances for each distance calculator
//         foreach (var distanceCalculator in distanceCalculators)
//         {
//             if (ballPrefab)
//             {
//                 distanceCalculator.ballInstance = Instantiate(ballPrefab, distanceCalculator.jointA.position, Quaternion.identity);
//                 distanceCalculator.ballInstance.transform.SetParent(distanceCalculator.jointA);
//                 distanceCalculator.ballInstance.transform.localScale = new Vector3(ballRadius, ballRadius, ballRadius);

//                 // Apply the ball material
//                 var renderer = distanceCalculator.ballInstance.GetComponent<Renderer>();
//                 if (renderer && ballMaterial)
//                 {
//                     renderer.material = ballMaterial;
//                 }
//             }
//         }
//     }

//     void Update()
//     {
//         foreach (var angleCalculator in angleCalculators)
//         {
//             float angle = angleCalculator.CalculateAngle();
//             float redIntensity = angleCalculator.GetRedIntensity(angle);
//             Color color = Color.Lerp(defaultColor, targetColor, redIntensity);
//             // Set the alpha value to gradually increase with red intensity
//             color.a = redIntensity;

//             // Change color of the ball instance
//             if (angleCalculator.ballInstance)
//             {
//                 var renderer = angleCalculator.ballInstance.GetComponent<Renderer>();
//                 if (renderer)
//                 {
//                     renderer.material.SetColor("_Color", color);
//                 }
//                 // Update the ball scale based on ballRadius
//                 Vector3 scale = new Vector3(ballRadius, ballRadius, ballRadius);
//                 angleCalculator.ballInstance.transform.localScale = scale;
//             }
//         }

//         foreach (var distanceCalculator in distanceCalculators)
//         {
//             float distance = distanceCalculator.CalculateDistance();

//             // Update the color of the ball instance based on the distance
//             if (distanceCalculator.ballInstance)
//             {
//                 Color color = distanceCalculator.IsDistanceInRange(distance) ? defaultColor : targetColor;
//                 var renderer = distanceCalculator.ballInstance.GetComponent<Renderer>();
//                 if (renderer)
//                 {
//                     renderer.material.SetColor("_Color", color);
//                 }
//                 // Update the ball scale based on ballRadius
//                 Vector3 scale = new Vector3(ballRadius, ballRadius, ballRadius);
//                 distanceCalculator.ballInstance.transform.localScale = scale;
//             }
//         }
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AngleScriptSquat : HumanoidCalculatorV3
{
    public PipeServer server;
    public Animator animator;
    private AngleCalculator angleCalculator;
    private DistanceCalculator distanceCalculator;
    // Start is called before the first frame update
    public override void Start()
    {
        // Angle contraints
        angleCalculator = new AngleCalculator("Left Knee", server.GetLandmark(Landmark.LEFT_HIP), server.GetLandmark(Landmark.LEFT_KNEE), server.GetLandmark(Landmark.LEFT_ANKLE), 75, 181, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
        this.angleCalculators.Add(angleCalculator);
        angleCalculator = new AngleCalculator("Right Knee", server.GetLandmark(Landmark.RIGHT_HIP), server.GetLandmark(Landmark.RIGHT_KNEE), server.GetLandmark(Landmark.RIGHT_ANKLE), 75, 181, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
        this.angleCalculators.Add(angleCalculator);
        // Distance constraints
        //distanceCalculator = new DistanceCalculator("Left knee to toe", server.GetLandmark(Landmark.LEFT_KNEE), server.GetLandmark(Landmark.LEFT_ANKLE), 0, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
        //this.distanceCalculators.Add(distanceCalculator);
        //distanceCalculator = new DistanceCalculator("Right knee to toe", server.GetLandmark(Landmark.RIGHT_KNEE), server.GetLandmark(Landmark.RIGHT_ANKLE), 0, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
        //this.distanceCalculators.Add(distanceCalculator);
        // Start
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
}
