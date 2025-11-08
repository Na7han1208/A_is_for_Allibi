using System.Collections;
using UnityEngine;

public class PhysicalButton : MonoBehaviour
{
    public int buttonIndex;
    public float hoverDarken = 0.8f;
    public float clickDarken = 0.5f;
    public float pressDepth = 0.015f;
    public float pressSpeed = 12f;

    private Renderer rend;
    private Color baseColor;
    private Coroutine pressRoutine;
    private Coroutine colorRoutine;
    private bool isHovered;
    private Vector3 startLocalPos;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;
        startLocalPos = transform.localPosition;
    }

    public void SetHover(bool on)
    {
        isHovered = on;
        if (rend == null) return;
        Color target = isHovered ? baseColor * hoverDarken : baseColor;
        if (colorRoutine != null) StopCoroutine(colorRoutine);
        colorRoutine = StartCoroutine(ColorLerp(target, 0.12f));
    }

    public void Press()
    {
        if (pressRoutine != null) StopCoroutine(pressRoutine);
        pressRoutine = StartCoroutine(PressAnimation());
        if (rend != null)
        {
            if (colorRoutine != null) StopCoroutine(colorRoutine);
            colorRoutine = StartCoroutine(ColorLerp(baseColor * clickDarken, 0.06f, true));
        }
    }

    IEnumerator PressAnimation()
    {
        if (rend == null)
            yield break;

        Transform t = rend.transform;
        Vector3 startPos = t.localPosition;
        Vector3 pressedPos = startPos + t.InverseTransformDirection(-t.forward) * pressDepth;

        float t1 = 0f;
        while (t1 < 1f)
        {
            t1 += Time.deltaTime * pressSpeed;
            t.localPosition = Vector3.Lerp(startPos, pressedPos, t1);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        t1 = 0f;
        while (t1 < 1f)
        {
            t1 += Time.deltaTime * pressSpeed;
            t.localPosition = Vector3.Lerp(pressedPos, startPos, t1);
            yield return null;
        }

        t.localPosition = startPos;
        pressRoutine = null;
    }

    IEnumerator ColorLerp(Color target, float duration, bool revertAfter = false)
    {
        if (rend == null) yield break;
        Color start = rend.material.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rend.material.color = Color.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        rend.material.color = target;
        colorRoutine = null;

        if (revertAfter)
        {
            yield return new WaitForSeconds(0.06f);
            Color next = isHovered ? baseColor * hoverDarken : baseColor;
            colorRoutine = StartCoroutine(ColorLerp(next, 0.12f));
        }
    }
}
