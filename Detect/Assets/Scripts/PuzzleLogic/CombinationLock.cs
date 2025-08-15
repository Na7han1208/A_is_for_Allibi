using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


/*
    Place this on each item you wish to be a combination lock

    NOTE: This only works for a 4 digit combination lock
*/
public class CombinationLock : MonoBehaviour
{
    [SerializeField] private int Combination;

    private GameObject canvasGameObject;
    private Canvas canvas;

    private GameObject[] images;
    private GameObject[] texts;
    private GameObject[] upButtons;
    private GameObject[] downButtons;

    public void Initialise()
    {
        canvasGameObject = new GameObject("CombinationLock");
        canvas = canvasGameObject.AddComponent<Canvas>();
        canvasGameObject.AddComponent<CanvasScaler>();
        canvasGameObject.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        //Add all necessary components
        for (int i = 0; i < Combination.ToString().Length; i++)
        {
            //Add Image
            GameObject imageGameObject = new GameObject("NumberBackground" + i);
            imageGameObject.AddComponent<Image>();
            imageGameObject.transform.SetParent(canvas.transform, false);
            images[i] = imageGameObject;
            //Assign up arrow sprite

            //Add Text
            GameObject textGameObject = new GameObject("NumberDisplay" + i);
            textGameObject.AddComponent<TMP_Text>();
            textGameObject.transform.SetParent(canvas.transform, false);
            textGameObject.GetComponent<TMP_Text>().text = Combination.ToString()[i].ToString();
            texts[i] = textGameObject;


            //Add up button
            GameObject upButtonGameObject = new GameObject("UpButton" + i);
            upButtonGameObject.AddComponent<Button>();
            upButtonGameObject.transform.SetParent(canvas.transform, false);
            upButtons[i] = upButtonGameObject;

            //Add down button
            GameObject downButtonGameObject = new GameObject("DownButton" + i);
            downButtonGameObject.AddComponent<Button>();
            downButtonGameObject.transform.SetParent(canvas.transform, false);
            downButtons[i] = downButtonGameObject;
        }

        //Calculate necessary positions
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        int comboLength = Combination.ToString().Length;

        int x_Distance = screenWidth / (comboLength + 2);
        int x_Current = 0;

        //Place components in correct position
        for (int i = 0; i < comboLength; i++)
        {
            x_Current += x_Distance;
            images[i].GetComponent<Image>().rectTransform.anchoredPosition = new UnityEngine.Vector2(x_Current, screenHeight / 2);
            texts[i].GetComponent<TMP_Text>().rectTransform.anchoredPosition = new UnityEngine.Vector2(x_Current, screenHeight / 2);

            upButtons[i].GetComponent<Button>().GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector2(x_Current, screenHeight / 2 + 50);
            downButtons[i].GetComponent<Button>().GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector2(x_Current, screenHeight / 2 - 50);
        }
    }
}
