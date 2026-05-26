using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSceneManager : MonoBehaviour
{
    [Header("메인 팝업 패널들")]
    [SerializeField] private GameObject missionPanel;       // 미션 선택 전체 패널
    [SerializeField] private GameObject confirmPanel;       // "수락하시겠습니까?" 확인 팝업창

    [Header("이동할 씬 이름들")]
    [SerializeField] private string storeSceneName = "StoreUI";      // 상점 씬 이름
    [SerializeField] private string gameScenePrefix = "GameScene_";  // 게임 스테이지 씬 이름 앞글자

    // 플레이어가 현재 어떤 장소(미션)를 클릭했는지 기억하는 변수
    private int selectedMissionIndex = -1;

    void Start()
    {
        // 화면 시작 시 모든 팝업창을 확실하게 꺼둡니다.
        if (missionPanel != null) missionPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void Update()
    {
        // ESC 키를 누르면 열려 있는 창들을 순서대로 닫아줍니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmPanel != null && confirmPanel.activeSelf)
            {
                OnClickConfirmNo(); // 확인창 닫기
            }
            else if (missionPanel != null && missionPanel.activeSelf)
            {
                missionPanel.SetActive(false); // 미션 패널 닫기
            }
        }
    }

    // ==========================================
    //          [1] 메인 화면 버튼 기능
    // ==========================================

    // MissionButton 누르면 실행: 미션 패널 열기
    public void OnClickMissionOpen()
    {
        if (missionPanel != null) missionPanel.SetActive(true);
    }

    // StoreButton 누르면 실행: 상점 씬(StoreUI)으로 이동
    public void OnClickGoToStore()
    {
        SceneManager.LoadScene(storeSceneName);
    }

    // ==========================================
    //         [2] 미션 패널 내 장소 버튼 기능
    // ==========================================

    // 6개의 장소 버튼을 누르면 실행되는 함수 (인스펙터에서 각각 1~6 입력)
    public void OnClickSelectPlace(int placeIndex)
    {
        selectedMissionIndex = placeIndex;
        Debug.Log($"{placeIndex}번 장소가 선택되었습니다. 확인 창을 띄웁니다.");

        // "수락하시겠습니까?" 창을 켭니다.
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    // ==========================================
    //         [3] 수락 / 거절 (예 / 아니오) 기능
    // ==========================================

    // 💡 '예' 버튼을 누르면 실행: 로딩 화면을 거쳐 게임 화면으로 이동!
    public void OnClickConfirmYes()
    {
        if (selectedMissionIndex != -1)
        {
            Debug.Log($"{selectedMissionIndex}번 장소 선택 완료. 로딩 화면으로 진입합니다.");
            
            // 1. 로딩 매니저에게 "우리 로딩 끝나면 'GameScene_X'로 갈 거야"라고 미리 타겟을 알려줍니다.
            LoadingSceneManager.nextSceneName = gameScenePrefix + selectedMissionIndex;

            // 2. 그리고 실제 게임 씬이 아닌, [로딩 화면 씬]을 먼저 불러옵니다!
            SceneManager.LoadScene("LoadingScene"); 
        }
    }

    // '아니오' 버튼을 누르면 실행: 확인 창만 닫고 다시 고르게 함
    public void OnClickConfirmNo()
    {
        Debug.Log("작전을 거절했습니다. 다시 장소를 선택합니다.");
        selectedMissionIndex = -1; // 선택 초기화

        if (confirmPanel != null) confirmPanel.SetActive(false);
    }
}