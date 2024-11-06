using UnityEngine;
using System.Collections.Generic;

public class HumanoidCalculatorV3 : MonoBehaviour
{
    public Color targetColor = Color.red;
    public Color defaultColor = new Color(1, 1, 1, 0); // Set initial color to be fully transparent
    public GameObject ballPrefab; // Assign the ball prefab in the inspector

    [System.Serializable]
    public class AngleCalculator
    {
        public string name;
        public Transform top;
        public Transform middle;
        public Transform bottom;
        public float minAngle;
        public float maxAngle;
        public GameObject ballInstance;
        public Transform ballTarget;

        public AngleCalculator(string name, Transform top, Transform middle, Transform bottom, float minAngle, float maxAngle, Transform ballTarget)
        {
            this.name = name;
            this.top = top;
            this.middle = middle;
            this.bottom = bottom;
            this.minAngle = minAngle;
            this.maxAngle = maxAngle;
            this.ballTarget = ballTarget;
        }

        // Calculate angle between top, middle, and bottom points
        public float CalculateAngle()
        {
            if (top && middle && bottom)
            {
                Vector3 upperVec = top.position - middle.position;
                Vector3 lowerVec = bottom.position - middle.position;
                return Vector3.Angle(upperVec, lowerVec);
            }
            return 0f;
        }

        // Check if the angle is within the valid range
        public bool IsAngleInRange(float angle) => angle >= minAngle && angle <= maxAngle;

        // Determine the red intensity based on how far the angle is from the valid range
        public float GetRedIntensity(float angle)
        {
            if (angle < minAngle) return Mathf.Clamp01((minAngle - angle) / 50f);
            if (angle > maxAngle) return Mathf.Clamp01((angle - maxAngle) / 50f);
            return 0f;
        }
    }

    [System.Serializable]
    public class DistanceCalculator
    {
        public string name;
        public Transform jointA;
        public Transform jointB;
        public float minDistance;
        public GameObject ballInstance;
        public Transform ballTarget;

        public DistanceCalculator(string name, Transform jointA, Transform jointB, float minDistance, Transform ballTarget)
        {
            this.name = name;
            this.jointA = jointA;
            this.jointB = jointB;
            this.minDistance = minDistance;
            this.ballTarget = ballTarget;
        }

        // Calculate the x-distance between jointA and jointB
        public float CalculateDistance()
        {
            if (jointA && jointB)
            {
                return (jointA.position.z - jointB.position.z);
            }
            return 0f;
        }

        // Check if the distance is within the valid range
        public bool IsDistanceInRange(float distance) => distance >= minDistance;
    }

    public List<AngleCalculator> angleCalculators = new List<AngleCalculator>();
    public List<DistanceCalculator> distanceCalculators = new List<DistanceCalculator>();

    public virtual void Start()
    {
        // Instantiate ball instances for each angle calculator
        foreach (var angleCalculator in angleCalculators)
        {
            if (ballPrefab)
            {
                angleCalculator.ballInstance = Instantiate(ballPrefab, angleCalculator.ballTarget.position, Quaternion.identity);
                angleCalculator.ballInstance.transform.SetParent(angleCalculator.ballTarget);
            }
        }

        // Instantiate ball instances for each distance calculator
        foreach (var distanceCalculator in distanceCalculators)
        {
            if (ballPrefab)
            {
                distanceCalculator.ballInstance = Instantiate(ballPrefab, distanceCalculator.ballTarget.position, Quaternion.identity);
                distanceCalculator.ballInstance.transform.SetParent(distanceCalculator.ballTarget);
            }
        }
    }

    public virtual void Update()
    {
        foreach (var angleCalculator in angleCalculators)
        {
            float angle = angleCalculator.CalculateAngle();
            float redIntensity = angleCalculator.GetRedIntensity(angle);
            Color color = Color.Lerp(defaultColor, targetColor, redIntensity);
            // Set the alpha value to gradually increase with red intensity
            color.a = redIntensity; // Adjust this value to control transparency
            // Change color of the ball instance
            if (angleCalculator.ballInstance)
            {
                var renderer = angleCalculator.ballInstance.GetComponent<Renderer>();
                if (renderer)
                {
                    // Ensure the material is using a shader that supports transparency
                    renderer.material.SetColor("_Color", color);
                }
            }
        }

        foreach (var distanceCalculator in distanceCalculators)
        {
            float distance = distanceCalculator.CalculateDistance();
            //Debug.Log($"{distanceCalculator.name} distance: {distance}");

            // Update the color of the ball instance based on the distance
            if (distanceCalculator.ballInstance)
            {
                Color color = distanceCalculator.IsDistanceInRange(distance) ? defaultColor : targetColor;
                var renderer = distanceCalculator.ballInstance.GetComponent<Renderer>();
                if (renderer)
                {
                    renderer.material.SetColor("_Color", color);
                }
            }
        }
    }
}