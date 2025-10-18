using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class CutsceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject skipImage;

    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool loopCutscene = false;

    [Header("Subtitles")]
    [SerializeField] private SubtitleSequence subtitleSequence;

    private bool videoFinished = false;
    private FPController fpController;
    private TutorialHelper tutorialHelper;

    private void Start()
    {
        InitializeReferences();
        SetupVideoPlayer();

        if (subtitleSequence != null)
        {
            SubtitleManager.Instance.PlaySequence(subtitleSequence);
        }

        fpController.isInspecting = true;
        videoPlayer.Prepare();
    }

    private void InitializeReferences()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (rawImage == null) rawImage = FindFirstObjectByType<RawImage>();
        if (skipImage != null) skipImage.SetActive(true);

        fpController = FindFirstObjectByType<FPController>();
        tutorialHelper = FindFirstObjectByType<TutorialHelper>();
    }

    private void SetupVideoPlayer()
    {
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.isLooping = loopCutscene;

        // Clean event bindings before re-adding
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.time = 0;
        rawImage.texture = vp.texture;
        rawImage.enabled = true;
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (loopCutscene)
        {
            vp.time = 0;
            vp.Play();
            return;
        }

        if (videoFinished) return;
        videoFinished = true;

        EndCutscene();
    }

    public void OnCutsceneSkip(InputAction.CallbackContext context)
    {
        if (!context.performed || videoFinished) return;

        Debug.Log("Cutscene skipped by player.");

        if (subtitleSequence != null)
            SubtitleManager.Instance.StopSubtitles();

        CleanupVideo();
        videoFinished = true;
        EndCutscene();
    }

    private void EndCutscene()
    {
        rawImage.enabled = false;
        skipImage?.SetActive(false);

        if (fpController != null)
            fpController.isInspecting = false;

        if (tutorialHelper != null)
        {
            tutorialHelper.ToggleInteraction(!tutorialHelper.pickedUp);
            tutorialHelper.DisplayMovement();
        }

        SoundManager.Instance.PlayComplex("NaproomMusic", transform);
        CleanupVideo();
    }

    private void CleanupVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
        }

        if (audioSource != null)
            audioSource.Stop();

        if (rawImage != null)
            rawImage.texture = null;

        videoFinished = true;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
