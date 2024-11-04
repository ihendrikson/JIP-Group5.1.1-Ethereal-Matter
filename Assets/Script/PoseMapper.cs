using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Net;
using UnityEngine;
using static PoseMapper;

public class PoseMapper : MonoBehaviour
{
    public Animator animator;  // Reference to the avatar's Animator component
    public Vector3[] joints;   // Array of joint positions, assumed to be world space
    public float smoothFactor = 5f;

    public JointPoint[] jointPoints;
    public class JointPoint
    {
        public Vector3 Pos3D = new Vector3();
        public Vector3 Now3D = new Vector3();
        public Vector3[] PrevPos3D = new Vector3[6];
        public float score3D;

        // Bones
        public Transform Transform = null;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;

        public JointPoint Child = null;
        public JointPoint Parent = null;

        // For Kalman filter
        //public Vector3 P = new Vector3();
        //public Vector3 X = new Vector3();
        //public Vector3 K = new Vector3();
    }
    public enum PositionIndex : int
    {
        Nose = 0,
        LeftEyeInner,
        LeftEye,
        LeftEyeOuter,
        RightEyeInner,
        RightEye,
        RightEyeOuter,
        LeftEar,
        RightEar,
        MouthLeft,
        MouthRight,
        LeftShoulder,
        RightShoulder,
        LeftElbow,
        RightElbow,
        LeftWrist,
        RightWrist,
        LeftPinky,
        RightPinky,
        LeftIndexFinger,
        RightIndexFinger,
        LeftThumb,
        RightThumb,
        LeftHip,
        RightHip,
        LeftKnee,
        RightKnee,
        LeftAnkle,
        RightAnkle,
        LeftHeel,
        RightHeel,
        LeftFootIndexToe,
        RightFootIndexToe,
        // Virtual (not provided by the model, but calculated)
        Spine,
        Hips,
        Neck,
        // Helpers
        Count,
    }

    private Vector3 spine, hips;
    private JointPoint hip, head;
    private JointPoint elbow;
    void calculatedJointPoint(JointPoint[] jointPoints)
    {
        // Calculate Neck
        jointPoints[(int)PositionIndex.Neck].Transform.position = (jointPoints[(int)PositionIndex.RightShoulder].Transform.position - jointPoints[(int)PositionIndex.LeftShoulder].Transform.position) / 2f;
        // Calculate Hips
        hips = (jointPoints[(int)PositionIndex.RightHip].Transform.position - jointPoints[(int)PositionIndex.LeftHip].Transform.position) / 2f;
        jointPoints[(int)PositionIndex.Hips].Transform.position = hips;
        // Calculate Spine
        spine = jointPoints[(int)PositionIndex.Hips].Transform.position;
        spine.z = hips.z * (2f / 3f);
        jointPoints[(int)PositionIndex.Spine].Transform.position = spine;
    }

    void getBoneTransforms(JointPoint[] jointPoints)
    {
        // Right arm
        jointPoints[(int)PositionIndex.RightShoulder].Transform = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[(int)PositionIndex.RightElbow].Transform = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[(int)PositionIndex.RightWrist].Transform = animator.GetBoneTransform(HumanBodyBones.RightHand);
        // Left arm
        jointPoints[(int)PositionIndex.LeftShoulder].Transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[(int)PositionIndex.LeftElbow].Transform = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[(int)PositionIndex.LeftWrist].Transform = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        // Right leg
        jointPoints[(int)PositionIndex.RightHip].Transform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[(int)PositionIndex.RightKnee].Transform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[(int)PositionIndex.RightAnkle].Transform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        // Right leg
        jointPoints[(int)PositionIndex.LeftHip].Transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[(int)PositionIndex.LeftKnee].Transform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[(int)PositionIndex.LeftAnkle].Transform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        // Head & Feet
        jointPoints[(int)PositionIndex.Nose].Transform = animator.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[(int)PositionIndex.RightFootIndexToe].Transform = animator.GetBoneTransform(HumanBodyBones.RightToes);
        jointPoints[(int)PositionIndex.LeftFootIndexToe].Transform = animator.GetBoneTransform(HumanBodyBones.RightToes);
        // Virtual Joint
        jointPoints[(int)PositionIndex.Neck].Transform = animator.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[(int)PositionIndex.Hips].Transform = animator.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[(int)PositionIndex.Spine].Transform = animator.GetBoneTransform(HumanBodyBones.Spine);
        //calculatedJointPoint(jointPoints);
    }

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator component not assigned.");
        }
        //// Array for all JointPoints and their values
        jointPoints = new JointPoint[(int)PositionIndex.Count];
        for (var i = 0; i < (int)PositionIndex.Count; i++) jointPoints[i] = new JointPoint();
        // Get bones
        jointPoints[(int)PositionIndex.RightShoulder].Transform = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[(int)PositionIndex.RightElbow].Transform = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[(int)PositionIndex.RightWrist].Transform = animator.GetBoneTransform(HumanBodyBones.RightHand);
        // Set initial rotation
        jointPoints[(int)PositionIndex.RightShoulder].InitRotation = jointPoints[(int)PositionIndex.RightShoulder].Transform.rotation;
        jointPoints[(int)PositionIndex.RightElbow].InitRotation = jointPoints[(int)PositionIndex.RightElbow].Transform.rotation;
        jointPoints[(int)PositionIndex.RightWrist].InitRotation = jointPoints[(int)PositionIndex.RightWrist].Transform.rotation;
        // Set parent child relations
        jointPoints[(int)PositionIndex.RightShoulder].Child = jointPoints[(int)PositionIndex.RightElbow];
        jointPoints[(int)PositionIndex.RightElbow].Child = jointPoints[(int)PositionIndex.RightWrist];
        jointPoints[(int)PositionIndex.RightElbow].Parent = jointPoints[(int)PositionIndex.RightShoulder];


        //// Hips
        //jointPoints[(int)PositionIndex.Hips].Transform = animator.GetBoneTransform(HumanBodyBones.Hips);
        //jointPoints[(int)PositionIndex.LeftHip].Transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        //jointPoints[(int)PositionIndex.RightHip].Transform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        //jointPoints[(int)PositionIndex.RightWrist].InitRotation = jointPoints[(int)PositionIndex.RightWrist].Transform.rotation;
        //getBoneTransforms(jointPoints);
        //var forward = TriangleNormal(jointPoints[(int)PositionIndex.Hips].Transform.position, joints[(int)PositionIndex.LeftHip], joints[(int)PositionIndex.RightHip]);

        //foreach (var jointPoint in jointPoints)
        //{
        //    if (jointPoint.Transform != null)
        //    {
        //        jointPoint.InitRotation = jointPoint.Transform.rotation;
        //    }

        //    if (jointPoint.Child != null)
        //    {
        //        jointPoint.Inverse = GetInverse(jointPoint, jointPoint.Child, forward);
        //        jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
        //    }
        //}
        //Debug.LogFormat("jointPoints initialized, {0} points", (int)PositionIndex.Count);
        // Transforms
        

        //// Child & Parent settings
        //// Right arm
        //jointPoints[(int)PositionIndex.RightShoulder].Child = jointPoints[(int)PositionIndex.RightElbow];
        //jointPoints[(int)PositionIndex.RightElbow].Child = jointPoints[(int)PositionIndex.RightWrist];
        //jointPoints[(int)PositionIndex.RightElbow].Parent = jointPoints[(int)PositionIndex.RightShoulder];
        //// Left Arm
        //jointPoints[(int)PositionIndex.LeftShoulder].Child = jointPoints[(int)PositionIndex.LeftElbow];
        //jointPoints[(int)PositionIndex.LeftElbow].Child = jointPoints[(int)PositionIndex.LeftWrist];
        //jointPoints[(int)PositionIndex.LeftElbow].Parent = jointPoints[(int)PositionIndex.LeftShoulder];

        //// Right leg
        //jointPoints[(int)PositionIndex.RightHip].Child = jointPoints[(int)PositionIndex.RightKnee];
        //jointPoints[(int)PositionIndex.RightKnee].Child = jointPoints[(int)PositionIndex.RightAnkle];
        //jointPoints[(int)PositionIndex.RightKnee].Parent = jointPoints[(int)PositionIndex.RightHip];
        //jointPoints[(int)PositionIndex.RightAnkle].Child = jointPoints[(int)PositionIndex.RightFootIndexToe];
        //jointPoints[(int)PositionIndex.RightAnkle].Parent = jointPoints[(int)PositionIndex.RightKnee];

        //// Left leg
        //jointPoints[(int)PositionIndex.LeftHip].Child = jointPoints[(int)PositionIndex.LeftKnee];
        //jointPoints[(int)PositionIndex.LeftKnee].Child = jointPoints[(int)PositionIndex.LeftAnkle];
        //jointPoints[(int)PositionIndex.LeftKnee].Parent = jointPoints[(int)PositionIndex.LeftHip];
        //jointPoints[(int)PositionIndex.LeftAnkle].Child = jointPoints[(int)PositionIndex.LeftFootIndexToe];
        //jointPoints[(int)PositionIndex.LeftAnkle].Parent = jointPoints[(int)PositionIndex.LeftKnee];

        //// Spine, neck and head
        //jointPoints[(int)PositionIndex.Spine].Child = jointPoints[(int)PositionIndex.Neck];
        //jointPoints[(int)PositionIndex.Neck].Child = jointPoints[(int)PositionIndex.Nose];
        //var forward = TriangleNormal(jointPoints[(int)PositionIndex.Hips].Transform.position, jointPoints[(int)PositionIndex.LeftHip].Transform.position, jointPoints[(int)PositionIndex.RightHip].Transform.position);
        //foreach (var jointPoint in jointPoints)
        //{
        //    if (jointPoint != null)
        //    {
        //        jointPoint.InitRotation = jointPoint.Transform.rotation;
        //    }
        //    if (jointPoint.Child != null)
        //    {
        //        jointPoint.Inverse = GetInverse(jointPoint, jointPoint.Child, forward);
        //        jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
        //    }
        //}

        //// Hips and head
        ////calculatedJointPoint(jointPoints);

        //var hip = jointPoints[(int)PositionIndex.Hips];
        ////initPosition = jointPoints[PositionIndex.hip.Int()].Transform.position;
        //hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        //hip.InverseRotation = hip.Inverse * hip.InitRotation;

        //var head = jointPoints[(int)PositionIndex.Nose];
        //head.InitRotation = jointPoints[(int)PositionIndex.Nose].Transform.rotation;
        //head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[(int)PositionIndex.Nose].Transform.position));
        //head.InverseRotation = head.Inverse * head.InitRotation;
        //Debug.Log("jointPoints finished building");
    }


    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
    private Quaternion GetInverse(JointPoint p1, JointPoint p2, Vector3 forward)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.Transform.position - p2.Transform.position, forward));
    }
    void Update()
    {
        if (joints == null || joints.Length != 33)
        {
            Debug.LogError("Joint data is either null or does not have 33 elements.");
            return;
        }

        // Map and update avatar's bones based on joint data
        Debug.Log("Rigging");
        for (int i = 0; i < joints.Length; i++)
        {
            jointPoints[i].Pos3D = joints[i];
        }

        jointPoints[(int)PositionIndex.RightElbow].Transform.rotation = Quaternion.LookRotation((jointPoints[(int)PositionIndex.RightElbow].Child.Pos3D - jointPoints[(int)PositionIndex.RightElbow].Pos3D).normalized);

        //var forward = TriangleNormal(jointPoints[(int)PositionIndex.Hips].Transform.position, joints[(int)PositionIndex.LeftHip], joints[(int)PositionIndex.RightHip]);

        //foreach (var jointPoint in jointPoints)
        //{
        //    if (jointPoint.Parent != null)
        //    {
        //        var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
        //        jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
        //    }
        //    else if (jointPoint.Child != null)
        //    {
        //        jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
        //    }
        //}
        //RigAvatar(jointPoints);
    }

    void RigAvatar(JointPoint[] jointPoints)
    {
        // Update all joint positions
        for (int i = 0; i < joints.Length; i++)
        {
            jointPoints[i].Pos3D = joints[i];
        }
        jointPoints[(int)PositionIndex.Hips].Pos3D = joints[(int)PositionIndex.LeftHip] + (joints[(int)PositionIndex.RightHip] - joints[(int)PositionIndex.LeftHip]) / 2.0f;
        getBoneTransforms(jointPoints);
        var forward = TriangleNormal(jointPoints[(int)PositionIndex.Hips].Pos3D, joints[(int)PositionIndex.LeftHip], joints[(int)PositionIndex.RightHip]);

        // rotate each of bones
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
            }
        }
    }
}
