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
        FindFirstObjectByType<TutorialHelper>().ToggleInteraction(true);
        FindFirstObjectByType<TutorialHelper>().DisplayMovement();
    }

    public void OnCutsceneSkip(InputAction.CallbackContext context)
    {
        Debug.Log("SKIPPED");

        videoPlayer.Stop();
        audioSource.Stop();

        OnVideoFinished(videoPlayer);
    }
}