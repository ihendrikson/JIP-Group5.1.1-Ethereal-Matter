using UnityEngine;
using UnityEngine.UI;

public class BananaManSkinChanger : MonoBehaviour
{
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private Material[] skinStyles;
    private int currentStyleIndex = 0;

    public Button nextSkinButton;
    public Button previousSkinButton;
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    // Add a variable to store the folder path for the skin materials
    public string materialFolderPath = "Materials/Skinstyles";  // Default path, but can be customized

    void Start()
    {
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
        {
            Debug.LogError("No SkinnedMeshRenderer components found!");
            return;
        }

        foreach (var renderer in skinnedMeshRenderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(materials[i]);
            }
            renderer.materials = materials;
        }

        // Load materials from the specified folder
        LoadSkinMaterials(materialFolderPath);

        if (nextSkinButton != null)
        {
            nextSkinButton.onClick.AddListener(OnNextSkinButtonClicked);
        }

        if (previousSkinButton != null)
        {
            previousSkinButton.onClick.AddListener(OnPreviousSkinButtonClicked);
        }

        if (redSlider != null)
        {
            redSlider.onValueChanged.AddListener(ColorChange);
        }

        if (greenSlider != null)
        {
            greenSlider.onValueChanged.AddListener(ColorChange);
        }

        if (blueSlider != null)
        {
            blueSlider.onValueChanged.AddListener(ColorChange);
        }
    }

    void LoadSkinMaterials(string folderPath)
    {
        // Load all materials from the provided folder path
        skinStyles = Resources.LoadAll<Material>(folderPath);

        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("No materials loaded. Ensure materials are stored in " + folderPath);
        }
        else
        {
            Debug.Log("Successfully loaded " + skinStyles.Length + " materials from " + folderPath);
        }

        ChangeSkinStyle(0);
    }

    public void ColorChange(float value)
    {
        OnColorSliderChanged(redSlider.value, greenSlider.value, blueSlider.value);
    }

    public void ChangeSkinStyle(int styleIndex)
    {
        if (skinnedMeshRenderers != null && skinStyles.Length > styleIndex)
        {
            foreach (var renderer in skinnedMeshRenderers)
            {
                Material[] materials = renderer.materials;
                materials[0] = new Material(skinStyles[styleIndex]);
                renderer.materials = materials;
            }
            currentStyleIndex = styleIndex;
        }
    }



    public void OnNextSkinButtonClicked()
    {
        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("Skin styles array is empty. Please assign materials to the skinStyles array.");
            return;
        }
        int nextStyleIndex = (currentStyleIndex + 1) % skinStyles.Length;
        ChangeSkinStyle(nextStyleIndex);
    }

    public void OnPreviousSkinButtonClicked()
    {
        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("Skin styles array is empty. Please assign materials to the skinStyles array.");
            return;
        }
        int previousStyleIndex = (currentStyleIndex - 1 + skinStyles.Length) % skinStyles.Length;
        ChangeSkinStyle(previousStyleIndex);
    }

    public void OnColorSliderChanged(float value_red, float value_green, float value_blue)
    {
        if (skinnedMeshRenderers != null)
        {
            foreach (var renderer in skinnedMeshRenderers)
            {
                Material[] materials = renderer.materials;
                if (materials == null || materials.Length == 0)
                {
                    Debug.LogError("SkinnedMeshRenderer has no materials!");
                    continue;
                }

                Color newColor = new Color(value_red, value_green, value_blue, materials[0].color.a);
                materials[0].color = newColor;
                renderer.materials = materials;
            }
        }
        else
        {
            Debug.LogError("skinnedMeshRenderers is null, cannot adjust color!");
        }
    }

    // Optional: Call this function to dynamically change the material folder path at runtime
    public void SetMaterialFolderPath(string newPath)
    {
        materialFolderPath = newPath;
        LoadSkinMaterials(materialFolderPath);
    }
}
