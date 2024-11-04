using UnityEngine;

public class ScriptToggle : MonoBehaviour
{
    [SerializeField] public Pointcloud_nocolor Pointcloud;  // The script you want to enable/disable
    [SerializeField] public MonoBehaviour Safetyzone_script;

    public bool HPE_visuallizer = true;
    public GameObject Avatar;

    void Update()
    {
        // Check if the toggle key is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Toggle the target script's enabled state
            Pointcloud.ClearPreviousMeshes();
            Pointcloud.enabled = !Pointcloud.enabled;
            Debug.Log(Pointcloud.name + " is now " + (Pointcloud.enabled ? "Enabled" : "Disabled"));
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            HPE_visuallizer = !HPE_visuallizer;
            Debug.Log(HPE_visuallizer);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Avatar.SetActive(!Avatar.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Safetyzone_script.enabled = !Safetyzone_script.enabled;
            Debug.Log(Safetyzone_script.name + " is now " + (Safetyzone_script.enabled ? "Enabled" : "Disabled"));
        }
        }
}
