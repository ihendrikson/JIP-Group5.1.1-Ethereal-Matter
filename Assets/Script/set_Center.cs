using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public Transform targetTransform;
    public float X_offset = 0f;
    public float Y_offset = 0f;
    public float Z_offset = 0f;
    PoseVisuallizer3D Greenman;

    void Start()
    {
        Greenman = FindObjectOfType<PoseVisuallizer3D>();
    }

    void Update()
    {
        if(Greenman.vector3Dictionary.ContainsKey("Center Hip"))
        {
            // Replace the GameObject's position with the specified new position
            targetTransform.position = Vector3.Scale(Greenman.vector3Dictionary["Center Hip"] + new Vector3(X_offset, Y_offset, Z_offset), new Vector3(-1f,1f,1f));
            
            //Change the scale of the avatar based on depth 
            //Debug.Log(Greenman.vector3Dictionary["Right Ankle"].z);
            //targetTransform.localScale = new Vector3(Greenman.vector3Dictionary["Right Ankle"].z, Greenman.vector3Dictionary["Right Ankle"].z, Greenman.vector3Dictionary["Right Ankle"].z);
        }
        
    }
}
