using UnityEngine;
using System.Collections.Generic;

public class SafetyZone : MonoBehaviour
{
    [System.Serializable]
    public class SafetyZoneSettings
    {
        public Transform jointA;
        public Transform jointB;
        public float radius;
        public GameObject cylinderPrefab; // Assign a cylinder prefab in the inspector
        public GameObject cylinderInstance;

        // Constructor for SafetyZoneSettings
        public SafetyZoneSettings(Transform jointA, Transform jointB, float radius, GameObject cylinderPrefab)
        {
            this.jointA = jointA;
            this.jointB = jointB;
            this.radius = radius;
            this.cylinderPrefab = cylinderPrefab;
        }

        // Create or update the cylinder
        public void CreateOrUpdateCylinder()
        {
            if (cylinderPrefab && jointA && jointB)
            {
                if (cylinderInstance == null) // Create if not exists
                {
                    Debug.Log("Creating Cylinder");
                    Vector3 position = (jointA.position + jointB.position) / 2;
                    Vector3 direction = jointB.position - jointA.position;
                    float distance = direction.magnitude;

                    // Instantiate the cylinder at the midpoint
                    cylinderInstance = Instantiate(cylinderPrefab, position, Quaternion.identity);

                    // Rotate to align with the direction
                    cylinderInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

                    // Adjust the cylinder's scale to match the radius and height
                    Vector3 scale = new Vector3(radius * 2, distance / 2, radius * 2);
                    cylinderInstance.transform.localScale = scale;
                }
                else // Update existing cylinder
                {
                    UpdateCylinder();
                }
            }
        }

        // Update cylinder position and scale
        private void UpdateCylinder()
        {
            if (cylinderInstance)
            {
                Vector3 position = (jointA.position + jointB.position) / 2;
                Vector3 direction = jointB.position - jointA.position;
                float distance = direction.magnitude;

                // Update the cylinder position, rotation, and scale
                cylinderInstance.transform.position = position;
                cylinderInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                Vector3 scale = new Vector3(radius * 2, distance / 2, radius * 2);
                cylinderInstance.transform.localScale = scale;
            }
        }
    }

    // Change the name of the list to avoid conflict
    public List<SafetyZoneSettings> safetyZoneSettingsList = new List<SafetyZoneSettings>();

    // Initialize the safety zones and create cylinders
    public virtual void Start()
    {
        Debug.Log("Starting Safety Zone!");
        foreach (var safetyZone in safetyZoneSettingsList)
        {
            safetyZone.CreateOrUpdateCylinder();
        }
    }

    // Update safety zones each frame
    public virtual void Update()
    {
        foreach (var safetyZone in safetyZoneSettingsList)
        {
            safetyZone.CreateOrUpdateCylinder(); // Keep updating in case of any changes
        }
    }
}
