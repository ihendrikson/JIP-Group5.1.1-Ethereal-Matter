using UnityEngine;
using System.Collections.Generic;

public class HumanoidCalculator : MonoBehaviour
{
    public Color targetColor = Color.red;
    public Color defaultColor = new Color(1, 1, 1, 0); // Set initial color to be fully transparent
    public GameObject ballPrefab; // Assign the ball prefab in the inspector

    [System.Serializable]
    public class DistanceCalculator
    {
        public string name;
        public Transform jointA;
        public Transform jointB;
        public Transform referenceJointA;
        public Transform referenceJointB;
        public float threshold;
        public GameObject ballInstance;

        public DistanceCalculator(string name, Transform jointA, Transform jointB, Transform referenceJointA, Transform referenceJointB, float threshold)
        {
            this.name = name;
            this.jointA = jointA;
            this.jointB = jointB;
            this.referenceJointA = referenceJointA;
            this.referenceJointB = referenceJointB;
            this.threshold = threshold;
        }

        // Calculate the x-distance between jointA and jointB
        public float CalculateDistance()
        {
            if (jointA && jointB)
            {
                return Mathf.Abs(jointA.position.x - jointB.position.x);
            }
            return 0f;
        }

        // Calculate the x-distance between referenceJointA and referenceJointB
        public float CalculateReferenceDistance()
        {
            if (referenceJointA && referenceJointB)
            {
                return Mathf.Abs(referenceJointA.position.x - referenceJointB.position.x);
            }
            return 0f;
        }

        // Check if the difference in distance is greater than the threshold
        public bool IsDifferenceOutOfRange(float distance, float referenceDistance)
        {
            return Mathf.Abs(distance - referenceDistance) > threshold;
        }
    }

    public List<DistanceCalculator> distanceCalculators = new List<DistanceCalculator>();

    void Start()
    {
        // Instantiate ball instances for each distance calculator
        foreach (var distanceCalculator in distanceCalculators)
        {
            if (ballPrefab)
            {
                distanceCalculator.ballInstance = Instantiate(ballPrefab, distanceCalculator.jointA.position, Quaternion.identity);
                distanceCalculator.ballInstance.transform.SetParent(distanceCalculator.jointA);
            }
        }
    }

    void Update()
    {
        foreach (var distanceCalculator in distanceCalculators)
        {
            float distance = distanceCalculator.CalculateDistance();
            float referenceDistance = distanceCalculator.CalculateReferenceDistance();

            Debug.Log($"{distanceCalculator.name} distance: {distance}, jointA x position: {distanceCalculator.jointA.position.x}, jointB x position: {distanceCalculator.jointB.position.x}");
            Debug.Log($"{distanceCalculator.name} reference distance: {referenceDistance}, referenceJointA x position: {distanceCalculator.referenceJointA.position.x}, referenceJointB x position: {distanceCalculator.referenceJointB.position.x}");

            // Update the color of the ball instance based on the difference in distance
            if (distanceCalculator.ballInstance)
            {
                Color color = distanceCalculator.IsDifferenceOutOfRange(distance, referenceDistance) ? targetColor : defaultColor;
                var renderer = distanceCalculator.ballInstance.GetComponent<Renderer>();
                if (renderer)
                {
                    renderer.material.SetColor("_Color", color);
                }
            }
        }
    }
}
