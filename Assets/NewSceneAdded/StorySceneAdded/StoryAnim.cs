using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryAnim : MonoBehaviour
{
    public Image[] AnimImages = new Image[4];
    public Image[] Dialogs = new Image[4];
    public Image back;
    public GameObject tip;
    [Header("按钮设置")]
    public Button restartButton;
    public Button skipButton;
    public Button returnMenuButton;

    [Header("音效设置")]
    public AudioClip sfx1;
    public AudioClip sfx2;
    public AudioSource audioSource;

    [Header("特效设置")]
    public Image[] effectFrames = new Image[8];
    public float effectFrameDuration = 0.1f;

    [Header("动画设置")]
    public float interval = 2.5f;

    private bool isPlaying = false;
    private Coroutine animationCoroutine;
    private int currentDialogIndex = 0;
    private bool waitingForClick = false;
    private Coroutine currentSFX1Coroutine;

    public static StoryAnim Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        HideAllImages();
        HideAllDialogs();

        if (audioSource == null)
            audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        HideAllEffectFrames();
    }

    void Start()
    {
        if (back != null)
            back.gameObject.SetActive(false);

        SetupButtons();
        AutoPlayStoryAnimation();
    }

    void Update()
    {
        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            OnDialogClick();
        }

        if (isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            SkipAnimation();
        }
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartScene);
            restartButton.gameObject.SetActive(true);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipAnimation);
            skipButton.gameObject.SetActive(true);
        }

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.RemoveAllListeners();
            returnMenuButton.onClick.AddListener(ReturnToMenu);
            returnMenuButton.gameObject.SetActive(true);
        }
    }

    public void AutoPlayStoryAnimation()
    {
        if (isPlaying) return;

        currentDialogIndex = 0;
        waitingForClick = false;

        animationCoroutine = StartCoroutine(PlayAnimationCoroutine());
    }

    public void PlayStoryAnimation()
    {
        if (isPlaying) return;

        currentDialogIndex = 0;
        waitingForClick = false;

        animationCoroutine = StartCoroutine(PlayAnimationCoroutine());
    }

    public void StopStoryAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (currentSFX1Coroutine != null)
        {
            StopCoroutine(currentSFX1Coroutine);
            currentSFX1Coroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        HideAllEffectFrames();
        EndAnimation();
    }

    public void SkipAnimation()
    {
        StopStoryAnimation();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private IEnumerator PlayAnimationCoroutine()
    {
        isPlaying = true;
        FreezePlayer(true);

        if (back != null)
            back.gameObject.SetActive(true);

        HideAllImages();
        HideAllDialogs();

        if (AnimImages.Length > 0 && AnimImages[0] != null)
            AnimImages[0].gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(0.5f);

        yield return StartCoroutine(PlayDialogSequence());

        for (int i = 1; i < AnimImages.Length; i++)
        {
            HideAllImages();

            if (AnimImages[i] != null)
            {
                AnimImages[i].gameObject.SetActive(true);
                StartCoroutine(FadeInImage(AnimImages[i]));
            }

            yield return new WaitForSecondsRealtime(interval);
        }

        HideAllImages();

        if (effectFrames.Length > 0)
        {
            yield return StartCoroutine(PlayEffectAnimation());
        }

        HideAllDialogs();
        if (Dialogs.Length > 3 && Dialogs[3] != null)
        {
            Dialogs[3].gameObject.SetActive(true);
            StartCoroutine(FadeInImage(Dialogs[3]));
            yield return new WaitForSecondsRealtime(2f);
        }

        EndAnimation();
    }

    private IEnumerator PlayEffectAnimation()
    {
        if (effectFrames.Length == 0) yield break;

        // 播放 sfx2 音效
        PlaySFX2();

        HideAllEffectFrames();

        for (int i = 0; i < effectFrames.Length; i++)
        {
            HideAllEffectFrames();

            if (effectFrames[i] != null)
            {
                effectFrames[i].gameObject.SetActive(true);
                StartCoroutine(FadeInImage(effectFrames[i], 0.2f));
            }

            yield return new WaitForSecondsRealtime(effectFrameDuration);
        }

        HideAllEffectFrames();
    }

    private void PlaySFX2()
    {
        if (audioSource == null || sfx2 == null) return;
        audioSource.PlayOneShot(sfx2);
    }

    private IEnumerator PlayDialogSequence()
    {
        if (Dialogs.Length > 0 && Dialogs[0] != null)
        {
            Dialogs[0].gameObject.SetActive(true);
            StartCoroutine(FadeInImage(Dialogs[0]));
            PlayDialogSound(0);
        }

        waitingForClick = true;
        currentDialogIndex = 0;

        var waitForClick = new WaitUntil(() => !waitingForClick);
        yield return waitForClick;

        StopSFX1IfPlaying();

        if (Dialogs.Length > 1 && Dialogs[1] != null)
        {
            HideAllDialogs();
            Dialogs[1].gameObject.SetActive(true);
            StartCoroutine(FadeInImage(Dialogs[1]));
            PlayDialogSound(1);

            waitingForClick = true;
            currentDialogIndex = 1;
            yield return waitForClick;
            StopSFX1IfPlaying();
        }

        if (Dialogs.Length > 2 && Dialogs[2] != null)
        {
            HideAllDialogs();
            Dialogs[2].gameObject.SetActive(true);
            StartCoroutine(FadeInImage(Dialogs[2]));
            PlayDialogSound(2);

            waitingForClick = true;
            currentDialogIndex = 2;
            yield return waitForClick;
            StopSFX1IfPlaying();
        }

        HideAllDialogs();
        StopSFX1IfPlaying();
    }

    private void PlayDialogSound(int dialogIndex)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = null;
        if (dialogIndex < 3)
        {
            clipToPlay = sfx1;

            if (clipToPlay != null && clipToPlay.length > 5f)
            {
                if (currentSFX1Coroutine != null)
                {
                    StopCoroutine(currentSFX1Coroutine);
                }
                currentSFX1Coroutine = StartCoroutine(TrackSFX1Playing(clipToPlay.length));
            }
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    private IEnumerator TrackSFX1Playing(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        currentSFX1Coroutine = null;
    }

    private void StopSFX1IfPlaying()
    {
        if (currentSFX1Coroutine != null)
        {
            StopCoroutine(currentSFX1Coroutine);
            currentSFX1Coroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDialogClick()
    {
        if (waitingForClick)
        {
            waitingForClick = false;
            currentDialogIndex++;
        }
    }

    private void EndAnimation()
    {
        HideAllImages();
        HideAllDialogs();
        if (back != null)
            back.gameObject.SetActive(false);

        HideAllEffectFrames();
        StopSFX1IfPlaying();
        FreezePlayer(false);
        tip.SetActive(true);
        isPlaying = false;
        waitingForClick = false;
    }

    private void HideAllImages()
    {
        foreach (Image img in AnimImages)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }
    }

    private void HideAllDialogs()
    {
        foreach (Image dialog in Dialogs)
        {
            if (dialog != null)
                dialog.gameObject.SetActive(false);
        }
    }

    private void HideAllEffectFrames()
    {
        foreach (Image frame in effectFrames)
        {
            if (frame != null)
                frame.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeInImage(Image image, float fadeTime = 0.5f)
    {
        if (image == null) yield break;

        Color originalColor = image.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;

        image.color = transparentColor;

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeTime;
            image.color = Color.Lerp(transparentColor, originalColor, progress);
            yield return null;
        }

        image.color = originalColor;
    }

    private void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in playerComponents)
            {
                if (component is UnityEngine.UI.Selectable ||
                    component is UnityEngine.UI.Graphic)
                    continue;

                component.enabled = !freeze;
            }
        }
    }

    public bool IsAnimationPlaying()
    {
        return isPlaying;
    }

    void OnDestroy()
    {
        if (isPlaying)
        {
            FreezePlayer(false);
            Time.timeScale = 1f;
        }

        if (currentSFX1Coroutine != null)
        {
            StopCoroutine(currentSFX1Coroutine);
        }
    }
}