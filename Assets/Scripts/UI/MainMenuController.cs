using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject manualPanel; // 조작법 패널 연결
    public GameObject exitPanel;   // 종료 확인 패널 연결

    // 조작법 버튼을 눌렀을 때
    public void OpenManual()
    {
        manualPanel.SetActive(true);
    }

    // 종료 버튼을 눌렀을 때
    public void OpenExitPopup()
    {
        exitPanel.SetActive(true);
    }

    // 팝업을 닫을 때 (취소 또는 닫기 버튼)
    public void CloseAllPopups()
    {
        manualPanel.SetActive(false);
        exitPanel.SetActive(false);
    }

    // 실제로 게임을 종료할 때 (종료 팝업의 '예' 버튼)
    public void ConfirmQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}