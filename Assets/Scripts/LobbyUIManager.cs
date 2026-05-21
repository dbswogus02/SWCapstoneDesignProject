using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUIManager : MonoBehaviour
{
    [Header("팝업 UI 패널들")]
    [SerializeField] private GameObject howToPlayPanel; 
    [SerializeField] private GameObject quitConfirmPanel; 

    [Header("이동할 다음 씬 이름")]
    [SerializeField] private string nextQuestSceneName = "GameScene"; 

    void Start()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }

    public void OnClickGameStart()
    {
        SceneManager.LoadScene(nextQuestSceneName);
    }

    public void OnClickHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    public void OnClickCloseHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    public void OnClickQuitGame()
    {
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(true);
    }

    public void OnClickQuitConfirm()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnClickQuitCancel()
    {
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }
}