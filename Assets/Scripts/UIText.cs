using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIText : MonoBehaviour
{
    Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        switch (MouseController.Instance.formation)
        {
            case FormationState.None:
                text.text = "None";
                return;
            case FormationState.Line:
                text.text = "Line";
                return;
            case FormationState.Square:
                text.text = "Square";
                return;
            case FormationState.Circle:
                text.text = "Circle";
                return;
        }
    }
}
