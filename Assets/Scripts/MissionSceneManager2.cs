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
    [SerializeField] private GameObject companyNormalButton;    
    [SerializeField] private GameObject companyClearedIcon;     

    [Header("🏥 2. 병원 (2번 스테이지) 부품들")]
    [SerializeField] private GameObject hospitalNormalButton;   
    [SerializeField] private GameObject hospitalClearedIcon;    

    private int selectedMissionIndex = -1; 

    void Start()
    {
        if (missionPanel != null) missionPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

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

    // 🎯 [핵심 변경 구역] 팀원분이 만든 진짜 맵 씬 이름으로 다이렉트 매칭!
    public void OnClickConfirmYes()
    {
        if (selectedMissionIndex != -1)
        {
            // 플레이어가 고른 스테이지 번호 저장
            PlayerPrefs.SetInt("CurrentPlayingStageIndex", selectedMissionIndex);
            
            // 🗺️ 기본 목적지는 1번 회사 씬 이름으로 세팅
            string targetSceneName = "Map1_Office"; 
            
            // 🗺️ 만약 고른 번호가 2번 병원이라면 목적지를 병원 씬 이름으로 변경!
            if (selectedMissionIndex == 2) 
            {
                targetSceneName = "Map2_Hospital"; 
            }

            // 🚀 로딩 매니저에게 조립된 진짜 목적지 씬 이름을 던져주고 로딩창을 켭니다!
            LoadingSceneManager.nextSceneName = targetSceneName; 
            SceneManager.LoadScene("LoadingUI"); 
        }
    }

    public void OnClickConfirmNo()
    {
        selectedMissionIndex = -1; 
        if (confirmPanel != null && confirmPanel.activeSelf) confirmPanel.SetActive(false);
    }
}