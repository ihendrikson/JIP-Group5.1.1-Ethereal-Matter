using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class PoseMapperIK : MonoBehaviour
{
    public Animator animator;
    public Vector3[] joints;
    public Transform lShoulder, rShoulder;
    public float LowPassScale = 0.7f;
    private Transform HipTransform;
    private Transform TorsoTransform;
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
    public float scaleFactorStatic;


    private float[] targetLengths = new float[8];       // UL LR Arm, LR LU Leg scales
    private Transform[] targetLimbs = new Transform[8]; // UL LR Arm, LR LU Leg bones

    private Vector3[] jointsPrev;
    private void Start()
    {
        //// Set shoulder position
        //lShoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        //rShoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

        // Hip transform
        HipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
        TorsoTransform = animator.GetBoneTransform(HumanBodyBones.UpperChest);

        // Limbs to rescale
        // Arms
        targetLimbs[0] = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        targetLimbs[1] = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        targetLimbs[2] = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        targetLimbs[3] = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        // Legs
        targetLimbs[4] = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        targetLimbs[5] = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        targetLimbs[6] = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        targetLimbs[7] = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            if (joints.Length >= 33)  // Assuming skeletonPoints[0] is for the left hand and [1] for the right hand
            {

                //lShoulder.position = joints[(int)PositionIndex.LeftShoulder];
                //rShoulder.position = joints[(int)PositionIndex.RightShoulder];

                // Low pass filter on joint positions
                if (jointsPrev != null)
                {
                    for (var i = 0; i < joints.Length; i++)
                    {
                        // Apply initial hip transform
                        joints[i] += HipTransform.position;
                        joints[i] = LowPassScale * joints[i] + (1 - LowPassScale) * jointsPrev[i];
                        jointsPrev[i] = joints[i];
                    }
                } else
                {
                    jointsPrev = new Vector3[33];
                    jointsPrev = joints;
                    return;
                }
                // Calculate and resize limbs to match the model
                CalculateScales();
                AdjustLimbScale();
                // Rotate Based on shoulders 
                Quaternion TorsoRotation = Quaternion.LookRotation(Vector3.Cross((joints[(int)PositionIndex.RightShoulder] - joints[(int)PositionIndex.LeftShoulder]) / 2, Vector3.up).normalized);
                HipTransform.rotation = TorsoRotation;

                // ---- Hands (Wrists) ----
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, joints[(int)PositionIndex.LeftWrist]);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, joints[(int)PositionIndex.RightWrist]);

                // ---- Elbows ----
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, joints[(int)PositionIndex.LeftElbow]);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, joints[(int)PositionIndex.RightElbow]);

                //// ---- Feet (Ankles) ----
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, joints[(int)PositionIndex.LeftAnkle]);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, joints[(int)PositionIndex.RightAnkle]);

                // ---- Knees ----
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
                animator.SetIKHintPosition(AvatarIKHint.LeftKnee, joints[(int)PositionIndex.LeftKnee]);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
                animator.SetIKHintPosition(AvatarIKHint.RightKnee, joints[(int)PositionIndex.RightKnee]);

                // Rotate animator around
                //HipTransform.rotation *= Quaternion.Euler(0, 180f, 0);




            }
        }
    }
    private void AdjustLimbScale()
    {
        for (int i = 0; i < targetLimbs.Length; i++)
        {
            if (targetLengths[i] == 0f) {  continue; }
            Transform bone = targetLimbs[i];
            // Calculate the original length of the bone by measuring its local scale along the Y-axis (assuming the bone is aligned this way)
            float originalLength = bone.localScale.y;

            // Calculate the scale factor needed to match the target length
            float scaleFactor = targetLengths[i] / originalLength;

            // Apply the new scale, scaling along the Y-axis to adjust the length
            Vector3 newScale = bone.localScale;
            newScale.y *= scaleFactor*scaleFactorStatic;
            bone.localScale = newScale;
        }
    }

    private void CalculateScales() 
    {
        // Arms
        targetLengths[0] = Vector3.Distance(joints[(int)PositionIndex.LeftShoulder], joints[(int)PositionIndex.LeftElbow]); // Left upper arm
        targetLengths[1] = Vector3.Distance(joints[(int)PositionIndex.LeftElbow], joints[(int)PositionIndex.LeftWrist]); // Left lower arm
        targetLengths[2] = Vector3.Distance(joints[(int)PositionIndex.RightShoulder], joints[(int)PositionIndex.RightElbow]); // Right upper arm
        targetLengths[3] = Vector3.Distance(joints[(int)PositionIndex.RightElbow], joints[(int)PositionIndex.RightWrist]); // Right lower arm
        // Legs
        targetLengths[4] = Vector3.Distance(joints[(int)PositionIndex.LeftHip], joints[(int)PositionIndex.LeftKnee]); // Left upper leg
        targetLengths[5] = Vector3.Distance(joints[(int)PositionIndex.LeftKnee], joints[(int)PositionIndex.LeftAnkle]); // Left lower leg
        targetLengths[6] = Vector3.Distance(joints[(int)PositionIndex.RightHip], joints[(int)PositionIndex.RightKnee]); // Right upper leg
        targetLengths[7] = Vector3.Distance(joints[(int)PositionIndex.RightKnee], joints[(int)PositionIndex.RightAnkle]); // Right lower leg
    }
}