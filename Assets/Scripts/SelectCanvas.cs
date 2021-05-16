using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCanvas : MonoBehaviour
{
    public static SelectCanvas selectCanvas;
    public enum selectMode { drag, select, turn}
    public Sprite highlightArrow, selectArrow, selectTurnArrow;

    private selectMode mode;
    private void Awake()
    {
        selectCanvas = this;
    }
    private void ChangeAllImages(Sprite newSprite)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Image>().sprite = newSprite;
        }
    }

    public void ChangeMode(selectMode newMode)
    {
        switch(newMode)
        {
            case selectMode.drag:
                //ChangeAllImages(highlightArrow);
                break;

            case selectMode.select:
                //ChangeAllImages(selectArrow);
                break;

            case selectMode.turn:
                //ChangeAllImages(selectTurnArrow);
                break;
        }
    }

    private void UpdateLook()
    {
        if (Camera.main)
            transform.LookAt(Camera.main.transform.position, transform.forward);
    }

    private void Update()
    {
        //UpdateLook();

    }
}
