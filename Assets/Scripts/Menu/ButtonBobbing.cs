using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBobbing : MonoBehaviour
{
    float initialY;
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialY = rectTransform.anchoredPosition.y;
    }

    void Update()
    {
        rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, initialY + Mathf.Sin(Time.time * 4f) * 10f, 0);
    }
}
