using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class MultiColourTracingPuzzle : TracingPuzzle
{
    [Header("Multi Colour Puzzle Settings")]
    [SerializeField] private RawImage[] maskImages;
    [SerializeField] private RawImage[] suspectDisplays;
    [SerializeField] private Sprite[] crayonColors;
    [SerializeField] private Image crayonCursor;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button blackButton;
    [SerializeField] private float transitionDelay = 3f;
    [SerializeField] private RectTransform virtualCursor;

    [Header("Per-Stage Completion Thresholds (0-1)")]
    [SerializeField] private float[] completionThresholds;

    private int currentDrawingIndex = 0;
    private int selectedCrayonIndex = -1;
    private bool transitioning = false;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerData;
    private EventSystem eventSystem;
    private GameObject currentHover;

    protected override void Start()
    {
        if (maskImages != null && maskImages.Length > 0)
            maskImage = maskImages[0];

        base.Start();

        if (yellowButton != null) yellowButton.onClick.AddListener(() => SelectCrayon(0));
        if (redButton != null) redButton.onClick.AddListener(() => SelectCrayon(1));
        if (blueButton != null) blueButton.onClick.AddListener(() => SelectCrayon(2));
        if (blackButton != null) blackButton.onClick.AddListener(() => SelectCrayon(3));

        for (int i = 0; i < maskImages.Length; i++)
        {
            if (maskImages[i] != null)
                maskImages[i].gameObject.SetActive(i == 0);

            if (suspectDisplays != null && i < suspectDisplays.Length && suspectDisplays[i] != null)
                suspectDisplays[i].gameObject.SetActive(i == 0);
        }

        currentDrawingIndex = 0;
        selectedCrayonIndex = -1;

        raycaster = GetComponentInChildren<GraphicRaycaster>(true);
        eventSystem = EventSystem.current;
    }

    private void LateUpdate()
    {
        if (virtualCursor == null || raycaster == null || eventSystem == null)
            return;

        Vector2 cursorPosition = virtualCursor.position;
        pointerData = new PointerEventData(eventSystem)
        {
            position = cursorPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        GameObject hitObject = results.Count > 0 ? results[0].gameObject : null;

        if (hitObject != currentHover)
        {
            if (currentHover != null)
                ExecuteEvents.Execute(currentHover, pointerData, ExecuteEvents.pointerExitHandler);

            if (hitObject != null)
                ExecuteEvents.Execute(hitObject, pointerData, ExecuteEvents.pointerEnterHandler);

            currentHover = hitObject;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && hitObject != null)
        {
            ExecuteEvents.Execute(hitObject, pointerData, ExecuteEvents.pointerClickHandler);
        }
    }

    private void SelectCrayon(int index)
    {
        selectedCrayonIndex = index;

        if (crayonCursor != null && index >= 0 && index < crayonColors.Length)
            crayonCursor.sprite = crayonColors[index];
    }

    protected override void TryStamp()
    {
        if (!CanDrawOnCurrent())
            return;

        base.TryStamp();
    }

    private bool CanDrawOnCurrent()
    {
        if (selectedCrayonIndex == -1)
            return false;

        return selectedCrayonIndex == currentDrawingIndex;
    }

    public override bool CheckCompletion()
    {
        if (transitioning)
            return false;

        float required = 1f;

        if (completionThresholds != null && currentDrawingIndex < completionThresholds.Length)
            required = Mathf.Clamp01(completionThresholds[currentDrawingIndex]);

        float current = GetCurrentCompletion();

        bool completed = current >= required;

        if (completed)
        {
            StopAllCoroutines();
            StartCoroutine(HandleDrawingComplete());
            finishedCalled = false;
        }

        return completed;
    }

    private float GetCurrentCompletion()
    {
        if (maskImage == null || maskImage.texture == null)
            return 0f;

        Texture src = maskImage.texture;
        int w = 512;
        int h = 512;

        if (src != null && src.width > 0 && src.height > 0)
        {
            w = src.width;
            h = src.height;
        }

        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tmp = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tmp.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tmp.Apply(false);

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        Color32[] pixels;
        try
        {
            pixels = tmp.GetPixels32();
        }
        catch
        {
            UnityEngine.Object.DestroyImmediate(tmp);
            return 0f;
        }

        int sampleTarget = Mathf.Clamp(w / 4, 16, 256);
        int step = Mathf.Max(1, w / sampleTarget);
        int transparent = 0;
        int total = 0;

        for (int y = 0; y < h; y += step)
        {
            int baseIdx = y * w;
            for (int x = 0; x < w; x += step)
            {
                total++;
                if (pixels[baseIdx + x].a < 51) transparent++;
            }
        }

        UnityEngine.Object.DestroyImmediate(tmp);

        if (total == 0) return 0f;
        return (float)transparent / total;
    }

    private IEnumerator HandleDrawingComplete()
    {
        transitioning = true;

        switch (currentDrawingIndex)
        {
            case 0: OnYellowComplete(); break;
            case 1: OnRedComplete(); break;
            case 2: OnBlueComplete(); break;
            case 3: OnBlackComplete(); break;
        }

        yield return new WaitForSeconds(transitionDelay);

        currentDrawingIndex++;

        if (currentDrawingIndex >= maskImages.Length)
        {
            OnAllDrawingsComplete();
            yield break;
        }

        for (int i = 0; i < maskImages.Length; i++)
        {
            bool active = (i == currentDrawingIndex);

            if (maskImages[i] != null)
                maskImages[i].gameObject.SetActive(active);

            if (suspectDisplays != null && i < suspectDisplays.Length && suspectDisplays[i] != null)
                suspectDisplays[i].gameObject.SetActive(active);
        }

        maskImage = maskImages[currentDrawingIndex];
        if (maskImage != null) maskImage.enabled = true;
        InitMask();
        selectedCrayonIndex = -1;
        transitioning = false;
    }

    private void OnYellowComplete()
    {
        Debug.Log("Yellow suspect traced!");
    }

    private void OnRedComplete()
    {
        Debug.Log("Red suspect traced!");
    }

    private void OnBlueComplete()
    {
        Debug.Log("Blue suspect traced!");
    }

    private void OnBlackComplete()
    {
        Debug.Log("Black suspect traced!");
    }

    private void OnAllDrawingsComplete()
    {
        Debug.Log("All suspects traced!");
    }
}
