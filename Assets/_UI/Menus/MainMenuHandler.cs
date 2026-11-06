using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] UIDocument menuDoc;
    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] float fadeDuration = 1f;

    VisualElement root;
    VisualElement parent;
    VisualElement fadeOverlay;
    Button quitButton;
    Button startButton;
    bool isLoading;

    void Start()
    {
        root = menuDoc.rootVisualElement;
        parent = root.Q<VisualElement>("parent");
        //parent.style.display = DisplayStyle.None;

        quitButton = root.Q<Button>("quit-btn");
        startButton = root.Q<Button>("start-btn");

        // Fullscreen black overlay used for the fade
        fadeOverlay = new VisualElement { name = "fade-overlay", pickingMode = PickingMode.Ignore };
        fadeOverlay.style.position = Position.Absolute;
        fadeOverlay.style.left = 0;
        fadeOverlay.style.top = 0;
        fadeOverlay.style.right = 0;
        fadeOverlay.style.bottom = 0;
        fadeOverlay.style.backgroundColor = Color.black;
        fadeOverlay.style.opacity = 0;
        root.Add(fadeOverlay); // added last so it draws on top

        quitButton.RegisterCallback<MouseOverEvent>(PlayHoverSound);
        startButton.RegisterCallback<MouseOverEvent>(PlayHoverSound);

        quitButton.RegisterCallback<ClickEvent>(OnQuit);
        startButton.RegisterCallback<ClickEvent>(OnStart);
    }

    public void PlayHoverSound(MouseOverEvent e)
    {
        SoundFXManager.instance.PlaySoundFXClip(PlayerSounds.Instance.menu_hover, transform, 1, SoundType.SOUNDFX);
    }

    public void PlayClickSound()
    {
        SoundFXManager.instance.PlaySoundFXClip(PlayerSounds.Instance.menu_click, transform, .8f, SoundType.SOUNDFX);
    }

    public void OnQuit(ClickEvent evt)
    {
        if (isLoading) return;
        PlayClickSound();
        //#if UNITY_EDITOR
           //     UnityEditor.EditorApplication.isPlaying = false;
       // #else
                Application.Quit();
        //#endif
    }

    public void OnStart(ClickEvent evt)
    {
        if (isLoading) return; // prevent double clicks
        isLoading = true;
        PlayClickSound();
        StartCoroutine(FadeAndLoad(gameSceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // Block clicks on the menu while fading
        fadeOverlay.pickingMode = PickingMode.Position;

        // Start loading in the background, but don't switch scenes yet
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Fade to black (unscaled time in case timeScale is 0)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.style.opacity = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeOverlay.style.opacity = 1f;

        // Wait until the scene is fully loaded (progress stops at 0.9 until activation is allowed)
        while (op.progress < 0.9f)
            yield return null;

        Time.timeScale = 1;
        op.allowSceneActivation = true;
    }

    public void Initialize(Player player, InputHandler playerInputs, UI_InputHandler uiInputs)
    {
        // this.playerInputs = playerInputs;
        // this.uiInputs = uiInputs;
    }

    public void SetPauseVisible(bool show)
    {
        if (show)
        {
            startButton.style.display = DisplayStyle.Flex;
            parent.style.display = DisplayStyle.Flex;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            parent.style.display = DisplayStyle.None;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            Time.timeScale = 1;
        }
    }
}