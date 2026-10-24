using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField]UIDocument document;
    Button restartBtn;
    Button quitButton;
    VisualElement parent;
    Label result;
    private void Start()
    {
        //document = GetComponent<UIDocument>();
        VisualElement root = document.rootVisualElement;
        restartBtn = root.Q<Button>("restart-btn");
        quitButton = root.Q<Button>("quit-btn");
        result = root.Q<Label>("result");
        parent = root.Q<VisualElement>("parent");

        restartBtn.RegisterCallback<ClickEvent>(OnRestart);

        quitButton.RegisterCallback<ClickEvent>(OnQuit);
        parent.style.visibility = Visibility.Hidden;
        //gameObject.SetActive(false);

    }
    public void Show(string resultText)
    {

        result.text = resultText;

        parent.style.visibility = Visibility.Visible;
        result.text = resultText;
    }
    public void OnRestart(ClickEvent evt)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OnQuit(ClickEvent evt)
    {
       UnityEditor.EditorApplication.isPlaying = false;
    }

}
