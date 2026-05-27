using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    [Header("=== 1. HUD UI (평소 켜짐) ===")]
    [SerializeField] private Image hpCircleImage;       
    [SerializeField] private TextMeshProUGUI hpText;    
    [SerializeField] private TextMeshProUGUI timerText; 
    [SerializeField] private TextMeshProUGUI killCountText; 
    [SerializeField] private Image expFillImage;        

    [Header("=== 2. 보스 출현 및 클리어 UI ===")]
    [SerializeField] private GameObject clearPanel; 
    [SerializeField] private TextMeshProUGUI warningText; 
    [SerializeField] private TextMeshProUGUI stageTimeText; 
    [SerializeField] private TextMeshProUGUI stageKillText; 
    [SerializeField] private RectTransform thumbsUp1;          
    [SerializeField] private RectTransform thumbsUp2;          
    [SerializeField] private RectTransform thumbsUp3;          

    [Header("=== 3. ESC 메뉴 & 종료 UI ===")]
    [SerializeField] private GameObject pauseParentPanel;  
    [SerializeField] private GameObject mainMenuGroup;     
    [SerializeField] private GameObject quitQuestionText;  

    [Header("=== 4. 게임 오버 UI ===")]
    [SerializeField] private GameObject infoWindow;         
    [SerializeField] private TextMeshProUGUI totalSurvivalTimeText; 
    [SerializeField] private TextMeshProUGUI totalKillText;         

    // === 인게임 데이터 ===
    private float currentHP = 100f;
    private float maxHP = 100f;
    private float currentEXP = 0f;
    private float maxEXP = 100f; 
    private float elapsedTime = 0f; 
    private int currentKills = 0;
    private int targetKills = 120; 
    private int hitCount = 0; 

    private bool isWarningTriggered = false;
    private bool isGamePaused = false; 
    private bool isGameOver = false; 
    private bool isGameCleared = false;

    void Start()
    {
        if (pauseParentPanel != null) pauseParentPanel.SetActive(false);
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (quitQuestionText != null) quitQuestionText.SetActive(false);
        if (infoWindow != null) infoWindow.SetActive(false);
        if (clearPanel != null) clearPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);

        UpdateHPUI();
        UpdateKillUI();
        UpdateEXPUI();
        
        hitCount = 0; 
        Time.timeScale = 1f; 
    }

    void Update()
    {
        if (isGameOver || isGameCleared) return; 

        if (!isGamePaused)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscInput();
        }

        HandleDebugInputs();
    }

    // ==========================================
    // 🎨 UI 실시간 갱신 및 보스 연출
    // ==========================================
    private void UpdateHPUI()
    {
        if (hpCircleImage != null) hpCircleImage.fillAmount = currentHP / maxHP;
        if (hpText != null) hpText.text = $"HP: {(int)currentHP} / {(int)maxHP}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateKillUI()
    {
        if (killCountText != null)
        {
            killCountText.text = $"Kill ( {currentKills:00} / {targetKills:00} )";
            
            if (currentKills >= targetKills)
            {
                killCountText.color = Color.green;

                if (!isWarningTriggered && warningText != null)
                {
                    isWarningTriggered = true;
                    StartCoroutine(AnimateWarningDiagonal());
                }
            }
        }
    }

    private void UpdateEXPUI()
    {
        if (expFillImage != null) expFillImage.fillAmount = currentEXP / maxEXP;
    }

    private IEnumerator AnimateWarningDiagonal()
    {
        warningText.gameObject.SetActive(true);
        RectTransform rectTransform = warningText.GetComponent<RectTransform>();

        Vector3 startPos = new Vector3(-Screen.width * 0.6f, Screen.height * 0.6f, 0);
        Vector3 endPos = new Vector3(Screen.width * 0.6f, -Screen.height * 0.6f, 0);

        float duration = 2.5f;   
        float timer = 0f;
        float blinkInterval = 0.4f; 
        float nextBlinkTime = 0f;

        while (timer < duration)
        {
            if (!isGamePaused)
            {
                timer += Time.deltaTime;
                float progress = timer / duration;
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, progress);

                if (timer >= nextBlinkTime)
                {
                    warningText.enabled = !warningText.enabled;
                    nextBlinkTime += blinkInterval;
                }
            }
            yield return null;
        }
        warningText.gameObject.SetActive(false);
    }

    // ==========================================
    // 🎲 ESC 일시정지 로직
    // ==========================================
    private void HandleEscInput()
    {
        if (isGamePaused && quitQuestionText != null && quitQuestionText.activeSelf)
        {
            OnClickCancelQuit(); 
            return;
        }

        if (isGameOver || isGameCleared) return;

        isGamePaused = !isGamePaused; 

        if (isGamePaused)
        {
            Time.timeScale = 0f; 
            if (pauseParentPanel != null) pauseParentPanel.SetActive(true);
            if (mainMenuGroup != null) mainMenuGroup.SetActive(true);       
            if (quitQuestionText != null) quitQuestionText.SetActive(false); 
        }
        else
        {
            Time.timeScale = 1f; 
            if (pauseParentPanel != null) pauseParentPanel.SetActive(false);
        }
    }

    public void OnClickResume()
    {
        isGamePaused = false;
        Time.timeScale = 1f; 
        if (pauseParentPanel != null) pauseParentPanel.SetActive(false);
    }

    public void OnClickGoToQuitMatch()
    {
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);     
        if (quitQuestionText != null) quitQuestionText.SetActive(true); 
    }

    public void OnClickCancelQuit()
    {
        if (mainMenuGroup != null) mainMenuGroup.SetActive(true);       
        if (quitQuestionText != null) quitQuestionText.SetActive(false); 
    }

    public void OnClickConfirmQuitToMenu()
    {
        Time.timeScale = 1f; 
        LoadingSceneManager.nextSceneName = "MyLobby"; 
        SceneManager.LoadScene("LoadingUI"); 
    }

    // ==========================================
    // 🏆 2. 보스 처치 및 스테이지 클리어
    // ==========================================
    public void TriggerStageClear()
    {
        if (isGameCleared || isGameOver) return;

        isGameCleared = true;
        Time.timeScale = 0f; 

        if (clearPanel != null)
        {
            clearPanel.SetActive(true); 

            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime % 60F);
            
            if (stageTimeText != null) stageTimeText.text = $"Survival Time : ( {minutes:00} : {seconds:00} )";
            if (stageKillText != null) stageKillText.text = $"Kills : ( {currentKills:00} )";

            // 어떤 스테이지를 통해 들어왔는지 번호를 낚아챕니다. (기본값 1번 회사)
            int currentStageIndex = PlayerPrefs.GetInt("CurrentPlayingStageIndex", 1);
            
            // 깬 번호의 클리어 장부에 성공 도장(1)을 쿵 찍어줍니다!
            PlayerPrefs.SetInt($"Stage{currentStageIndex}_Cleared", 1);
            PlayerPrefs.Save();
            Debug.Log($"🎯 스테이지 {currentStageIndex}번 클리어 데이터가 안전하게 누적 저장되었습니다!");

            StartCoroutine(ClearStageSequence());
            SaveCumulativeData();
        }
    }

    private IEnumerator ClearStageSequence()
    {
        RectTransform panelRect = clearPanel.GetComponent<RectTransform>();
        panelRect.localScale = Vector3.zero; 

        float duration = 0.25f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; 
            float progress = timer / duration;

            float curve = Mathf.Sin(progress * Mathf.PI * 0.75f) * 1.15f; 
            float currentScale = Mathf.Clamp(curve, 0f, 1.15f);
            
            if (progress >= 0.9f) currentScale = Mathf.Lerp(currentScale, 1.0f, progress);

            panelRect.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }
        panelRect.localScale = Vector3.one;

        yield return StartCoroutine(StampThumbsUpProcess());
    }

    private IEnumerator StampThumbsUpProcess()
    {
        int totalThumbs = 0;

        if (elapsedTime <= 300f && hitCount <= 1) totalThumbs = 3; 
        else if (elapsedTime <= 420f && hitCount <= 3) totalThumbs = 2; 
        else totalThumbs = 1; 

        List<RectTransform> thumbsList = new List<RectTransform> { thumbsUp1, thumbsUp2, thumbsUp3 };

        for (int i = 0; i < thumbsList.Count; i++)
        {
            if (thumbsList[i] != null) thumbsList[i].gameObject.SetActive(false);
        }

        yield return new WaitForSecondsRealtime(0.2f); 

        for (int i = 0; i < totalThumbs; i++)
        {
            if (thumbsList[i] == null) continue;

            thumbsList[i].gameObject.SetActive(true); 
            thumbsList[i].localScale = new Vector3(4f, 4f, 1f); 

            float stampDuration = 0.15f; 
            float stampTimer = 0f;

            while (stampTimer < stampDuration)
            {
                stampTimer += Time.unscaledDeltaTime;
                float progress = stampTimer / stampDuration;
                
                float currentScale = Mathf.Lerp(4f, 1f, progress);
                thumbsList[i].localScale = new Vector3(currentScale, currentScale, 1f);
                
                yield return null; 
            }

            thumbsList[i].localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(0.25f); 
        }

        for (int i = totalThumbs; i < thumbsList.Count; i++)
        {
            if (thumbsList[i] != null) thumbsList[i].gameObject.SetActive(false);
        }
    }

    public void OnClickClearRetry()
    {
        Time.timeScale = 1f; 
        LoadingSceneManager.nextSceneName = SceneManager.GetActiveScene().name; 
        SceneManager.LoadScene("LoadingUI"); 
    }

    // 🎯 [우진님 기획 수정 완료] 클리어 후 복귀할 목적지 주소를 'MissionUI'로 다이렉트 변경!
    public void OnClickGoToCompany()
    {
        Time.timeScale = 1f; 
        LoadingSceneManager.nextSceneName = "MissionUI"; 
        SceneManager.LoadScene("LoadingUI"); 
    }

    // ==========================================
    // 💀 3. 게임 오버 로직
    // ==========================================
    public void TriggerGameOver()
    {
        if (isGameOver || isGameCleared) return;

        isGameOver = true;
        Time.timeScale = 0f; 

        if (infoWindow != null)
        {
            infoWindow.SetActive(true); 
            SaveCumulativeData();

            float accumulatedTime = PlayerPrefs.GetFloat("TotalSurvivalTime", 0f);
            int accumulatedKills = PlayerPrefs.GetInt("TotalKills", 0);

            int minutes = Mathf.FloorToInt(accumulatedTime / 60F);
            int seconds = Mathf.FloorToInt(accumulatedTime % 60F);
            
            if (totalSurvivalTimeText != null) 
                totalSurvivalTimeText.text = $"Total Survival Time : ( {minutes:00} : {seconds:00} )";
                
            if (totalKillText != null) 
                totalKillText.text = $"Total Kill : ( {accumulatedKills:00} )";

            ResetRoguelikeSaveSlot();
        }
    }

    private void SaveCumulativeData()
    {
        float previousTotalTime = PlayerPrefs.GetFloat("TotalSurvivalTime", 0f);
        int previousTotalKills = PlayerPrefs.GetInt("TotalKills", 0);

        float newTotalTime = previousTotalTime + elapsedTime;
        int newTotalKills = previousTotalKills + currentKills;

        PlayerPrefs.SetFloat("TotalSurvivalTime", newTotalTime);
        PlayerPrefs.SetInt("TotalKills", newTotalKills);
        PlayerPrefs.Save(); 
    }

    private void ResetRoguelikeSaveSlot()
    {
        PlayerPrefs.SetFloat("TotalSurvivalTime", 0f);
        PlayerPrefs.SetInt("TotalKills", 0);
        PlayerPrefs.SetInt("Slot1_HasSaveData", 0); 

        // 죽었을 때 1번부터 5번까지의 미션 클리어 데이터도 함께 지워 체크마크를 일반 버튼으로 초기화합니다.
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.SetInt($"Stage{i}_Cleared", 0);
        }

        PlayerPrefs.Save();
        Debug.Log("💀 유저가 사망하여 모든 미션 클리어 마크 데이터가 초기화되었습니다.");
    }

    public void OnClickBackToLobby()
    {
        Time.timeScale = 1f; 
        LoadingSceneManager.nextSceneName = "MyLobby"; 
        SceneManager.LoadScene("LoadingUI"); 
    }

    private void HandleDebugInputs()
    {
        if (isGameOver || isGameCleared) return; 

        if (Input.GetKeyDown(KeyCode.H))
        {
            currentHP -= 20f; 
            hitCount++; 
            if (currentHP <= 0)
            {
                currentHP = 0;
                UpdateHPUI();
                TriggerGameOver(); 
                return;
            }
            UpdateHPUI();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            currentKills++;
            if (currentKills > targetKills) currentKills = targetKills;
            UpdateKillUI();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TriggerStageClear();
        }
    }
}