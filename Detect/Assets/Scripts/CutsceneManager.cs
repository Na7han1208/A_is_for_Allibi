using System.Collections;
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

    private void Start()
    {
        if (videoPlayer == null) videoPlayer = gameObject.AddComponent<VideoPlayer>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        FindFirstObjectByType<FPController>().isInspecting = true;
    }

    private void OnVideoPrepared(VideoPlayer videoPlayer)
    {
        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        //audioSource.Play();
    }

    private void OnVideoFinished(VideoPlayer videoPlayer)
    {
        rawImage.enabled = false;
        FindFirstObjectByType<FPController>().isInspecting = false;

        TutorialHelper tutorialHelper = FindFirstObjectByType<TutorialHelper>();
        tutorialHelper.ToggleInteraction(tutorialHelper.pickedUp ? false : true);
        FindFirstObjectByType<TutorialHelper>().DisplayMovement();
        skipImage.SetActive(false);
    }

    public void OnCutsceneSkip(InputAction.CallbackContext context)
    {
        Debug.Log("SKIPPED");

        videoPlayer.Stop();
        audioSource.Stop();

        OnVideoFinished(videoPlayer);
    }
}