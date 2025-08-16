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
    [SerializeField] private int[] combo;
    [SerializeField] private Button[] upButtons;
    [SerializeField] private Button[] downButtons;
    [SerializeField] private TMP_Text[] numDisplays;

    public void Awake()
    {
        HidePuzzle();
    }

    public void ShowPuzzle()
    {
        foreach (Button button in upButtons)
        {
            button.gameObject.SetActive(true);
        }
        foreach (Button button in downButtons)
        {
            button.gameObject.SetActive(true);
        }
        foreach (TMP_Text text in numDisplays)
        {
            text.gameObject.SetActive(true);
        }

        foreach (TMP_Text text in numDisplays)
        {
            text.text = "0";
        }
    }

    public void HidePuzzle()
    {
        foreach (Button button in upButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (Button button in downButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (TMP_Text text in numDisplays)
        {
            text.gameObject.SetActive(false);
        }
    }

    public void ButtonIncrease(int index)
    {
        if (combo[index] == 9)
        {
            combo[index] = 0;
        }
        else
        {
            combo[index]++;
        }
        numDisplays[index].text = combo[index].ToString();
    }

    public void ButtonDecrease(int index)
    {
        if (combo[index] == 0)
        {
            combo[index] = 9;
        }
        else
        {
            combo[index]--;
        }
        numDisplays[index].text = combo[index].ToString();
    }

    private void CheckIfSolved()
    {
        if (numDisplays[0].text == combo[0].ToString() && numDisplays[1].text == combo[1].ToString() && numDisplays[2].text == combo[2].ToString() && numDisplays[3].text == combo[3].ToString())
        {
            Debug.Log("PUZZLE SOLVED");
        }
    }
}
