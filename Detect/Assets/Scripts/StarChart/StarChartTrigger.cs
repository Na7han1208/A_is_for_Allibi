using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StarChartTrigger : MonoBehaviour
{
    [Header("Chart Settings")]
    public string chartId;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        StarChartManager.Instance?.SetCurrentChart(chartId);
        Debug.Log($"Entered chart zone: {chartId}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        StarChartManager.Instance?.ClearCurrentChart();
        Debug.Log($"Exited chart zone: {chartId}");
    }
}
