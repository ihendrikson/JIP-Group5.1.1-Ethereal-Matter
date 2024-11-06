//using UnityEngine;

//public class WebCamInput : MonoBehaviour
//{
//    [SerializeField] string webCamName;
//    [SerializeField] Vector2 webCamResolution = new Vector2(1920, 1080);
//    [SerializeField] Texture staticInput;

//    // Provide input image Texture.
//    public Texture inputImageTexture
//    {
//        get
//        {
//            if (staticInput != null) return staticInput;
//            return inputRT;
//        }
//    }

//    WebCamTexture webCamTexture;
//    RenderTexture inputRT;

//    void Start()
//    {
//        if (staticInput == null)
//        {
//            webCamTexture = new WebCamTexture("Orbbec", (int)webCamResolution.x, (int)webCamResolution.y);
//            webCamTexture.deviceName = "Orbbec Femto Bolt RGB Camera";
//            webCamTexture.Play();
//        }

//        inputRT = new RenderTexture((int)webCamResolution.x, (int)webCamResolution.y, 0);
//    }

//    void Update()
//    {
//        if (staticInput != null) return;
//        if (!webCamTexture.didUpdateThisFrame) return;

//        var aspect1 = (float)webCamTexture.width / webCamTexture.height;
//        var aspect2 = (float)inputRT.width / inputRT.height;
//        var aspectGap = aspect2 / aspect1;

//        var vMirrored = webCamTexture.videoVerticallyMirrored;
//        var scale = new Vector2(aspectGap, vMirrored ? -1 : 1);
//        var offset = new Vector2((1 - aspectGap) / 2, vMirrored ? 1 : 0);

//        Graphics.Blit(webCamTexture, inputRT, scale, offset);
//    }

//    void OnDestroy()
//    {
//        if (webCamTexture != null) Destroy(webCamTexture);
//        if (inputRT != null) Destroy(inputRT);
//    }
//}

using UnityEngine;

public class WebCamInput : MonoBehaviour
{
    [SerializeField] private string webCamName; // Set this in the Inspector with the desired device name
    [SerializeField] private Vector2 webCamResolution = new Vector2(1920, 1080);
    [SerializeField] private Texture staticInput;

    // Provide input image Texture.
    public Texture inputImageTexture => staticInput != null ? staticInput : inputRT;

    private WebCamTexture webCamTexture;
    private RenderTexture inputRT;

    void Start()
    {
        if (staticInput == null)
        {
            // Dynamically find and assign the webcam device
            WebCamDevice[] devices = WebCamTexture.devices;
            bool deviceFound = false;

            foreach (var device in devices)
            {
                Debug.Log("Available device: " + device.name);
                if (device.name == webCamName)
                {
                    webCamTexture = new WebCamTexture(device.name, (int)webCamResolution.x, (int)webCamResolution.y);
                    webCamTexture.Play();
                    Debug.Log("Using device: " + device.name);
                    deviceFound = true;
                    break;
                }
            }

            if (!deviceFound)
            {
                Debug.LogError("Device not found: " + webCamName);
                return;
            }
        }

        // Initialize RenderTexture
        inputRT = new RenderTexture((int)webCamResolution.x, (int)webCamResolution.y, 0);
        if (inputRT == null)
        {
            Debug.LogError("Failed to create RenderTexture.");
            return;
        } 
        else
        {
            Debug.Log("Initialized RenderTexture");
        }
    }

    void Update()
    {
        if (staticInput != null || webCamTexture == null || inputRT == null) return;

        if (!webCamTexture.didUpdateThisFrame) return;

        // Calculate aspect ratio adjustment
        float aspect1 = (float)webCamTexture.width / webCamTexture.height;
        float aspect2 = (float)inputRT.width / inputRT.height;
        float aspectGap = aspect2 / aspect1;

        // Apply mirroring and aspect scaling
        bool vMirrored = webCamTexture.videoVerticallyMirrored;
        Vector2 scale = new Vector2(aspectGap, vMirrored ? -1 : 1);
        Vector2 offset = new Vector2((1 - aspectGap) / 2, vMirrored ? 1 : 0);

        Graphics.Blit(webCamTexture, inputRT, scale, offset);
    }

    void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
        }
        if (inputRT != null) Destroy(inputRT);
    }
}
