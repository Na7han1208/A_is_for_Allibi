using System.Linq;
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
    [SerializeField] private int[] correctCombo;
    private int[] currentCombo = {0,0,0,0};
    

    [SerializeField] private Button[] upButtons;
    [SerializeField] private Button[] downButtons;
    [SerializeField] private TMP_Text[] numDisplays;
    [SerializeField] private Button returnButton;

    public void Awake()
    {
        HidePuzzle();
    }

    public void ShowPuzzle()
    {
        FPController controller = FindFirstObjectByType<FPController>();
        if (controller != null)
        {
            controller.isInspecting = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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
        returnButton.gameObject.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            numDisplays[i].text = currentCombo[i].ToString();
        }
    }

    public void HidePuzzle()
    {
        FPController controller = FindFirstObjectByType<FPController>();
        if (controller != null)
        {
            controller.isInspecting = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

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
        returnButton.gameObject.SetActive(false);
    }

    public void ButtonIncrease(int index)
    {
        if (currentCombo[index] == 9)
        {
            currentCombo[index] = 0;
        }
        else
        {
            currentCombo[index]++;
        }
        numDisplays[index].text = currentCombo[index].ToString();
        CheckIfSolved();
    }

    public void ButtonDecrease(int index)
    {
        if (currentCombo[index] == 0)
        {
            currentCombo[index] = 9;
        }
        else
        {
            currentCombo[index]--;
        }
        numDisplays[index].text = currentCombo[index].ToString();
        CheckIfSolved();
    }

    private void CheckIfSolved()
    {
        if (currentCombo.SequenceEqual(correctCombo))
        {
            Debug.Log("PUZZLE SOLVED");
            HidePuzzle();
            SoundManager.Instance.Play("Unlock", this.transform);
            this.enabled = false;
        }
    }
}
