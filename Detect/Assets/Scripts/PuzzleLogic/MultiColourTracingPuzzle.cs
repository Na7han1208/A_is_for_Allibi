using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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

    private int currentDrawingIndex = 0;
    private int selectedCrayonIndex = -1;
    private bool transitioning = false;

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

        bool completed = base.CheckCompletion();
        if (completed)
        {
            StopAllCoroutines();
            StartCoroutine(HandleDrawingComplete());
            finishedCalled = false;
        }

        return completed;
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