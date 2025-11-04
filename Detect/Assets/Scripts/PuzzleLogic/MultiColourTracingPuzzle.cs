using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public class MultiColourTracingPuzzle : MonoBehaviour
{
    [Header("Multi Colour Puzzle Settings")]
    [SerializeField] private TracingPuzzle tracingPuzzle;
    [SerializeField] private RawImage[] maskImages;
    [SerializeField] private RawImage[] suspectDisplays;
    [SerializeField] private Color[] crayonColors;
    [SerializeField] private Image crayonCursor;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button blackButton;
    [SerializeField] private float transitionDelay = 3f;

    private int currentDrawingIndex = 0;
    private int selectedCrayonIndex = -1; // 0=yellow, 1=red, 2=blue, 3=black
    private bool transitioning = false;

    private void Start()
    {
        if (tracingPuzzle == null)
            tracingPuzzle = GetComponent<TracingPuzzle>();

        if (yellowButton != null) yellowButton.onClick.AddListener(() => SelectCrayon(0));
        if (redButton != null) redButton.onClick.AddListener(() => SelectCrayon(1));
        if (blueButton != null) blueButton.onClick.AddListener(() => SelectCrayon(2));
        if (blackButton != null) blackButton.onClick.AddListener(() => SelectCrayon(3));

        for (int i = 0; i < maskImages.Length; i++)
        {
            if (maskImages[i] != null) maskImages[i].gameObject.SetActive(i == 0);
            if (suspectDisplays != null && i < suspectDisplays.Length && suspectDisplays[i] != null)
                suspectDisplays[i].gameObject.SetActive(i == 0);
        }

        currentDrawingIndex = 0;
        selectedCrayonIndex = -1; 


        if (tracingPuzzle != null && maskImages.Length > 0)
        {
            tracingPuzzle.maskImage = maskImages[0];
        }
    }

    private void Update()
    {
        if (transitioning || tracingPuzzle == null) return;

        if (tracingPuzzle.CheckCompletion() && !transitioning)
        {
            StartCoroutine(HandleDrawingComplete());
        }
    }

    private void SelectCrayon(int index)
    {
        selectedCrayonIndex = index;

        if (crayonCursor != null && index >= 0 && index < crayonColors.Length)
            crayonCursor.color = crayonColors[index];
    }

    private bool CanDrawOnCurrent()
    {
        if (selectedCrayonIndex == -1) return false;
        return selectedCrayonIndex == currentDrawingIndex;
    }

    public void OnDraw(InputAction.CallbackContext ctx)
    {
        if (tracingPuzzle == null || transitioning) return;

        if (ctx.performed)
        {
            if (CanDrawOnCurrent())
                tracingPuzzle.isDrawing = true;
        }
        else if (ctx.canceled)
        {
            tracingPuzzle.isDrawing = false;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (tracingPuzzle != null)
            tracingPuzzle.OnMove(ctx);
    }

    public void OnExit(InputAction.CallbackContext ctx)
    {
        if (tracingPuzzle != null)
            tracingPuzzle.OnExit(ctx);
    }

    public void ShowPuzzle()
    {
        if (tracingPuzzle != null)
            tracingPuzzle.ShowPuzzle();
    }

    public void HidePuzzle()
    {
        if (tracingPuzzle != null)
            tracingPuzzle.HidePuzzle();
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
            if (maskImages[i] != null) maskImages[i].gameObject.SetActive(active);
            if (suspectDisplays != null && i < suspectDisplays.Length && suspectDisplays[i] != null)
                suspectDisplays[i].gameObject.SetActive(active);
        }

        tracingPuzzle.maskImage = maskImages[currentDrawingIndex];

        tracingPuzzle.ResetMask();

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
        HidePuzzle();
    }
}