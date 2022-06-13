using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayLight : MonoBehaviour
{
    public GameObject fieldObject;
    
    Field environment;
    Light lightComp;
    public Material daySkybox;
    public Material midnightSkybox;
    public Material noonSkybox;
    public Material nightSkybox;

    public float morningTime;
    public float eveningTime;
    public float angleInTime;
    public bool isDay = false;

    Quaternion currentRotation;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.transform.rotation.eulerAngles);
        currentRotation.eulerAngles = new Vector3(150.0f, -30.0f, 0.0f);
        this.transform.rotation = currentRotation;

        environment = fieldObject.GetComponent<Field>();
        lightComp = this.GetComponent<Light>();

        morningTime = environment.timeInDay / 24 * 6;
        eveningTime = environment.timeInDay / 24 * 18;
        angleInTime = 120 / environment.timeInDay * 2; // 120 * 2
    }

    public void onNoonLight()
    {
        RenderSettings.skybox = noonSkybox;
    }

    public void onMidnightLight()
    {
        RenderSettings.skybox = midnightSkybox;
    }

    public void onDayLight()
    {
        RenderSettings.skybox = daySkybox;
        Debug.Log("is day!");
        currentRotation.eulerAngles = new Vector3(150.0f, -30.0f, 0.0f);
        this.transform.rotation = currentRotation;

        lightComp.color = new Color(255.0f / 256.0f, 244.0f / 256.0f, 214.0f / 256.0f);
        lightComp.intensity = 1.0f;
        isDay = true;
    }

    public void onNightLight()
    {
        RenderSettings.skybox = nightSkybox;
        currentRotation.eulerAngles = new Vector3(150.0f, -30.0f, 0.0f);
        this.transform.rotation = currentRotation;
        
        lightComp.color = new Color(128.0f / 256.0f, 49.0f / 256.0f, 120.0f / 256.0f);
        lightComp.intensity = 0.7f;
        isDay = false;
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    private void FixedUpdate()
    {
        this.transform.Rotate(-1 * angleInTime * Time.deltaTime, 0.0f, 0.0f, Space.Self);
    }
}
