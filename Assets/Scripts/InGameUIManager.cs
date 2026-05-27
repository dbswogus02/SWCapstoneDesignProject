using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 🛑 일반 UI Text와 Image를 쓰기 위해 추가된 라이브러리입니다!
using UnityEngine.SceneManagement;
using TMPro; // 🛑 HUD UI (HP, 타이머, 킬수)의 TextMeshProUGUI를 유지하기 위한 라이브러리입니다!

public class InGameUIManager : MonoBehaviour
{
    // 다른 팀원들의 스크립트(좀비, 플레이어 등)에서 우진님 매니저를 편하게 부를 수 있게 싱글톤 장착!
    public static InGameUIManager instance;

    [Header("=== 1. HUD UI (평소 켜짐 - 순정 텍스트메시프로 유지) ===")]
    [SerializeField] private Image hpCircleImage;       
    [SerializeField] private TextMeshProUGUI hpText;       // 🔒 순정 유지!
    [SerializeField] private TextMeshProUGUI timerText;    // 🔒 순정 유지!
    [SerializeField] private TextMeshProUGUI killCountText; // 🔒 순정 유지!
    [SerializeField] private Image expFillImage;        

    [Header("=== 2. 보스 출현 및 클리어 UI (순정 텍스트메시프로 유지) ===")]
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

    [Header("=== 👑 5. [우진님 기획 5종 완벽 반영] 증강 UI ===")]
    [SerializeField] private GameObject augmentPanel;       // 레벨업 시 켜질 증강 팝업 전체 Panel
    [SerializeField] private Button augmentButton1;         // 1번 선택지 버튼
    [SerializeField] private Button augmentButton2;         // 2번 선택지 버튼
    [SerializeField] private Button augmentButton3;         // 3번 선택지 버튼
    
    // 🎯 [우진님 피드백 반영] 다른 곳은 건드리지 않고, 오직 여기 증강 글씨 3개만 일반 UI Text로 전격 교체!
    [SerializeField] private Text augmentText1;  // 1번 버튼의 자식 글씨 (일반 UI Text 자석 연결 가능! 🧲)
    [SerializeField] private Text augmentText2;  // 2번 버튼의 자식 글씨 (일반 UI Text 자석 연결 가능! 🧲)
    [SerializeField] private Text augmentText3;  // 3번 버튼의 자식 글씨 (일반 UI Text 자석 연결 가능! 🧲)

    // === 🛠️ [중요] 나중에 다른 팀원 코드(스탯, 좀비)와 연동할 실시간 증강 수치 스태틱 변수들 ===
    public static float BonusEXPPerKill = 0f;        // [야간 수당] 누적 보너스 경험치 (기본 0, 선택 시 +0.5씩 무한 누적)
    public static float SpeedModifier = 1.0f;        // [기능성 안전화] 이동 속도 계수 (기본 1.0, 선택 시 +1%인 +0.01f씩 무한 누적)
    public static float AvoidChance = 0f;            // [방역 패드] 데미지 무효화 확률 (기본 0, 선택 시 +2%인 +0.02f씩 무한 누적)

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

    // 현재 화면 3개의 버튼에 무작위로 등장한 증강 ID(1~5)를 기억할 장부 배열
    private int[] activeAugmentIDs = new int[3];

    private void Awake()
    {
        // 어디서나 소통 가능한 글로벌 하이패스 통로 개설
        instance = this;

        // 게임이 새로 켜질 때마다 지난 판에 누적되었던 증강 스펙 수치 깔끔하게 초기화
        BonusEXPPerKill = 0f;
        SpeedModifier = 1.0f;
        AvoidChance = 0f;
    }

    void Start()
    {
        if (pauseParentPanel != null) pauseParentPanel.SetActive(false);
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (quitQuestionText != null) quitQuestionText.SetActive(false);
        if (infoWindow != null) infoWindow.SetActive(false);
        if (clearPanel != null) clearPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
        if (augmentPanel != null) augmentPanel.SetActive(false); // 증강창은 평소엔 숨겨두기

        UpdateHPUI();
        UpdateKillUI();
        UpdateEXPUI();
        
        hitCount = 0; 
        Time.timeScale = 1f; 
    }

    void Update()
    {
        if (isGameOver || isGameCleared || (augmentPanel != null && augmentPanel.activeSelf)) return; 

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
    // 🎨 UI 실시간 갱신 및 연출 구역
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

    public void AddExperience(float amount)
    {
        if (isGameOver || isGameCleared) return;

        currentEXP += (amount + BonusEXPPerKill);
        
        if (currentEXP >= maxEXP)
        {
            currentEXP -= maxEXP; 
            TriggerLevelUpAugment(); 
        }

        UpdateEXPUI();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isGameOver || isGameCleared) return;

        if (Random.value < AvoidChance)
        {
            Debug.Log($"🛡️ [방역 패드 작동 성공] {AvoidChance * 100}% 확률 적중! 데미지를 완전히 무효화했습니다.");
            return;
        }

        currentHP -= damageAmount;
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
    // 👑 🎲 [우진님 오리지널 정식 5종] 랜덤 증강 시스템 구역
    // ==========================================
    private void TriggerLevelUpAugment()
    {
        Time.timeScale = 0f; 
        if (augmentPanel != null) augmentPanel.SetActive(true);

        List<int> idList = new List<int> { 1, 2, 3, 4, 5 };
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, idList.Count);
            activeAugmentIDs[i] = idList[randomIndex];
            idList.RemoveAt(randomIndex); 
        }

        SetAugmentButtonUI(augmentText1, activeAugmentIDs[0]);
        SetAugmentButtonUI(augmentText2, activeAugmentIDs[1]);
        SetAugmentButtonUI(augmentText3, activeAugmentIDs[2]);

        if (augmentButton1 != null) { augmentButton1.onClick.RemoveAllListeners(); augmentButton1.onClick.AddListener(() => OnSelectAugment(activeAugmentIDs[0])); }
        if (augmentButton2 != null) { augmentButton2.onClick.RemoveAllListeners(); augmentButton2.onClick.AddListener(() => OnSelectAugment(activeAugmentIDs[2])); }
        if (augmentButton3 != null) { augmentButton3.onClick.RemoveAllListeners(); augmentButton3.onClick.AddListener(() => OnSelectAugment(activeAugmentIDs[2])); }
    }

    // 🎯 우진님 피드백 완벽 반영: 받아오는 인자 타입을 TextMeshProUGUI 대신 일반 Text로 영리하게 교체!
    private void SetAugmentButtonUI(Text targetText, int augmentID)
    {
        if (targetText == null) return;

        // 일반 UI Text도 <b> 태그(Rich Text 기능)가 기본 지원되므로 우진님의 명품 디자인이 그대로 유지됩니다!
        switch (augmentID)
        {
            case 1: targetText.text = "<b>[응급 처치]</b>\n즉시 HP 10을 회복합니다."; break;
            case 2: targetText.text = "<b>[방호복 보강]</b>\n최대 HP 총량이 10 늘어납니다."; break;
            case 3: targetText.text = "<b>[야간 수당]</b>\n몬스터 처치 시 경험치 획득량이 0.5 증가합니다."; break;
            case 4: targetText.text = "<b>[기능성 안전화]</b>\n플레이어의 이동 속도가 1% 증가합니다."; break;
            case 5: targetText.text = "<b>[방역 패드]</b>\n피격 시 2%의 확률로 데미지를 무효화합니다."; break;
        }
    }

    private void OnSelectAugment(int augmentID)
    {
        switch (augmentID)
        {
            case 1: // 🩹 [응급 처치] -> 일회성 즉시 회복 (최대 체력 통 절대 초과 불가능!)
                currentHP += 10f;
                if (currentHP > maxHP) 
                {
                    currentHP = maxHP; 
                }
                UpdateHPUI();
                Debug.Log($"🩹 [응급 처치 고름] 일회성 회복 완료. (현재 체력: {currentHP}/{maxHP})");
                break;

            case 2: // 🦺 [방호복 보강] -> 최대 HP '통만' 상승 (현재 체력 숫자는 그대로 보존!)
                maxHP += 10f; 
                UpdateHPUI();
                Debug.Log($"🦺 [방호복 보강 고름] 최대 체력 통 10 증가! (현재 체력: {currentHP}/{maxHP})");
                break;

            case 3: // ⚡ [야간 수당] -> 고를 때마다 0.5씩 영구 무한 중첩 축적
                BonusEXPPerKill += 0.5f; 
                Debug.Log($"⚡ [야간 수당 고름] 추가 경험치 버프 중첩! (현재 총 킬당 보너스: +{BonusEXPPerKill})");
                break;

            case 4: // 👟 [기능성 안전화] -> 고를 때마다 1%씩 영구 무한 중첩 축적
                SpeedModifier += 0.01f; 
                Debug.Log($"👟 [기능성 안전화 고름] 이동 속도 버프 중첩! (현재 속도 버프 계수: {SpeedModifier * 100}%)");
                break;

            case 5: // 🛡️ [방역 패드] -> 고를 때마다 2%씩 영구 무한 중첩 축적
                AvoidChance += 0.02f; 
                Debug.Log($"🛡️ [방역 패드 고름] 데미지 무효화 확률 스택 누적! (현재 총 확률: {AvoidChance * 100}%)");
                break;
        }

        if (augmentPanel != null) augmentPanel.SetActive(false);
        Time.timeScale = 1f; 
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

        if (isGameOver || isGameCleared || (augmentPanel != null && augmentPanel.activeSelf)) return;

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

            int currentStageIndex = PlayerPrefs.GetInt("CurrentPlayingStageIndex", 1);
            
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

    public void OnClickGoToCompany()
    {
        Time.timeScale = 1f; 
        LoadingSceneManager.nextSceneName = "MissionUI"; 
        SceneManager.LoadScene("LoadingUI"); 
    }

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
        if (isGameOver || isGameCleared || (augmentPanel != null && augmentPanel.activeSelf)) return; 

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(20f); 
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            currentKills++;
            if (currentKills > targetKills) currentKills = targetKills;
            UpdateKillUI();

            // ⚡ 치트키 K를 4번 연속 연타하면 경험치 100이 채워져서 랜덤 증강 5종 팝업 연출을 무한대로 테스트할 수 있습니다!
            AddExperience(25f); 
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TriggerStageClear();
        }
    }
}