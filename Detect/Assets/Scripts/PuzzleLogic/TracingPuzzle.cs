using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor.Rendering;

public class TracingPuzzle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas puzzleCanvas;
    [SerializeField] private RawImage maskImage;
    [SerializeField] private RawImage teddyDisplay;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Vector2 cursorOffset = new Vector2(10f, -10f);
    [SerializeField] private GameObject eddyHat;

    [Header("Tracing Settings")]
    [SerializeField] private Sprite teddyBearSprite;
    [SerializeField] private Texture2D brushTexture;
    [SerializeField] private int fallbackTextureSize = 512;
    [SerializeField, Range(0f, 1f)] private float completionThreshold = 0.5f;
    [SerializeField] private float checkInterval = 0.5f;

    [Header("Brush Size")]
    [SerializeField] private bool useAbsoluteBrush = true;
    [SerializeField] private int brushPixelSize = 24;
    [SerializeField] private float brushScale = 0.3f;

    private Texture2D maskTex;
    private Color32[] maskPixels;
    private Color32[] originalMaskPixels;
    private Color32[] brushPixels;
    private int brushWorig;
    private int brushHorig;
    private Color32[] blockBuffer;
    private bool finishedCalled;
    private float checkTimer;
    private bool dirty;
    private bool ignoreUntilReleased;

    private void Awake()
    {
        if (puzzleCanvas != null) puzzleCanvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (teddyDisplay != null && teddyBearSprite != null) teddyDisplay.texture = teddyBearSprite.texture;

        cursorImage.raycastTarget = false;
        Cursor.visible = false;

        Texture src = (maskImage != null) ? maskImage.texture : null;
        int w = fallbackTextureSize;
        int h = fallbackTextureSize;

        if (src != null && src.width > 0 && src.height > 0)
        {
            w = src.width;
            h = src.height;
            RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            maskTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            maskTex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            maskTex.Apply(false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
        }
        else
        {
            maskTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color32[] fill = new Color32[w * h];
            for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(255, 255, 255, 255);
            maskTex.SetPixels32(fill);
            maskTex.Apply(false);
        }

        maskPixels = maskTex.GetPixels32();
        originalMaskPixels = (Color32[])maskPixels.Clone();
        if (maskImage != null) maskImage.texture = maskTex;

        if (brushTexture != null)
        {
            brushWorig = brushTexture.width;
            brushHorig = brushTexture.height;
            try { brushPixels = brushTexture.GetPixels32(); }
            catch { brushPixels = null; }
        }
        else
        {
            brushPixels = null;
            brushWorig = brushHorig = 0;
        }
    }

    private void Update()
    {
        if (puzzleCanvas == null || !puzzleCanvas.gameObject.activeSelf || finishedCalled) return;

        Vector2 pos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        cursorImage.rectTransform.position = pos + cursorOffset;

        bool isPressed = false;
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current != null)
        {
            foreach (var t in Touchscreen.current.touches)
            {
                if (t.press.isPressed)
                {
                    screenPos = t.position.ReadValue();
                    isPressed = true;
                    break;
                }
            }
        }

        if (!isPressed && Mouse.current != null && Mouse.current.leftButton != null)
        {
            isPressed = Mouse.current.leftButton.isPressed;
            if (isPressed) screenPos = Mouse.current.position.ReadValue();
        }

        if (ignoreUntilReleased)
        {
            if (!isPressed) ignoreUntilReleased = false;
            else return;
        }

        if (isPressed && maskImage != null)
        {
            Camera cam = null;
            if (puzzleCanvas != null && puzzleCanvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = puzzleCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(maskImage.rectTransform, screenPos, cam, out Vector2 localPoint))
            {
                Rect rect = maskImage.rectTransform.rect;
                Vector2 pivot = maskImage.rectTransform.pivot;
                float localFromLeftX = localPoint.x + rect.width * pivot.x;
                float localFromBottomY = localPoint.y + rect.height * pivot.y;
                float normX = localFromLeftX / rect.width;
                float normY = localFromBottomY / rect.height;

                if (normX >= 0f && normX <= 1f && normY >= 0f && normY <= 1f)
                {
                    Rect uv = maskImage.uvRect;
                    float u = uv.x + uv.width * normX;
                    float v = uv.y + uv.height * normY;
                    int px = Mathf.Clamp(Mathf.RoundToInt(u * (maskTex.width - 1)), 0, maskTex.width - 1);
                    int py = Mathf.Clamp(Mathf.RoundToInt(v * (maskTex.height - 1)), 0, maskTex.height - 1);
                    StampAt(px, py);
                }
            }
        }

        if (dirty)
        {
            maskTex.SetPixels32(maskPixels);
            maskTex.Apply(false);
            dirty = false;
        }

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckCompletion();
        }
    }

    private void StampAt(int cx, int cy)
    {
        if (maskPixels == null) return;

        int bw = useAbsoluteBrush ? Mathf.Max(1, brushPixelSize) : Mathf.Max(1, Mathf.RoundToInt((brushWorig > 0 ? brushWorig : brushPixelSize) * Mathf.Max(0.0001f, brushScale)));
        int bh = useAbsoluteBrush ? Mathf.Max(1, brushPixelSize) : Mathf.Max(1, Mathf.RoundToInt((brushHorig > 0 ? brushHorig : brushPixelSize) * Mathf.Max(0.0001f, brushScale)));
        bw = Mathf.Min(bw, maskTex.width);
        bh = Mathf.Min(bh, maskTex.height);

        int halfW = bw / 2;
        int halfH = bh / 2;
        int startX = Mathf.Clamp(cx - halfW, 0, maskTex.width - 1);
        int startY = Mathf.Clamp(cy - halfH, 0, maskTex.height - 1);
        int endX = Mathf.Clamp(cx + halfW, 0, maskTex.width - 1);
        int endY = Mathf.Clamp(cy + halfH, 0, maskTex.height - 1);

        int blockW = endX - startX + 1;
        int blockH = endY - startY + 1;
        if (blockW <= 0 || blockH <= 0) return;

        int need = blockW * blockH;
        if (blockBuffer == null || blockBuffer.Length < need) blockBuffer = new Color32[need];

        int i = 0;
        for (int y = 0; y < blockH; y++)
        {
            float v = (blockH == 1) ? 0.5f : (y / (float)(blockH - 1));
            int srcY = brushPixels != null ? Mathf.Clamp(Mathf.RoundToInt(v * (brushHorig - 1)), 0, brushHorig - 1) : -1;

            for (int x = 0; x < blockW; x++, i++)
            {
                float u = (blockW == 1) ? 0.5f : (x / (float)(blockW - 1));
                int srcX = brushPixels != null ? Mathf.Clamp(Mathf.RoundToInt(u * (brushWorig - 1)), 0, brushWorig - 1) : -1;

                float brushAlpha = 0f;
                if (brushPixels != null && brushWorig > 0 && brushHorig > 0)
                {
                    brushAlpha = brushPixels[srcY * brushWorig + srcX].a / 255f;
                }
                else
                {
                    float cxBrush = (x + 0.5f) - blockW * 0.5f;
                    float cyBrush = (y + 0.5f) - blockH * 0.5f;
                    float dist = Mathf.Sqrt(cxBrush * cxBrush + cyBrush * cyBrush);
                    float radius = Mathf.Max(blockW, blockH) * 0.5f;
                    float fall = Mathf.Clamp01(1f - (dist / radius));
                    brushAlpha = fall;
                }

                int tx = startX + x;
                int ty = startY + y;
                int idx = ty * maskTex.width + tx;
                Color32 cur = maskPixels[idx];
                byte newA = cur.a;
                if (brushAlpha > 0.05f) newA = 0;
                cur.a = newA;
                maskPixels[idx] = cur;
                blockBuffer[i] = cur;
            }
        }

        maskTex.SetPixels32(startX, startY, blockW, blockH, blockBuffer);
        dirty = true;
    }

    private void CheckCompletion()
    {
        if (maskPixels == null) return;
        int sampleTarget = Mathf.Clamp(maskTex.width / 4, 16, 256);
        int step = Mathf.Max(1, maskTex.width / sampleTarget);
        int transparent = 0;
        int total = 0;
        for (int y = 0; y < maskTex.height; y += step)
        {
            int baseIdx = y * maskTex.width;
            for (int x = 0; x < maskTex.width; x += step)
            {
                total++;
                if (maskPixels[baseIdx + x].a < 51) transparent++;
            }
        }
        float erasedFraction = total == 0 ? 0f : (float)transparent / total;
        if (erasedFraction >= completionThreshold)
        {
            finishedCalled = true;
            Finished();
        }
    }

    private void Finished()
    {
        var fpc = FindFirstObjectByType<FPController>();
        if (fpc != null) fpc.PlaySuccessParticles();
        SoundManager.Instance.PlayComplex("PaperTraceSolve", this.transform);
        StartCoroutine(PuzzleCompletionCoroutine());
    }

    private IEnumerator PuzzleCompletionCoroutine()
    {
        yield return new WaitForSeconds(3f);
        HidePuzzle();
        yield return new WaitForSeconds(5f);

        var fpc = FindFirstObjectByType<FPController>();
        Transform camT = null;
        if (fpc != null && fpc.cameraTransform != null) camT = fpc.cameraTransform;
        if (camT == null && Camera.main != null) camT = Camera.main.transform;
        if (camT == null || eddyHat == null) yield break;

        if (fpc != null) fpc.SetPuzzleActive(true);

        Vector3 dir = eddyHat.transform.position - camT.position;
        if (dir.sqrMagnitude <= 0.0001f)
        {
            if (fpc != null) fpc.SetPuzzleActive(false);
            yield break;
        }

        Quaternion startRot = camT.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        float speed = 90f; // degrees per second
        float angle = Quaternion.Angle(startRot, targetRot);
        float duration = angle / speed;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            camT.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camT.rotation = targetRot;

        yield return new WaitForSeconds(1f);

        if (fpc != null) fpc.SetPuzzleActive(false);
    }

    public void ShowPuzzle()
    {
        FindFirstObjectByType<TutorialHelper>().ToggleDrawTip(true);
        cursorImage.gameObject.SetActive(true);

        if (puzzleCanvas != null) puzzleCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        ignoreUntilReleased = IsPointerDown();
        var input = FindFirstObjectByType<PlayerInput>();
        if (input != null) input.SwitchCurrentActionMap("Puzzle");
    }

    private bool IsPointerDown()
    {
        if (Mouse.current != null && Mouse.current.leftButton != null && Mouse.current.leftButton.isPressed) return true;
        if (Touchscreen.current != null)
        {
            foreach (var t in Touchscreen.current.touches) if (t.press.isPressed) return true;
        }
        return false;
    }

    public void HidePuzzle()
    {
        FindFirstObjectByType<TutorialHelper>().ToggleDrawTip(false);
        cursorImage.gameObject.SetActive(false);

        if (puzzleCanvas != null) puzzleCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        ignoreUntilReleased = false;
        var input = FindFirstObjectByType<PlayerInput>();
        if (input != null) input.SwitchCurrentActionMap("Player");
    }

    public void ResetMask()
    {
        if (originalMaskPixels == null || maskPixels == null) return;
        System.Array.Copy(originalMaskPixels, maskPixels, originalMaskPixels.Length);
        maskTex.SetPixels32(maskPixels);
        maskTex.Apply(false);
        dirty = false;
        finishedCalled = false;
    }
}

/*
    I spent way too much time in this script, so I figured I may as well put some art down here to look at for sanity.
    Credit to: Normand Veilleux from https://www.asciiart.eu/
                                  _______
                           _,,ad8888888888bba,_
                        ,ad88888I888888888888888ba,
                      ,88888888I88888888888888888888a,
                    ,d888888888I8888888888888888888888b,
                   d88888PP"""" ""YY88888888888888888888b,
                 ,d88"'__,,--------,,,,.;ZZZY8888888888888,
                ,8IIl'"                ;;l"ZZZIII8888888888,
               ,I88l;'                  ;lZZZZZ888III8888888,
             ,II88Zl;.                  ;llZZZZZ888888I888888,
            ,II888Zl;.                .;;;;;lllZZZ888888I8888b
           ,II8888Z;;                 `;;;;;''llZZ8888888I8888,
           II88888Z;'                        .;lZZZ8888888I888b
           II88888Z; _,aaa,      .,aaaaa,__.l;llZZZ88888888I888
           II88888IZZZZZZZZZ,  .ZZZZZZZZZZZZZZ;llZZ88888888I888,
           II88888IZZ<'(@@>Z|  |ZZZ<'(@@>ZZZZ;;llZZ888888888I88I
          ,II88888;   `""" ;|  |ZZ; `"""     ;;llZ8888888888I888
          II888888l            `;;          .;llZZ8888888888I888,
         ,II888888Z;           ;;;        .;;llZZZ8888888888I888I
         III888888Zl;    ..,   `;;       ,;;lllZZZ88888888888I888
         II88888888Z;;...;(_    _)      ,;;;llZZZZ88888888888I888,
         II88888888Zl;;;;;' `--'Z;.   .,;;;;llZZZZ88888888888I888b
         ]I888888888Z;;;;'   ";llllll;..;;;lllZZZZ88888888888I8888,
         II888888888Zl.;;"Y88bd888P";;,..;lllZZZZZ88888888888I8888I
         II8888888888Zl;.; `"PPP";;;,..;lllZZZZZZZ88888888888I88888
         II888888888888Zl;;. `;;;l;;;;lllZZZZZZZZW88888888888I88888
         `II8888888888888Zl;.    ,;;lllZZZZZZZZWMZ88888888888I88888
          II8888888888888888ZbaalllZZZZZZZZZWWMZZZ8888888888I888888,
          `II88888888888888888b"WWZZZZZWWWMMZZZZZZI888888888I888888b
           `II88888888888888888;ZZMMMMMMZZZZZZZZllI888888888I8888888
            `II8888888888888888 `;lZZZZZZZZZZZlllll888888888I8888888,
             II8888888888888888, `;lllZZZZllllll;;.Y88888888I8888888b,
            ,II8888888888888888b   .;;lllllll;;;.;..88888888I88888888b,
            II888888888888888PZI;.  .`;;;.;;;..; ...88888888I8888888888,
            II888888888888PZ;;';;.   ;. .;.  .;. .. Y8888888I88888888888b,
           ,II888888888PZ;;'                        `8888888I8888888888888b,
           II888888888'                              888888I8888888888888888b
          ,II888888888                              ,888888I88888888888888888
         ,d88888888888                              d888888I8888888888ZZZZZZZ
      ,ad888888888888I                              8888888I8888ZZZZZZZZZZZZZ
    ,d888888888888888'                              888888IZZZZZZZZZZZZZZZZZZ
  ,d888888888888P'8P'                               Y888ZZZZZZZZZZZZZZZZZZZZZ
 ,8888888888888,  "                                 ,ZZZZZZZZZZZZZZZZZZZZZZZZ
d888888888888888,                                ,ZZZZZZZZZZZZZZZZZZZZZZZZZZZ
888888888888888888a,      _                    ,ZZZZZZZZZZZZZZZZZZZZ888888888
888888888888888888888ba,_d'                  ,ZZZZZZZZZZZZZZZZZ88888888888888
8888888888888888888888888888bbbaaa,,,______,ZZZZZZZZZZZZZZZ888888888888888888
88888888888888888888888888888888888888888ZZZZZZZZZZZZZZZ888888888888888888888
8888888888888888888888888888888888888888ZZZZZZZZZZZZZZ88888888888888888888888
888888888888888888888888888888888888888ZZZZZZZZZZZZZZ888888888888888888888888
8888888888888888888888888888888888888ZZZZZZZZZZZZZZ88888888888888888888888888
88888888888888888888888888888888888ZZZZZZZZZZZZZZ8888888888888888888888888888
8888888888888888888888888888888888ZZZZZZZZZZZZZZ88888888888888888 Normand  88
88888888888888888888888888888888ZZZZZZZZZZZZZZ8888888888888888888 Veilleux 88
8888888888888888888888888888888ZZZZZZZZZZZZZZ88888888888888888888888888888888
*/