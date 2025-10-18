using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI")]
    public RawImage rawImage;
    public AudioSource audioSource;
    public GameObject skipImage;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public VideoClip videoClip;
    private bool videoFinished = false;

    [Header("Subtitles")]
    public SubtitleSequence introSequence;

    private void Start()
    {
        if (videoPlayer == null) videoPlayer = gameObject.AddComponent<VideoPlayer>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        SubtitleManager.Instance.PlaySequence(introSequence);

        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // Avoid duplicate bindings
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Prepare();

        FindFirstObjectByType<FPController>().isInspecting = true;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.time = 0; // always start at beginning
        rawImage.texture = vp.texture;
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoFinished) return;
        rawImage.enabled = false;
        FindFirstObjectByType<FPController>().isInspecting = false;

        TutorialHelper tutorialHelper = FindFirstObjectByType<TutorialHelper>();
        tutorialHelper.ToggleInteraction(!tutorialHelper.pickedUp);
        tutorialHelper.DisplayMovement();

        skipImage.SetActive(false);
        SoundManager.Instance.PlayComplex("NaproomMusic", transform);

        CleanupVideo();
        videoFinished = true;
    }

    public void OnCutsceneSkip(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Debug.Log("SKIPPED");

        videoPlayer.time = 0;
        CleanupVideo();
        OnVideoFinished(videoPlayer);
    }

    private void CleanupVideo()
    {
        videoPlayer.Stop();
        videoPlayer.clip = null;
        audioSource.Stop();
        rawImage.texture = null;
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
