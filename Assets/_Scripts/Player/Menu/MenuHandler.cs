using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] UIDocument menuDoc;
    [SerializeField] string quitSceneName = "MainMenu";
    [SerializeField] float fadeDuration = 1f;

    Player player;
    InputHandler playerInputs;
    UI_InputHandler uiInputs;
    VisualElement root;
    VisualElement parent;
    VisualElement fadeOverlay;
    Label pauselabel;
    Button restartBtn;
    Button quitButton;
    Button continueButton;
    Slider sensitivitySlider;
    bool isTransitioning;

    Button controlsButton;
    Button backButton;
    VisualElement controlsParent;
    VisualElement menuParent;
    [SerializeField]AudioSource bg;
    void Start()
    {
        root = menuDoc.rootVisualElement;
        parent = root.Q<VisualElement>("menu-parent");
        parent.style.display = DisplayStyle.None;

        restartBtn = root.Q<Button>("restart-btn");
        quitButton = root.Q<Button>("quit-btn");
        continueButton = root.Q<Button>("continue-btn");
        pauselabel = root.Q<Label>("pause-label");
        sensitivitySlider = root.Q<Slider>("sensitivity-slider");


        controlsButton = root.Q<Button>("control-btn");
        backButton = root.Q<Button>("back");
        controlsParent = root.Q<VisualElement>("control-parent");
        menuParent = root.Q<VisualElement>("parent-parent");

        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(CameraHandler.SensitivityPrefKey, 1f));
            sensitivitySlider.RegisterValueChangedCallback(OnSensitivityChanged);
        }

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

        restartBtn.RegisterCallback<MouseOverEvent>(PlayHoverSound);
        quitButton.RegisterCallback<MouseOverEvent>(PlayHoverSound);
        continueButton.RegisterCallback<MouseOverEvent>(PlayHoverSound);
        controlsButton.RegisterCallback<MouseOverEvent>(PlayHoverSound);
        backButton.RegisterCallback<MouseOverEvent>(PlayHoverSound);

        restartBtn.RegisterCallback<ClickEvent>(OnRestart);
        quitButton.RegisterCallback<ClickEvent>(OnQuit);
        continueButton.RegisterCallback<ClickEvent>(OnContinue);
        controlsButton.RegisterCallback<ClickEvent>(OnControls);
        backButton.RegisterCallback<ClickEvent>(OnBack);
    }

    public void PlayHoverSound(MouseOverEvent e)
    {
        SoundFXManager.instance.PlaySoundFXClip(PlayerSounds.Instance.menu_hover, transform, .8f, SoundType.SOUNDFX);
    }

    public void PlayClickSound()
    {
        SoundFXManager.instance.PlaySoundFXClip(PlayerSounds.Instance.menu_click, transform, .8f, SoundType.SOUNDFX);
    }
    void OnSensitivityChanged(ChangeEvent<float> evt)
    {
        PlayerPrefs.SetFloat(CameraHandler.SensitivityPrefKey, evt.newValue);
        if (player != null)
            player.cameraHandler.SetSensitivityMultiplier(evt.newValue);
    }
    public void OnRestart(ClickEvent evt)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        PlayClickSound();
        StartCoroutine(FadeAndLoad(SceneManager.GetActiveScene().name));
    }
    public void OnControls(ClickEvent evt)
    {
        PlayClickSound();
        menuParent.style.display = DisplayStyle.None;
        controlsParent.style.display = DisplayStyle.Flex;
    }
    public void OnBack(ClickEvent evt)
    {
        PlayClickSound();
        menuParent.style.display = DisplayStyle.Flex;
        controlsParent.style.display = DisplayStyle.None;
    }

    public void OnQuit(ClickEvent evt)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        PlayClickSound();
        StartCoroutine(FadeAndLoad(quitSceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        fadeOverlay.pickingMode = PickingMode.Position;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.style.opacity = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeOverlay.style.opacity = 1f;
        while (op.progress < 0.9f)
            yield return null;

        Time.timeScale = 1;
        op.allowSceneActivation = true;
    }

    public void OnContinue(ClickEvent evt)
    {
        PlayClickSound();
        parent.style.display = DisplayStyle.None;
        playerInputs.SetEnabled(true);
        uiInputs.SetEnabled(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        Time.timeScale = 1;
        bg.Play();
    }

    public void Initialize(Player player, InputHandler playerInputs, UI_InputHandler uiInputs)
    {
        this.player = player;
        this.playerInputs = playerInputs;
        this.uiInputs = uiInputs;
    }

    public void SetPauseVisible(bool show)
    {
        if (show)
        {
            pauselabel.text = "Game Paused";
            continueButton.style.display = DisplayStyle.Flex;
            parent.style.display = DisplayStyle.Flex;
            playerInputs.SetEnabled(false);
            uiInputs.SetEnabled(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Time.timeScale = 0;
            bg.Pause();
        }
        else
        {
            parent.style.display = DisplayStyle.None;
            playerInputs.SetEnabled(true);
            uiInputs.SetEnabled(false);
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            Time.timeScale = 1;
            bg.Play();
        }
    }

    public void SetGameOverVisible(bool show, string text)
    {
        if (show)
        {
            pauselabel.text = text;
            continueButton.style.display = DisplayStyle.None;
            parent.style.display = DisplayStyle.Flex;
            playerInputs.SetEnabled(false);
            uiInputs.SetEnabled(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            parent.style.display = DisplayStyle.None;
            playerInputs.SetEnabled(true);
            uiInputs.SetEnabled(false);
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            Time.timeScale = 1;
        }
    }
}