using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class TracingPuzzle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas puzzleCanvas;
    [SerializeField] private RawImage maskImage;
    [SerializeField] private RawImage teddyDisplay;

    [Header("Tracing Settings")]
    [SerializeField] private Sprite teddyBearSprite;
    [SerializeField] private Texture2D brushTexture;
    [SerializeField] private int textureSize = 512;
    [SerializeField] [Range(0f, 1f)] private float completionThreshold = 0.5f;
    [SerializeField] private float checkInterval = 0.5f;

    [Header("Brush Size (choose one)")]
    [SerializeField] private bool useAbsoluteBrush = true;
    [SerializeField] private int brushPixelSize = 24;
    [SerializeField] private float brushScale = 0.1f;

    private Texture2D maskTex;
    private Color32[] maskPixels;
    private Color32[] brushPixels;
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

        maskTex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        maskTex.wrapMode = TextureWrapMode.Clamp;
        maskTex.filterMode = FilterMode.Point;

        maskPixels = new Color32[textureSize * textureSize];
        for (int i = 0; i < maskPixels.Length; i++) maskPixels[i] = new Color32(255, 255, 255, 255);

        maskTex.SetPixels32(maskPixels);
        maskTex.Apply(false);

        if (maskImage != null) maskImage.texture = maskTex;

        if (brushTexture != null)
        {
            brushPixels = brushTexture.GetPixels32();
            if (brushPixels == null || brushPixels.Length == 0) brushPixels = new Color32[brushTexture.width * brushTexture.height];
        }
    }

    private void Update()
    {
        if (puzzleCanvas == null || !puzzleCanvas.gameObject.activeSelf || finishedCalled) return;

        bool isPressed = false;
        Vector2 screenPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton != null)
        {
            isPressed = Mouse.current.leftButton.isPressed;
            if (isPressed) screenPos = Mouse.current.position.ReadValue();
        }

        if (ignoreUntilReleased)
        {
            if (!isPressed) ignoreUntilReleased = false;
            else return;
        }

        if (isPressed)
        {
            if (maskImage == null) return;

            Camera cam = null;
            if (puzzleCanvas != null && puzzleCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = puzzleCanvas.worldCamera;

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
                    int px = Mathf.Clamp(Mathf.RoundToInt(normX * (textureSize - 1)), 0, textureSize - 1);
                    int py = Mathf.Clamp(Mathf.RoundToInt(normY * (textureSize - 1)), 0, textureSize - 1);
                    StampAt(px, py);
                }
            }
        }

        if (dirty)
        {
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
        if (brushPixels == null || maskPixels == null) return;

        int bw, bh;
        if (useAbsoluteBrush)
        {
            bw = Mathf.Max(1, brushPixelSize);
            bh = Mathf.Max(1, brushPixelSize);
        }
        else
        {
            bw = Mathf.Max(1, Mathf.RoundToInt(brushTexture.width * Mathf.Max(0.0001f, brushScale)));
            bh = Mathf.Max(1, Mathf.RoundToInt(brushTexture.height * Mathf.Max(0.0001f, brushScale)));
        }

        bw = Mathf.Min(bw, textureSize);
        bh = Mathf.Min(bh, textureSize);

        int halfW = bw / 2;
        int halfH = bh / 2;

        int startX = Mathf.Clamp(cx - halfW, 0, textureSize - 1);
        int startY = Mathf.Clamp(cy - halfH, 0, textureSize - 1);
        int endX = Mathf.Clamp(cx + halfW, 0, textureSize - 1);
        int endY = Mathf.Clamp(cy + halfH, 0, textureSize - 1);

        int blockW = endX - startX + 1;
        int blockH = endY - startY + 1;
        if (blockW <= 0 || blockH <= 0) return;

        Color32[] block = new Color32[blockW * blockH];

        int i = 0;
        for (int y = 0; y < blockH; y++)
        {
            float v = (blockH == 1) ? 0.5f : (y / (float)(blockH - 1));
            int srcY = Mathf.Clamp(Mathf.RoundToInt(v * (brushTexture.height - 1)), 0, brushTexture.height - 1);

            for (int x = 0; x < blockW; x++, i++)
            {
                float u = (blockW == 1) ? 0.5f : (x / (float)(blockW - 1));
                int srcX = Mathf.Clamp(Mathf.RoundToInt(u * (brushTexture.width - 1)), 0, brushTexture.width - 1);

                Color32 b = brushPixels[srcY * brushTexture.width + srcX];
                if (b.a > 10)
                    block[i] = new Color32(255, 255, 255, 0);
                else
                    block[i] = maskPixels[(startY + y) * textureSize + (startX + x)];
            }
        }

        // write back into maskPixels and texture
        i = 0;
        for (int y = 0; y < blockH; y++)
        {
            int dst = (startY + y) * textureSize + startX;
            for (int x = 0; x < blockW; x++, i++)
                maskPixels[dst + x] = block[i];
        }

        maskTex.SetPixels32(startX, startY, blockW, blockH, block);
        dirty = true;
    }

    private void CheckCompletion()
    {
        int sample = Mathf.Clamp(textureSize / 4, 16, 256);
        int step = Mathf.Max(1, textureSize / sample);

        int transparent = 0;
        int total = 0;

        for (int y = 0; y < textureSize; y += step)
        {
            int baseIdx = y * textureSize;
            for (int x = 0; x < textureSize; x += step)
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
        Debug.Log("Puzzle finished!");
        var fpc = FindFirstObjectByType<FPController>();
        if (fpc != null) fpc.PlaySuccessParticles();
        SoundManager.Instance.PlayComplex("PaperTraceSolve", this.transform);
        StartCoroutine(WaitThenHide(3f));
    }

    private IEnumerator WaitThenHide(float s)
    {
        yield return new WaitForSeconds(s);
        HidePuzzle();
    }

    public void ShowPuzzle()
    {
        if (puzzleCanvas != null) puzzleCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        if (puzzleCanvas != null) puzzleCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ignoreUntilReleased = false;
        var input = FindFirstObjectByType<PlayerInput>();
        if (input != null) input.SwitchCurrentActionMap("Player");
    }

    public void ResetMask()
    {
        if (maskPixels == null || maskTex == null) return;
        for (int i = 0; i < maskPixels.Length; i++) maskPixels[i] = new Color32(255, 255, 255, 255);
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