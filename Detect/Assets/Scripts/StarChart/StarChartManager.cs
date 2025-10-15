using UnityEngine;
using System;
using System.Collections.Generic;

public class StarChartManager : MonoBehaviour
{
    public static StarChartManager Instance;

    [Header("UI References")]
    public GameObject chartUI;

    [Header("Stars")]
    public List<Star> goldStars;
    public List<Star> silverStars;

    public event Action OnAllGoldUnlocked;

    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        if (chartUI != null)
            chartUI.SetActive(false);
    }

    public void ToggleStarChart()
    {
        if (chartUI == null) return;

        isOpen = !isOpen;
        chartUI.SetActive(isOpen);

        if (isOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UnlockStar(string starId)
    {
        foreach (var star in goldStars)
        {
            if (star.starId == starId)
            {
                star.Unlock();
                CheckGoldCompletion();
                return;
            }
        }

        foreach (var star in silverStars)
        {
            if (star.starId == starId)
            {
                star.Unlock();
                return;
            }
        }

        Debug.LogWarning($"Star ID '{starId}' not found!");
    }

    private void CheckGoldCompletion()
    {
        foreach (var star in goldStars)
        {
            if (!star.IsUnlocked) return;
        }

        OnAllGoldUnlocked?.Invoke();
        Debug.Log("All gold stars unlocked!");
    }
}