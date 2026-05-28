using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 👈 일반 Text 컴포넌트를 쓰기 위해 반드시 필요한 필수 장부입니다!
using UnityEngine.SceneManagement;
using TMPro; 

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager instance;

    [Header("=== 1. HUD UI (평소 켜짐 - 순정 텍스트메시프로 유지) ===")]
    [SerializeField] private Image hpCircleImage;       
    [SerializeField] private TextMeshProUGUI hpText;       
    [SerializeField] private TextMeshProUGUI timerText;    
    [SerializeField] private TextMeshProUGUI killCountText; 
    [SerializeField] private Image expFillImage;        

    [Header("=== 2. 클리어 및 경고 연출 UI ===")]
    [SerializeField] private GameObject clearPanel; 
    [SerializeField] private TextMeshProUGUI warningText; 
    [SerializeField] private TextMeshProUGUI stageTimeText; 
    [SerializeField] private TextMeshProUGUI stageKillText; 
    
    // 💰 [우진님 피드백 반영] TextMeshProUGUI를 일반 Text 컴포넌트로 완벽 교체!
    [SerializeField] private Text stageMoneyText; // 👈여기에 하이어라키의 StageMoneyText를 드래그해 넣으세요!
    
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
    [SerializeField] private GameObject augmentPanel;       
    [SerializeField] private Button augmentButton1;         
    [SerializeField] private Button augmentButton2;         
    [SerializeField] private Button augmentButton3;         
    [SerializeField] private Text augmentText1;  
    [SerializeField] private Text augmentText2;  
    [SerializeField] private Text augmentText3;  

    // === 🛠️ 실시간 증강 수치 장부 ===
    public static float BonusEXPPerKill = 0f;        
    public static float SpeedModifier = 1.0f;        
    public static float AvoidChance = 0f;            

    // === 인게임 데이터 ===
    private float currentHP = 100f;
    private float maxHP = 100f;
    private float currentEXP = 0f;
    private float maxEXP = 100f; 
    private float elapsedTime = 0f; 
    private int currentKills = 0;
    private int hitCount = 0; 

    // 맵 정보 목표 수치 분기 장부
    private int warningKillTarget = 30;  
    private int clearKillTarget = 50;    

    private bool isWarningTriggered = false;
    private bool isGamePaused = false; 
    private bool isGameOver = false; 
    private bool isGameCleared = false;

    private int[] activeAugmentIDs = new int[3];

    // === 🕵️‍♂️ [팀원 원본 추적용 스파이 변수] ===
    private PlayerHealth targetPlayerHealth;
    private PlayerController targetPlayerController;
    private float originalPlayerMoveSpeed = 4f; 
    private List<EnemyHealth> activeEnemiesList = new List<EnemyHealth>(); 
    private int previousTrackedHP = 100;

    // ⏳ 사망 연출 추적용 내부 제어 변수
    private bool isSpyDeathTriggered = false;

    private void Awake()
    {
        instance = this;
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
        if (augmentPanel != null) augmentPanel.SetActive(false);
        
        // 🧼 게임 시작할 때는 보상 문구를 안전하게 숨겨둡니다.
        if (stageMoneyText != null) stageMoneyText.gameObject.SetActive(false);

        int currentStageIndex = PlayerPrefs.GetInt("CurrentPlayingStageIndex", 1); 
        if (currentStageIndex == 1)
        {
            warningKillTarget = 30;   
            clearKillTarget = 50;     
        }
        else if (currentStageIndex == 2)
        {
            warningKillTarget = 130;  
            clearKillTarget = 201;    
        }

        targetPlayerHealth = FindObjectOfType<PlayerHealth>();
        targetPlayerController = FindObjectOfType<PlayerController>();

        if (targetPlayerController != null)
        {
            originalPlayerMoveSpeed = targetPlayerController.moveSpeed; 
        }

        if (targetPlayerHealth != null)
        {
            maxHP = targetPlayerHealth.maxHealth;
            System.Reflection.FieldInfo hpField = typeof(PlayerHealth).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null) currentHP = (int)hpField.GetValue(targetPlayerHealth);
            previousTrackedHP = (int)currentHP;
        }

        UpdateHPUI();
        UpdateKillUI();
        UpdateEXPUI();
        
        hitCount = 0; 
        isSpyDeathTriggered = false;
        Time.timeScale = 1f; 
    }

    void Update()
    {
        if (isGameOver || isGameCleared || (augmentPanel != null && augmentPanel.activeSelf)) return; 

        UpdateSpyRadar();

        if (!isGamePaused && !isSpyDeathTriggered)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscInput();

        HandleDebugInputs();
    }

    // ==========================================
    // 🕵️‍♂️ 3대 핵심 컴포넌트 실시간 원격 레이더 구역
    // ==========================================
    private void UpdateSpyRadar()
    {
        if (targetPlayerHealth != null)
        {
            maxHP = targetPlayerHealth.maxHealth;
            System.Reflection.FieldInfo hpField = typeof(PlayerHealth).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                int realTimeHP = (int)hpField.GetValue(targetPlayerHealth);

                if (realTimeHP < previousTrackedHP)
                {
                    if (Random.value < AvoidChance)
                    {
                        Debug.Log($"🛡️ [방역 패드 발동] 데미지 원천 무효화! 팀원 모르게 피를 롤백합니다.");
                        hpField.SetValue(targetPlayerHealth, previousTrackedHP); 
                        realTimeHP = previousTrackedHP;
                    }
                    else
                    {
                        hitCount++; 
                    }
                }

                currentHP = realTimeHP;
                previousTrackedHP = realTimeHP;
            }
            UpdateHPUI();
        }

        if (targetPlayerController != null && !isSpyDeathTriggered)
        {
            System.Reflection.FieldInfo deadField = typeof(PlayerController).GetField("isDead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (deadField != null)
            {
                bool isPlayerControllerDead = (bool)deadField.GetValue(targetPlayerController);

                if (isPlayerControllerDead)
                {
                    isSpyDeathTriggered = true; 
                    StartCoroutine(WaitForDeathAnimationSequence(1.8f));
                }
            }
        }

        EnemyHealth[] monsters = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth monster in monsters)
        {
            if (!activeEnemiesList.Contains(monster) && monster != null)
            {
                System.Reflection.FieldInfo enemyHPField = typeof(EnemyHealth).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (enemyHPField != null)
                {
                    int enemyHP = (int)enemyHPField.GetValue(monster);
                    if (enemyHP > 0) 
                    {
                        activeEnemiesList.Add(monster);
                    }
                }
            }
        }

        for (int i = activeEnemiesList.Count - 1; i >= 0; i--)
        {
            EnemyHealth currentEnemy = activeEnemiesList[i];
            if (currentEnemy == null)
            {
                activeEnemiesList.RemoveAt(i);
                continue;
            }

            System.Reflection.FieldInfo enemyHPField = typeof(EnemyHealth).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enemyHPField != null)
            {
                int enemyHP = (int)enemyHPField.GetValue(currentEnemy);
                if (enemyHP <= 0)
                {
                    activeEnemiesList.RemoveAt(i); 
                    OnEnemyTrackedDestroy();       
                }
            }
        }
    }

    private IEnumerator WaitForDeathAnimationSequence(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        TriggerGameOver();
    }

    private void OnEnemyTrackedDestroy()
    {
        currentKills++;
        if (currentKills > clearKillTarget) currentKills = clearKillTarget;
        
        UpdateKillUI();
        AddExperience(1f); 
    }

    // ==========================================
    // 🎨 UI 실시간 갱신 구역
    // ==========================================
    private void UpdateHPUI()
    {
        if (hpCircleImage != null && maxHP > 0) hpCircleImage.fillAmount = currentHP / maxHP;
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
            killCountText.text = $"Kill ( {currentKills:00} / {clearKillTarget:00} )";
            
            if (currentKills >= warningKillTarget)
            {
                killCountText.color = Color.green;
                if (!isWarningTriggered && warningText != null)
                {
                    isWarningTriggered = true;
                    StartCoroutine(AnimateWarningDiagonal());
                }
            }

            if (currentKills >= clearKillTarget)
            {
                TriggerStageClear(); 
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
    // 👑 🎲 랜덤 증강 시스템 및 버프 원격 강제 주입 구역
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
        if (augmentButton2 != null) { augmentButton2.onClick.RemoveAllListeners(); augmentButton2.onClick.AddListener(() => OnSelectAugment(activeAugmentIDs[1])); }
        if (augmentButton3 != null) { augmentButton3.onClick.RemoveAllListeners(); augmentButton3.onClick.AddListener(() => OnSelectAugment(activeAugmentIDs[2])); }
    }

    private void SetAugmentButtonUI(Text targetText, int augmentID)
    {
        if (targetText == null) return;
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
        System.Reflection.FieldInfo hpField = null;
        if (targetPlayerHealth != null)
        {
            hpField = typeof(PlayerHealth).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }

        switch (augmentID)
        {
            case 1: 
                if (targetPlayerHealth != null && hpField != null)
                {
                    int currentEnemyHP = (int)hpField.GetValue(targetPlayerHealth);
                    currentEnemyHP += 10;
                    if (currentEnemyHP > targetPlayerHealth.maxHealth) currentEnemyHP = targetPlayerHealth.maxHealth;
                    hpField.SetValue(targetPlayerHealth, currentEnemyHP); 
                    previousTrackedHP = currentEnemyHP;
                }
                break;

            case 2: 
                if (targetPlayerHealth != null && hpField != null)
                {
                    targetPlayerHealth.maxHealth += 10;
                    int currentEnemyHP = (int)hpField.GetValue(targetPlayerHealth);
                    currentEnemyHP += 10; 
                    hpField.SetValue(targetPlayerHealth, currentEnemyHP);
                    previousTrackedHP = currentEnemyHP;
                }
                break;

            case 3: 
                BonusEXPPerKill += 0.5f; 
                break;

            case 4: 
                SpeedModifier += 0.01f; 
                if (targetPlayerController != null)
                {
                    targetPlayerController.moveSpeed = originalPlayerMoveSpeed * SpeedModifier;
                }
                break;

            case 5: 
                AvoidChance += 0.02f; 
                break;
        }

        if (augmentPanel != null) augmentPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    // ==========================================
    // 🎲 ESC 메뉴 및 순정 로직 구역
    // ==========================================
    private void HandleEscInput()
    {
        if (isGamePaused && quitQuestionText != null && quitQuestionText.activeSelf) { OnClickCancelQuit(); return; }
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
    // 🏆 스테이지 클리어 처리 구역
    // ==========================================
    public void TriggerStageClear()
    {
        if (isGameCleared || isGameOver) return;
        isGameCleared = true;
        Time.timeScale = 0f; 

        if (clearPanel != null)
        {
            clearPanel.SetActive(true); 

            // 🧮 따봉 성적에 맞춘 전용 변수 연산
            int rewardMoney = 100; 
            if (elapsedTime <= 300f && hitCount <= 8) rewardMoney = 300;      // 따봉 3개
            else if (elapsedTime <= 420f && hitCount <= 15) rewardMoney = 200; // 따봉 2개
            else rewardMoney = 100;                                           // 따봉 1개

            // 💰 백그라운드 장부에 누적 합산 및 영구 저장
            int currentMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
            PlayerPrefs.SetInt("PlayerMoney", currentMoney + rewardMoney);
            PlayerPrefs.Save();
            Debug.Log($"🏆 [차등 보상 완료] 따봉 성적 연산으로 ${rewardMoney} 저축! 전재산: ${currentMoney + rewardMoney}");

            // ✍️ [우진님 기획 변경 피드백 100% 반영] 일반 UI.Text 문구 갈아끼우기!
            if (stageMoneyText != null)
            {
                stageMoneyText.text = $"의뢰 보상금 ${rewardMoney}가 입금되었습니다.";
                stageMoneyText.gameObject.SetActive(true);
            }

            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime % 60F);
            
            if (stageTimeText != null) stageTimeText.text = $"Survival Time : ( {minutes:00} : {seconds:00} )";
            if (stageKillText != null) stageKillText.text = $"Kills : ( {currentKills:00} )";

            int currentStageIndex = PlayerPrefs.GetInt("CurrentPlayingStageIndex", 1);
            PlayerPrefs.SetInt($"Stage{currentStageIndex}_Cleared", 1);
            PlayerPrefs.Save();

            StartCoroutine(ClearStageSequence());
            SaveCumulativeData();
        }
    }

    private IEnumerator ClearStageSequence()
    {
        RectTransform panelRect = clearPanel.GetComponent<RectTransform>();
        panelRect.localScale = Vector3.zero; 
        float duration = 0.25f; float timer = 0f;
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
        if (elapsedTime <= 300f && hitCount <= 8) totalThumbs = 3; 
        else if (elapsedTime <= 420f && hitCount <= 15) totalThumbs = 2; 
        else totalThumbs = 1; 

        List<RectTransform> thumbsList = new List<RectTransform> { thumbsUp1, thumbsUp2, thumbsUp3 };
        for (int i = 0; i < thumbsList.Count; i++) if (thumbsList[i] != null) thumbsList[i].gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.2f); 

        for (int i = 0; i < totalThumbs; i++)
        {
            if (thumbsList[i] == null) continue;
            thumbsList[i].gameObject.SetActive(true); 
            thumbsList[i].localScale = new Vector3(4f, 4f, 1f); 
            float stampDuration = 0.15f; float stampTimer = 0f;
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
            
            if (totalSurvivalTimeText != null) totalSurvivalTimeText.text = $"Total Survival Time : ( {minutes:00} : {seconds:00} )";
            if (totalKillText != null) totalKillText.text = $"Total Kill : ( {accumulatedKills:00} )";

            ResetRoguelikeSaveSlot();
        }
    }

    private void SaveCumulativeData()
    {
        float previousTotalTime = PlayerPrefs.GetFloat("TotalSurvivalTime", 0f);
        int previousTotalKills = PlayerPrefs.GetInt("TotalKills", 0);
        PlayerPrefs.SetFloat("TotalSurvivalTime", previousTotalTime + elapsedTime);
        PlayerPrefs.SetInt("TotalKills", previousTotalKills + currentKills);
        PlayerPrefs.Save(); 
    }

    private void ResetRoguelikeSaveSlot()
    {
        PlayerPrefs.SetFloat("TotalSurvivalTime", 0f);
        PlayerPrefs.SetInt("TotalKills", 0);
        PlayerPrefs.SetInt("Slot1_HasSaveData", 0); 
        for (int i = 1; i <= 5; i++) PlayerPrefs.SetInt($"Stage{i}_Cleared", 0);
        PlayerPrefs.Save();
    }

    // 🚨 [우진님 기획 복구] 죽어서 상점 자금 리셋하고 완전히 로비로 탈출하는 진짜 정답 마법 버튼!
    public void OnClickBackToLobby()
    {
        // 🧹 로그라이크 최고 핵심: 죽은 후 나갈 때 가진 전재산을 $0원으로 완벽 초기화시킵니다!
        PlayerPrefs.SetInt("PlayerMoney", 0);
        PlayerPrefs.Save();
        Debug.Log("💀 [로그라이크 초기화 완료] 플레이어가 사망하여 전재산이 무자비하게 $0원으로 리셋되었습니다.");

        Time.timeScale = 1f; 
        LoadingSceneManager.nextSceneName = "MyLobby"; 
        SceneManager.LoadScene("LoadingUI"); 
    }

    private void HandleDebugInputs()
    {
        if (isGameOver || isGameCleared || (augmentPanel != null && augmentPanel.activeSelf)) return; 

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (targetPlayerHealth != null)
            {
                System.Reflection.FieldInfo hpField = typeof(PlayerHealth).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (hpField != null)
                {
                    int currentEnemyHP = (int)hpField.GetValue(targetPlayerHealth);
                    currentEnemyHP -= 20; 
                    hpField.SetValue(targetPlayerHealth, currentEnemyHP);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            OnEnemyTrackedDestroy();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            AddExperience(100f); 
        }
    }
}