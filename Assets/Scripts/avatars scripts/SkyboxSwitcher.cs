using UnityEngine;
using UnityEngine.UI; 

public class SkyboxSwitcher : MonoBehaviour
{
    public Button nextSkyboxButton; 
    public Button previousSkyboxButton; 

    private Material[] skyboxMaterials;
    private int currentSkyboxIndex = 0; 

    private void Start()
    {
        skyboxMaterials = Resources.LoadAll<Material>("Skyboxes");
        
        // Check if any skybox materials were loaded successfully
        if (skyboxMaterials.Length > 0)
        {
            RenderSettings.skybox = skyboxMaterials[currentSkyboxIndex];
        }
        else
        {
            Debug.LogWarning("No skybox materials found. Please ensure materials are located in Resources/Skyboxes folder.");
        }

        // Bind click events to buttons
        if (nextSkyboxButton != null)
            nextSkyboxButton.onClick.AddListener(NextSkybox);

        if (previousSkyboxButton != null)
            previousSkyboxButton.onClick.AddListener(PreviousSkybox);
    }

    // Switch to the next skybox
    public void NextSkybox()
    {
        if (skyboxMaterials.Length == 0) return;
        
        currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxMaterials.Length;
        RenderSettings.skybox = skyboxMaterials[currentSkyboxIndex];
    }

    // Switch to the previous skybox
    public void PreviousSkybox()
    {
        if (skyboxMaterials.Length == 0) return;
        
        currentSkyboxIndex = (currentSkyboxIndex - 1 + skyboxMaterials.Length) % skyboxMaterials.Length;
        RenderSettings.skybox = skyboxMaterials[currentSkyboxIndex];
    }
}
