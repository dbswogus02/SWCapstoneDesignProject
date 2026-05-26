using UnityEngine;
using UnityEngine.SceneManagement; // 💡 완전히 새로운 미션 UI 화면(씬)으로 넘어가기 위해 필수!
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("팝업 UI 패널들")]
    [SerializeField] private GameObject howToPlayPanel; 
    [SerializeField] private GameObject quitConfirmPanel; 
    [SerializeField] private GameObject saveSlotPanel;       // 세이브 슬롯 패널 (D90 Panel)

    [Header("이동할 새로운 화면(씬) 이름")]
    [SerializeField] private string missionSceneName = "MissionUI"; // 💡 완전히 새로운 미션 UI 씬 이름

    [Header("FHD 대응을 위해 키워줄 UI 오브젝트들")]
    [SerializeField] private RectTransform titleTextRect;     
    [SerializeField] private RectTransform buttonGroupRect;   

    [Header("UI 확대 배율")]
    [SerializeField] private float scaleFactor = 3.5f;        

    void Start()
    {
        // 해상도 고정 (1920x1080 FHD)
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);

        // 💡 [오류 해결] 게임이 시작되면 화면에 켜져 있던 세이브 슬롯을 포함한 모든 팝업을 자동으로 숨깁니다.
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
        if (saveSlotPanel != null) saveSlotPanel.SetActive(false);

        ScaleUpUIForFHD();
    }

    void Update()
    {
        // ESC 키를 누르면 열려 있는 창을 안전하게 닫아주는 기능
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (howToPlayPanel != null && howToPlayPanel.activeSelf)
            {
                OnClickCloseHowToPlay();
            }
            else if (quitConfirmPanel != null && quitConfirmPanel.activeSelf)
            {
                OnClickQuitCancel();
            }
            else if (saveSlotPanel != null && saveSlotPanel.activeSelf)
            {
                // 세이브 슬롯 창이 켜져 있을 때 ESC를 누르면 로비로 돌아감
                saveSlotPanel.SetActive(false);
            }
        }
    }

    private void ScaleUpUIForFHD()
    {
        Vector3 newScale = new Vector3(scaleFactor, scaleFactor, 1f);

        if (titleTextRect != null)
        {
            titleTextRect.localScale = newScale;
            titleTextRect.sizeDelta = new Vector2(600, 150); 
        }

        if (buttonGroupRect != null)
        {
            buttonGroupRect.localScale = newScale;
            buttonGroupRect.sizeDelta = new Vector2(500, 300); 
        }
    }

    // ==========================================
    //               버튼 이벤트 함수들
    // ==========================================

    // 💡 1. 로비의 StartButton을 누르면 실행되는 함수
    public void OnClickGameStart()
    {
        if (saveSlotPanel != null) 
        {
            saveSlotPanel.SetActive(true); // 이제 바로 게임씬으로 안 가고 슬롯 창을 켭니다!
        }
    }

    // 💡 2. 슬롯 버튼(Slot 1, Slot 2, Slot 3)을 누르면 실행되는 함수
    public void OnClickSaveSlot(int slotIndex)
    {
        Debug.Log($"슬롯 {slotIndex}번이 선택되었습니다. 데이터를 준비하고 완전히 새로운 미션 UI 화면으로 전환합니다.");
        
        // [중요 비즈니스 로직 영역]
        // 어떤 슬롯을 선택했는지 기억하는 임시 코드 (나중에 JSON 로드할 때 씁니다)
        PlayerPrefs.SetInt("SelectedSlotID", slotIndex);
        PlayerPrefs.Save();

        // 💡 씬 매니저를 사용하여 완전히 새로운 'MissionUI' 씬으로 화면을 전환합니다!
        SceneManager.LoadScene(missionSceneName);
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
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 플레이 종료
        #else
        Application.Quit(); // 빌드된 게임 종료
        #endif
    }

    public void OnClickQuitCancel()
    {
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }
}