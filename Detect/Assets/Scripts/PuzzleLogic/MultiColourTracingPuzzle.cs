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

    private bool prevIsDrawing = false;

    private RenderTexture stableBuffer;
    private Texture2D readTexture;

    [Header("Timelines")]
    public GameObject NiceDayTimeline;
    public GameObject EddyKitchentimeline;

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
        prevIsDrawing = false;
        InitStableBuffer();
    }

    private void InitStableBuffer()
    {
        ReleaseStableBuffer();
        if (maskImage != null && maskImage.texture != null)
        {
            int w = Mathf.Max(1, maskImage.texture.width);
            int h = Mathf.Max(1, maskImage.texture.height);
            stableBuffer = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
            stableBuffer.Create();
            Graphics.Blit(maskImage.texture, stableBuffer);
            if (readTexture != null && (readTexture.width != w || readTexture.height != h))
            {
                UnityEngine.Object.DestroyImmediate(readTexture);
                readTexture = null;
            }
            if (readTexture == null)
                readTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        }
    }

    private void ReleaseStableBuffer()
    {
        if (stableBuffer != null)
        {
            stableBuffer.Release();
            UnityEngine.Object.DestroyImmediate(stableBuffer);
            stableBuffer = null;
        }
        if (readTexture != null)
        {
            UnityEngine.Object.DestroyImmediate(readTexture);
            readTexture = null;
        }
    }

    private void LateUpdate()
    {
        if (virtualCursor == null || raycaster == null || eventSystem == null)
            return;

        Vector2 cursorPosition = virtualCursor.position;
        pointerData = new PointerEventData(eventSystem) { position = cursorPosition };

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

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && hitObject != null)
            ExecuteEvents.Execute(hitObject, pointerData, ExecuteEvents.pointerClickHandler);

        bool nowDrawing = isDrawing;
        if (nowDrawing != prevIsDrawing)
        {
            if (nowDrawing)
            {
                prevIsDrawing = true;
            }
            else
            {
                prevIsDrawing = false;
                UpdateStableBufferFromMask();
                CheckCompletion();
            }
        }
    }

    private void UpdateStableBufferFromMask()
    {
        if (maskImage == null || maskImage.texture == null) return;
        if (stableBuffer == null || stableBuffer.width != maskImage.texture.width || stableBuffer.height != maskImage.texture.height)
        {
            InitStableBuffer();
            if (stableBuffer == null) return;
        }
        Graphics.Blit(maskImage.texture, stableBuffer);
        if (readTexture == null)
        {
            readTexture = new Texture2D(stableBuffer.width, stableBuffer.height, TextureFormat.RGBA32, false);
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

        if (isDrawing)
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
        if ((stableBuffer == null || readTexture == null) && (maskImage == null || maskImage.texture == null))
            return 0f;

        RenderTexture source = stableBuffer != null ? stableBuffer : null;
        if (source == null && maskImage != null && maskImage.texture != null)
        {
            int w = maskImage.texture.width;
            int h = maskImage.texture.height;
            source = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(maskImage.texture, source);
        }

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = source;

        if (readTexture == null || readTexture.width != source.width || readTexture.height != source.height)
        {
            if (readTexture != null) UnityEngine.Object.DestroyImmediate(readTexture);
            readTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        }

        readTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readTexture.Apply(false);

        RenderTexture.active = prev;

        Color32[] pixels = readTexture.GetPixels32();

        if (source != stableBuffer)
            RenderTexture.ReleaseTemporary(source);

        int w2 = readTexture.width;
        int h2 = readTexture.height;
        int sampleTarget = Mathf.Clamp(w2 / 4, 16, 256);
        int step = Mathf.Max(1, w2 / sampleTarget);
        int transparent = 0;
        int total = 0;

        for (int y = 0; y < h2; y += step)
        {
            int baseIdx = y * w2;
            for (int x = 0; x < w2; x += step)
            {
                total++;
                if (pixels[baseIdx + x].a < 51) transparent++;
            }
        }

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
        InitStableBuffer();
        selectedCrayonIndex = -1;
        transitioning = false;
    }

    private void OnYellowComplete()
    {
        SoundManager.Instance.PlayComplex("Suspect2", transform);
    }

    private void OnRedComplete()
    {
        SoundManager.Instance.PlayComplex("Suspect3", transform);
    }

    private void OnBlueComplete()
    {
        SoundManager.Instance.PlayComplex("Suspect4", transform);
    }

    private void OnBlackComplete()
    {
        StartCoroutine(FinishedDrawing());
    }

    private void OnAllDrawingsComplete()
    {
    }

    private IEnumerator FinishedDrawing()
    {
        SoundManager.Instance.PlayComplex("Suspect5", transform);
        yield return new WaitForSeconds(8f);
        SoundManager.Instance.PlayComplex("SuspectSketchSolve", transform);
        StarChartManager.Instance.UnlockStar("CR2");
        

        yield return new WaitForSeconds(3f);
        NiceDayTimeline.SetActive(true);
        EddyKitchentimeline.SetActive(true);
        SoundManager.Instance.PlayComplex("TimelineUnlock", transform);

        this.enabled = false;
    }

    private void OnDestroy()
    {
        ReleaseStableBuffer();
    }
}
