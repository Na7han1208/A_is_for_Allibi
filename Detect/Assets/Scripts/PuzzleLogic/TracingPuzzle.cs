using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Mono.Cecil.Cil;
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
    [SerializeField] private int renderTextureSize = 1024;
    [SerializeField] [Range(0f,1f)] private float completionThreshold = 0.5f;

    private RenderTexture renderTex;
    private bool finishedCalled = false;

    private void Awake()
    {
        puzzleCanvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (teddyDisplay != null && teddyBearSprite != null)
            teddyDisplay.texture = teddyBearSprite.texture;

        renderTex = new RenderTexture(renderTextureSize, renderTextureSize, 0, RenderTextureFormat.ARGB32);
        renderTex.Create();

        // Fully opaque mask
        Graphics.SetRenderTarget(renderTex);
        GL.Clear(true, true, Color.white);
        Graphics.SetRenderTarget(null);

        if (maskImage != null)
            maskImage.texture = renderTex;
    }

    private void Update()
    {
        if (!puzzleCanvas.gameObject.activeSelf || finishedCalled) return;

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                maskImage.rectTransform,
                mousePos,
                null,
                out Vector2 localPoint
            );

            Rect rect = maskImage.rectTransform.rect;
            float normX = (localPoint.x - rect.xMin) / rect.width;
            float normY = (localPoint.y - rect.yMin) / rect.height;

            int px = Mathf.RoundToInt(normX * renderTex.width);
            int py = Mathf.RoundToInt(normY * renderTex.height);

            DrawBrush(px, py);
            CheckCompletion();
        }
    }

    private void DrawBrush(int x, int y)
    {
        if (brushTexture == null || renderTex == null) return;

        RenderTexture.active = renderTex;
        Texture2D tempTex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
        tempTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tempTex.Apply();

        int startX = Mathf.Clamp(x - brushTexture.width / 2, 0, renderTex.width);
        int startY = Mathf.Clamp(y - brushTexture.height / 2, 0, renderTex.height);

        for (int i = 0; i < brushTexture.width; i++)
        {
            for (int j = 0; j < brushTexture.height; j++)
            {
                int px = startX + i;
                int py = startY + j;

                if (px >= renderTex.width || py >= renderTex.height) continue;

                Color brushPixel = brushTexture.GetPixel(i, j);
                Color currentPixel = tempTex.GetPixel(px, py);

                currentPixel.a *= 1f - brushPixel.a;
                tempTex.SetPixel(px, py, currentPixel);
            }
        }

        tempTex.Apply();
        Graphics.Blit(tempTex, renderTex);
        RenderTexture.active = null;
        Destroy(tempTex);
    }

    private void CheckCompletion()
    {
        Texture2D tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTex;
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        int transparentCount = 0;
        int totalPixels = tex.width * tex.height;

        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.2f) // almost fully erased
                transparentCount++;
        }

        Destroy(tex);

        float erasedFraction = (float)transparentCount / totalPixels;
        if (erasedFraction >= completionThreshold)
        {
            finishedCalled = true;
            Finished();
        }
    }

    private void Finished()
    {
        Debug.Log("Puzzle finished!");
        var FPC = FindFirstObjectByType<FPController>();
        if (FPC != null)
        {
            FPC.PlaySuccessParticles();
        }
        StartCoroutine(WaitSecondsThenHide(3f));
    }

    private IEnumerator WaitSecondsThenHide(float seconds)
    {
        yield return new WaitForSeconds(3f);
        HidePuzzle();
    }

    public void ShowPuzzle()
    {
        puzzleCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Puzzle");
    }

    public void HidePuzzle()
    {
        puzzleCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");
    }
}
