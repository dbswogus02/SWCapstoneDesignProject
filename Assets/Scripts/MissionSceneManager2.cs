using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;          
using UnityEngine.SceneManagement;

public class MissionSceneManager2 : MonoBehaviour
{
    [Header("메인 팝업 패널들")]
    [SerializeField] private GameObject missionPanel;       
    [SerializeField] private GameObject confirmPanel;       
    [SerializeField] private Text confirmText;              

    [Header("이동할 씬 이름들")]
    [SerializeField] private string storeSceneName = "StoreUI";      

    [Header("🏢 1. 회사 (1번 스테이지) 부품들")]
    [SerializeField] private GameObject companyNormalButton;    // 원래 회사 버튼
    [SerializeField] private GameObject companyClearedIcon;     // 회사 클리어 체크마크 아이콘

    [Header("🏥 2. 병원 (2번 스테이지) 부품들")]
    [SerializeField] private GameObject hospitalNormalButton;   // 원래 병원 버튼
    [SerializeField] private GameObject hospitalClearedIcon;    // 병원 클리어 체크마크 아이콘

    private int selectedMissionIndex = -1; 

    void Start()
    {
        if (missionPanel != null) missionPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // 씬 시작할 때 저장된 데이터를 보고 버튼들을 똑똑하게 스위칭합니다.
        CheckAllStages();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmPanel != null && confirmPanel.activeSelf) OnClickConfirmNo(); 
            else if (missionPanel != null && missionPanel.activeSelf) missionPanel.SetActive(false); 
        }
    }

    // 각 스테이지가 깨졌는지 독립적으로 정밀 검사 (버그 발생 확률 0%)
    private void CheckAllStages()
    {
        // === 1. 회사 검사 ===
        if (companyNormalButton != null && companyClearedIcon != null)
        {
            if (PlayerPrefs.GetInt("Stage1_Cleared", 0) == 1)
            {
                companyNormalButton.SetActive(false);
                companyClearedIcon.SetActive(true);
            }
            else
            {
                companyNormalButton.SetActive(true);
                companyClearedIcon.SetActive(false);
            }
        }

        // === 2. 병원 검사 ===
        if (hospitalNormalButton != null && hospitalClearedIcon != null)
        {
            if (PlayerPrefs.GetInt("Stage2_Cleared", 0) == 1)
            {
                hospitalNormalButton.SetActive(false);
                hospitalClearedIcon.SetActive(true);
            }
            else
            {
                hospitalNormalButton.SetActive(true);
                hospitalClearedIcon.SetActive(false);
            }
        }
    }

    public void OnClickMissionOpen()
    {
        if (missionPanel != null) missionPanel.SetActive(true);
    }

    public void OnClickGoToStore()
    {
        SceneManager.LoadScene(storeSceneName);
    }

    // 🎯 숫자가 절대 꼬이지 않게 우진님이 인스펙터 버튼에서 수동으로 먹인 번호를 그대로 읽습니다.
    public void OnClickSelectPlace(int placeIndex)
    {
        selectedMissionIndex = placeIndex;
        string placeName = "알 수 없는 지역";

        if (placeIndex == 1) placeName = "회사";
        else if (placeIndex == 2) placeName = "병원";

        if (confirmText != null)
        {
            if (PlayerPrefs.GetInt($"Stage{placeIndex}_Cleared", 0) == 1)
            {
                confirmText.text = $"{placeName} 작전을\n재도전하시겠습니까?";
            }
            else
            {
                confirmText.text = $"{placeName} 작전을\n시작하시겠습니까?";
            }
        }

        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    public void OnClickConfirmYes()
    {
        if (selectedMissionIndex != -1)
        {
            PlayerPrefs.SetInt("CurrentPlayingStageIndex", selectedMissionIndex);
            LoadingSceneManager.nextSceneName = "InGameUI"; 
            SceneManager.LoadScene("LoadingUI"); 
        }
    }

    public void OnClickConfirmNo()
    {
        selectedMissionIndex = -1; 
        if (confirmPanel != null && confirmPanel.activeSelf) confirmPanel.SetActive(false);
    }
}