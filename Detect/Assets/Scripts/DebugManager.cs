using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private TMP_Text debugTextUI;

    private float deltaTime;
    private StringBuilder debugText = new StringBuilder();

    void Update()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        float fps = 1.0f / deltaTime;
        string resolution = $"{Screen.width}x{Screen.height}";

        Vector3 pos = playerTransform ? playerTransform.position : Vector3.zero;
        Vector3 rot = playerTransform ? playerTransform.eulerAngles : Vector3.zero;

        debugText.Clear();

        debugText.AppendLine($"Player Pos: X:{pos.x:0.00} Y:{pos.y:0.00} Z:{pos.z:0.00}");
        debugText.AppendLine($"Player Rot: X:{rot.x:0.00} Y:{rot.y:0.00} Z:{rot.z:0.00}");

        debugText.AppendLine($"Resolution: {resolution}");
        debugText.AppendLine($"FPS: {fps:0.0}");

        if (debugTextUI != null)
            debugTextUI.text = debugText.ToString();
    }
}