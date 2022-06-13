using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIText : MonoBehaviour
{
    public GameObject fieldObject;
    public GameObject userObject;

    public Texture sunny;
    public Texture rainy;
    public Texture night;

    Field environment;
    
    Text timeText;
    Text dateText;
    RawImage whetherImage;

    int minInHour;

    // Start is called before the first frame update
    void Start()
    {
        environment = fieldObject.GetComponent<Field>();
        timeText = GameObject.Find("TimeText").GetComponent<Text>();
        dateText = GameObject.Find("DateText").GetComponent<Text>();
        whetherImage = GameObject.Find("Weather").GetComponent<RawImage>();

        if (environment.isDay) whetherImage.texture = sunny;
        else whetherImage.texture = night;

        minInHour = (int)environment.timeInDay / 24;
    }

    // Update is called once per frame
    void Update()
    {
        timeText.text = string.Format("{0:D2}:{1:D2}", (int)(environment.time / minInHour), (int)((environment.time % minInHour) / minInHour * 60) / 5 * 5);
        dateText.text = environment.date + "ÀÏÂ÷";

        if (environment.isDay) whetherImage.texture = sunny;
        else whetherImage.texture = night;
    }
}
