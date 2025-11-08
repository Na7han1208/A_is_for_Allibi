using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

[Serializable]
public class StarChartUI
{
    public string chartId;
    public GameObject chartUI;
    public List<Star> goldStars = new List<Star>();
    public List<Star> silverStars = new List<Star>();
}

public class StarChartManager : MonoBehaviour
{
    public static StarChartManager Instance;

    [Header("Charts")]
    [SerializeField] private List<StarChartUI> starCharts = new List<StarChartUI>();

    [Header("Settings")]
    [SerializeField] private bool startHidden = true;

    public event Action OnAllGoldUnlocked;

    private StarChartUI currentChart;
    private bool isChartVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (startHidden)
            HideAllCharts(true);
    }

    public void OnStarChartPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentChart == null)
        {
            Debug.Log("out of chart range");
            return;
        }

        if (isChartVisible)
            HideAllCharts(true);
        else
            ShowChart(currentChart.chartId);
    }

    public void SetCurrentChart(string chartId)
    {
        currentChart = starCharts.Find(c => c.chartId == chartId);
    }

    public void ClearCurrentChart()
    {
        currentChart = null;
        HideAllCharts(true);
    }

    public void ShowChart(string chartId)
    {
        HideAllCharts(true);

        var chart = starCharts.Find(c => c.chartId == chartId);
        if (chart == null || chart.chartUI == null)
        {
            Debug.LogWarning("chart not found");
            return;
        }

        chart.chartUI.SetActive(true);
        isChartVisible = true;

        Cursor.lockState = CursorLockMode.None;
        CursorManager.Instance?.ShowCursor(true);
    }

    public void HideAllCharts(bool force)
    {
        foreach (var chart in starCharts)
        {
            if (chart.chartUI != null)
                chart.chartUI.SetActive(false);
        }

        isChartVisible = false;

        Cursor.lockState = CursorLockMode.None;
        CursorManager.Instance?.ShowCursor(false);
    }

    public bool AnyChartOpen()
    {
        foreach (var chart in starCharts)
        {
            if (chart.chartUI != null && chart.chartUI.activeSelf)
                return true;
        }
        return false;
    }

    public void UnlockStar(string starId)
    {
        foreach (var chart in starCharts)
        {
            foreach (var star in chart.goldStars)
            {
                if (star != null && star.starId == starId)
                {
                    star.Unlock();
                    CheckGoldCompletion(chart);
                    return;
                }
            }

            foreach (var star in chart.silverStars)
            {
                if (star != null && star.starId == starId)
                {
                    star.Unlock();
                    return;
                }
            }
        }

        Debug.LogWarning("starid not found");
    }

    private void CheckGoldCompletion(StarChartUI chart)
    {
        foreach (var star in chart.goldStars)
        {
            if (star == null || !star.IsUnlocked)
                return;
        }

        OnAllGoldUnlocked?.Invoke();
    }
}
