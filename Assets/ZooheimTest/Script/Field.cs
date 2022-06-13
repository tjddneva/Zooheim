using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.Events;

public class Field : MonoBehaviour
{
    public GameObject fieldObject;
    private GameObject field;

    public GameObject GrassPrefab1;
    public GameObject GrassPrefab2;
    public GameObject TreePrefab;

    public GameObject GrassParent;
    public GameObject WoodParent;

    public UnityEvent onDay;
    public UnityEvent onNight;
    public UnityEvent onNoon;
    public UnityEvent onMidnight;

    public int width = 100;
    public int height = 100;

    public int temperature = 23;     // 온도
    public int precipitation;   // 강수량
    public bool isRainy = false;
    public bool isDay;
    bool isNoon = false;
    bool isMidnight = false;

    public int date = 0;            // accumulated date
    public float time = 720.0f;            // 1sec in realtime == 1min in game. 24h is 1440
    public float timeInDay = 1440.0f;

    public float[,] ForestMap;
    public float[,] GrassMap;

    // Start is called before the first frame update
    public void CreateWorld() {
        // Create field
        field = Instantiate(fieldObject);
        field.GetComponent<Transform>().localScale = new Vector3(width, 1, height);

        // Initialize
        date = 0;

        temperature = 20;
        precipitation = 30;

        initArea(width, height, 8);
        initGrass();
        initTree();

        if (time >= timeInDay / 24 * 6 && time < timeInDay / 24 * 18) isDay = true;
        else isDay = false;

        WoodParent = GameObject.Find("WoodParent");
        GrassParent = GameObject.Find("GrassParent");
    }

    private void FixedUpdate()
    {   
        time += Time.deltaTime;
        
        if(time >= timeInDay)
        {
            time = 0;
            date++;
        }

        // Day / night
        if (time >= timeInDay / 24 * 6 && time < timeInDay / 24 * 18) // day
        {
            if (!isDay)
            {
                onDay.Invoke();
                isDay = true;
                isMidnight = false;
            }
        }
        else if (isDay) // night 18 o clock
        {
            onNight.Invoke();
            isDay = false;
            isNoon = false;            
        }
        
        // Noon / sunset
        if (((time >= timeInDay / 24 * 21 && time < timeInDay / 24 * 24) || (time >= timeInDay / 24 * 0 && time < timeInDay / 24 * 6)) && !isMidnight) // midnight
        {
            isMidnight = true;
            onMidnight.Invoke();
        }
        if (time >= timeInDay / 24 * 9 && time < timeInDay / 24 * 18 && !isNoon) // noon
        {
            isNoon = true;
            onNoon.Invoke();
        }
    }

    
    private void initArea(int width, int height, int scale)
    {
        ForestMap = new float[width, height];
        GrassMap = new float[width, height];


        // Generate Forest Area noise map
        float dx = Random.Range(0.0f, 1000.0f);
        float dy = Random.Range(0.0f, 1000.0f);
        Debug.Log(dx + " " + dy);
        for (float x = 0.0f; x < width; x++)
        {
            for (float y = 0.0f; y < height; y++)
            {
                float value = Mathf.PerlinNoise(dx + x/width*scale, dy + y/height*scale);
                
                if (value > 0.5f)
                {
                    ForestMap[(int)x, (int)y] = 0.04f;
                }
                else
                {
                    ForestMap[(int)x, (int)y] = 0.01f;
                }
            }
        }
        

        // Generate Grassland Area noise map
        dx = Random.Range(0.0f, 1000.0f);
        dy = Random.Range(0.0f, 1000.0f);

        Debug.Log(dx + " " + dy);
        for (float x = 0.0f; x < width; x++)
        {
            for (float y = 0.0f; y < height; y++)
            {
                float value = Mathf.PerlinNoise(dx + x / width * scale, dy + y / height * scale);

                if (value > 0.5f)
                {
                    GrassMap[(int)x, (int)y] = 0.2f;
                }
                else
                {
                    GrassMap[(int)x, (int)y] = 0.05f;
                }
            }
        }

    
        // Generate Lake Area noise map

    }

    private void initGrass()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (Random.Range(0.0f, 1.0f) < GrassMap[i, j])
                {
                    float x = Random.Range(-0.5f, 0.5f);
                    float z = Random.Range(-0.5f, 0.5f);

                    if (Random.Range(0.0f, 1.0f) < 0.5f)
                    {
                        Instantiate(GrassPrefab1, new Vector3(x + j - width/2, 0, z + i-height/2), Quaternion.identity, GrassParent.transform);
                    }
                    else
                    {
                        Instantiate(GrassPrefab2, new Vector3(x + j - width / 2, 0, z + i - height / 2), Quaternion.identity, GrassParent.transform);
                    }
                }
            }
        }
    }

    private void initTree()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (Random.Range(0.0f, 1.0f) < ForestMap[i, j])
                {
                    float x = Random.Range(-0.5f, 0.5f);
                    float z = Random.Range(-0.5f, 0.5f);

                    Instantiate(TreePrefab, new Vector3(x + j - width / 2, 0, z + i - height / 2), Quaternion.identity, WoodParent.transform);
                }
            }
        }
    }

    public void ClearWorld() {
        GameObject[] ClearTarget;
        ClearTarget = GameObject.FindGameObjectsWithTag("Ground");
        foreach(var item in ClearTarget) Destroy(item);
        ClearTarget = GameObject.FindGameObjectsWithTag("DeadTree");
        foreach(var item in ClearTarget) Destroy(item);
        ClearTarget = GameObject.FindGameObjectsWithTag("Tree");
        foreach(var item in ClearTarget) Destroy(item);
        ClearTarget = GameObject.FindGameObjectsWithTag("Grass");
        foreach(var item in ClearTarget) Destroy(item);
        
    }
}
