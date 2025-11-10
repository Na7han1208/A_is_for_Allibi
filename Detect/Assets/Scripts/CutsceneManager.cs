using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

[Serializable]
public class CutsceneData
{
    public string cutsceneName;
    public VideoClip videoClip;
    public SubtitleSequence subtitleSequence;
    public bool loop;
    public bool skippable;

    [Header("Events")]
    public UnityEvent onBeforeCutscene;
    public UnityEvent onAfterCutscene;
}

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private GameObject skipImage;
    [SerializeField] private GameObject crosshair;

    [Header("Audio/Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    [Header("Library")]
    [SerializeField] private CutsceneData[] cutscenes;

    [Header("Options")]
    [SerializeField] private bool dontDestroy = true;
    [SerializeField] private float prepareTimeout = 5f;

    public bool IsInCutscene { get; private set; }

    private CutsceneData current;
    private bool playing;
    private bool prepared;
    private bool allowSkip;
    private bool pendingPrepare;
    private float prepareTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroy) DontDestroyOnLoad(gameObject);

        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>() ?? gameObject.AddComponent<VideoPlayer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (rawImage == null) rawImage = FindFirstObjectByType<RawImage>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
        audioSource.mute = false;
        audioSource.volume = 1f;

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnEnded;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnEnded;

        if (rawImage != null) rawImage.gameObject.SetActive(false);
        if (skipImage != null) skipImage.SetActive(false);

        IsInCutscene = false;
    }

    void Update()
    {
        if (!playing) return;

        bool skipPressed = false;
        if (Keyboard.current != null) skipPressed |= Keyboard.current.spaceKey.wasPressedThisFrame;
        skipPressed |= Input.GetKeyDown(KeyCode.Space);

        if (skipPressed && allowSkip) Skip();

        if (pendingPrepare)
        {
            prepareTimer += Time.unscaledDeltaTime;
            if (prepareTimer >= prepareTimeout)
            {
                pendingPrepare = false;
                prepared = videoPlayer.isPrepared;
                StartPlaybackAfterPrepare();
            }
        }

        if (prepared && rawImage != null && videoPlayer.texture != null)
            rawImage.texture = videoPlayer.texture;
    }

    public void PlayCutscene(string name)
    {
        var data = Array.Find(cutscenes, c => c != null && c.cutsceneName == name);
        if (data == null) return;
        StartCutscene(data);
    }

    public void PlayCutscene(int index)
    {
        if (index < 0 || index >= cutscenes.Length) return;
        StartCutscene(cutscenes[index]);
    }

    private void StartCutscene(CutsceneData data)
    {
        StopCurrentQuiet();

        crosshair.SetActive(false);

        current = data;
        playing = true;
        IsInCutscene = true;
        prepared = false;
        pendingPrepare = false;
        prepareTimer = 0f;
        allowSkip = data.skippable;
        if (skipImage != null) skipImage.SetActive(allowSkip);

        if (rawImage == null) rawImage = FindFirstObjectByType<RawImage>();
        if (rawImage != null) rawImage.gameObject.SetActive(true);

        videoPlayer.Stop();
        audioSource.Stop();

        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.clip = data.videoClip;
        videoPlayer.isLooping = data.loop;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        if (videoPlayer.audioTrackCount > 0)
        {
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
                videoPlayer.EnableAudioTrack(i, true);
            videoPlayer.SetDirectAudioMute(0, false);
            videoPlayer.SetDirectAudioVolume(0, 1f);
        }
        audioSource.spatialBlend = 0f;
        audioSource.mute = false;
        audioSource.volume = 1f;

        if (data.subtitleSequence != null && SubtitleManager.Instance != null)
            SubtitleManager.Instance.PlaySequence(data.subtitleSequence);

        data.onBeforeCutscene?.Invoke();

        pendingPrepare = true;
        prepareTimer = 0f;
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        if (vp != videoPlayer) return;
        prepared = true;
        pendingPrepare = false;
        if (rawImage != null && vp.texture != null)
            rawImage.texture = vp.texture;
        StartPlaybackAfterPrepare();
    }

    private void StartPlaybackAfterPrepare()
    {
        if (!prepared && videoPlayer.isPrepared) prepared = true;
        if (videoPlayer.clip == null) return;
        videoPlayer.Play();
        playing = true;
    }

    private void OnEnded(VideoPlayer vp)
    {
        if (current != null && current.loop) return;
        EndCutscene();
    }

    public void Skip()
    {
        if (!playing || !allowSkip) return;
        EndCutscene();
    }

    public void Stop()
    {
        if (!playing) return;
        EndCutscene();
    }

    private void EndCutscene()
    {
        if (!playing) return;

        var finishedCutscene = current;

        playing = false;
        IsInCutscene = false;
        prepared = false;
        pendingPrepare = false;
        prepareTimer = 0f;

        if (videoPlayer.isPlaying)
            videoPlayer.Stop();
        if (audioSource.isPlaying)
            audioSource.Stop();

        if (rawImage != null) rawImage.gameObject.SetActive(false);
        if (skipImage != null) skipImage.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true);

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSubtitles();

        current = null;

        StartCoroutine(InvokeAfterEndOfFrame(finishedCutscene));
    }

    private IEnumerator InvokeAfterEndOfFrame(CutsceneData data)
    {
        yield return null; // wait one frame
        data?.onAfterCutscene?.Invoke();
    }


    private void StopCurrentQuiet()
    {
        if (playing)
        {
            videoPlayer.Stop();
            audioSource.Stop();
            if (rawImage != null) rawImage.gameObject.SetActive(false);
            if (skipImage != null) skipImage.SetActive(false);
            playing = false;
            IsInCutscene = false;
            prepared = false;
            pendingPrepare = false;
            prepareTimer = 0f;
            if (current != null && current.subtitleSequence != null && SubtitleManager.Instance != null)
                SubtitleManager.Instance.StopSubtitles();
            current = null;
        }
    }

    void OnDisable()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnEnded;
    }

    void OnEnable()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnEnded;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnEnded;
    }
}
