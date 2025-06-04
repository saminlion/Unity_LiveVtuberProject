using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class EnvironmentManager : MonoBehaviour
{
    public Camera mainCam;
    public GameObject mainLight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitCamera();
        InitLight();
    }

    void InitCamera()
    {
        if (mainCam == null) return;

        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = Color.black;
    }

    void InitLight()
    {
        if (mainLight == null) return;

        var light = mainLight.GetComponent<Light>();

        light.color = Color.white;
        light.intensity = 1.1f;
        
        mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
