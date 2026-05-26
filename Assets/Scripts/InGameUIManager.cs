using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    [Header("=== 1. HP UI (상단 좌측) ===")]
    [SerializeField] private Image hpCircleImage;       // 원형(마스크 안 진짜 피) 이미지 컴포넌트
    [SerializeField] private TextMeshProUGUI hpText;    // HP 텍스트

    [Header("=== 2. 타이머 UI (상단 중앙) ===")]
    [SerializeField] private TextMeshProUGUI timerText; // 타이머 텍스트 (예: 00:00)

    [Header("=== 3. 몬스터 처치수 UI (상단 우측) ===")]
    [SerializeField] private TextMeshProUGUI killCountText; // 킬 카운트 텍스트 (예: Kill ( 00 / 120 ))
    // 🔔 우진님의 보스 연출 기획: 대각선으로 무빙하며 깜빡일 WARNING 텍스트 오브젝트!
    [SerializeField] private TextMeshProUGUI warningText;   

    [Header("=== 4. 경험치 UI (하단) ===")]
    [SerializeField] private Image expFillImage;        // 가로로 차오르는 진짜 게이지 Image 사용!

    [Header("=== 5. 레벨업 증강 팝업 패널 ===")]
    [SerializeField] private GameObject augmentPanel;   // 레벨업 시 켜질 패널
    [SerializeField] private TextMeshProUGUI cardText1; // 1번 카드 글자
    [SerializeField] private TextMeshProUGUI cardText2; // 2번 카드 글자
    [SerializeField] private TextMeshProUGUI cardText3; // 3번 카드 글자

    // === 가상 인게임 데이터 (추후 조원들 스크립트와 동기화할 변수들) ===
    private float currentHP = 100f;
    private float maxHP = 100f;
    
    private float currentEXP = 0f;
    private float maxEXP = 100f; 
    
    private float elapsedTime = 0f; 
    
    private int currentKills = 0;
    private int targetKills = 120; // 🎯 목표 몬스터 수 120마리

    // 증강 무작위 풀
    private List<int> augmentPool = new List<int> { 1, 2, 3, 4, 5, 6 };
    private List<int> selectedAugments = new List<int>();

    // 보스 경고창 연출 상태 체크용
    private bool isWarningTriggered = false;

    void Start()
    {
        // 시작할 때 UI 상태 초기화
        if (augmentPanel != null) augmentPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);

        UpdateHPUI();
        UpdateKillUI();
        UpdateEXPUI();
    }

    void Update()
    {
        // 타이머 실시간 계산 및 UI 반영
        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        // 🛠️ 개발자용 치트키 테스트 입력
        HandleDebugInputs();
    }

    // ==========================================
    // 🎨 UI 실시간 갱신 비서 함수들
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
            
            // 대망의 120마리 달성 시!
            if (currentKills >= targetKills)
            {
                killCountText.color = Color.green;

                // 🔔 한 번만 대각선 깜빡이 경고 연출 코루틴을 실행해라!
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


    // ==========================================
    // 🐉 우진님의 킬러 연출: WARNING 대각선 깜빡이 무빙 로직
    // ==========================================
    private IEnumerator AnimateWarningDiagonal()
    {
        warningText.gameObject.SetActive(true);
        RectTransform rectTransform = warningText.GetComponent<RectTransform>();

        // 1. 화면 해상도 기준으로 대각선 무빙의 시작점(왼쪽 위 화면 밖)과 끝점(오른쪽 아래 화면 밖) 계산
        Vector3 startPos = new Vector3(-Screen.width * 0.6f, Screen.height * 0.6f, 0);
        Vector3 endPos = new Vector3(Screen.width * 0.6f, -Screen.height * 0.6f, 0);

        float duration = 2f;   // 전체 연출 시간 (2초 동안 지나감)
        float timer = 0f;
        float blinkInterval = 0.4f; // 깜빡거리는 주기 (0.4초마다 켜졌다 꺼졌다 함)
        float nextBlinkTime = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 2. 부드럽게 왼쪽 위에서 오른쪽 아래로 위치 이동 (Lerp 선형 보간)
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, progress);

            // 3. 켜졌다 꺼졌다 깜빡이는 타이밍 제어
            if (timer >= nextBlinkTime)
            {
                warningText.enabled = !warningText.enabled; // 글자 가시성을 반대로 꼬기 (On/Off)
                nextBlinkTime += blinkInterval;
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 4. 연출이 다 끝나면 완전히 끄고 보스 등장 신호 주기
        warningText.gameObject.SetActive(false);
        Debug.Log("🐉 [보스 출현] WARNING 연출 종료! 보스가 등장합니다!");
        // (조원들이 여기에 보스 인스턴스 소환 함수를 연동하면 끝납니다!)
    }


    // ==========================================
    // 🎰 레벨업 & 증강 시스템 로직
    // ==========================================
    public void GainExperience(float amount)
    {
        currentEXP += amount;
        if (currentEXP >= maxEXP)
        {
            currentEXP -= maxEXP;
            UpdateEXPUI();
            TriggerLevelUp();
        }
        else
        {
            UpdateEXPUI();
        }
    }

    private void TriggerLevelUp()
    {
        Time.timeScale = 0f; // 게임 일시정지
        if (augmentPanel != null) augmentPanel.SetActive(true);

        selectedAugments.Clear();
        List<int> tempPool = new List<int>(augmentPool);

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            selectedAugments.Add(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex);
        }

        SetCardText(cardText1, selectedAugments[0]);
        SetCardText(cardText2, selectedAugments[1]);
        SetCardText(cardText3, selectedAugments[2]);
    }

    private void SetCardText(TextMeshProUGUI targetText, int augmentID)
    {
        if (targetText == null) return;

        switch (augmentID)
        {
            case 1: targetText.text = "<b>[응급 처치]</b>\n즉시 HP 10을 회복합니다."; break;
            case 2: targetText.text = "<b>[방호복 보강]</b>\n최대 HP 총량이 10 늘어납니다."; break;
            case 3: targetText.text = "<b>[고성능 청소기]</b>\n경험치 습득 범위가 2% 증가합니다."; break;
            case 4: targetText.text = "<b>[야간 수당]</b>\n몬스터 처치 시 경험치 획득량이 2% 증가합니다."; break;
            case 5: targetText.text = "<b>[기능성 안전화]</b>\n플레이어의 이동 속도가 2% 증가합니다."; break;
            case 6: targetText.text = "<b>[방역 패드]</b>\n피격 시 2%의 확률로 데미지를 무효화합니다."; break;
        }
    }

    public void OnClickCard1() { ApplyAugment(selectedAugments[0]); }
    public void OnClickCard2() { ApplyAugment(selectedAugments[1]); }
    public void OnClickCard3() { ApplyAugment(selectedAugments[2]); }

    private void ApplyAugment(int augmentID)
    {
        Debug.Log($"증강 {augmentID}번 적용 완료!");

        if (augmentID == 1)
        {
            currentHP += 10f;
            if (currentHP > maxHP) currentHP = maxHP;
            UpdateHPUI();
        }
        else if (augmentID == 2)
        {
            float oldHP = currentHP;
            maxHP += 10f; 
            currentHP = oldHP; 
            UpdateHPUI();
        }

        if (augmentPanel != null) augmentPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    // ==========================================
    // 🕹️ 개발자 테스트용 키 입력 (치트키)
    // ==========================================
    private void HandleDebugInputs()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            currentHP -= 15f;
            if (currentHP < 0) currentHP = 0;
            UpdateHPUI();
        }

        // [K] 연타해서 120마리를 채우는 순간 대각선 경고창이 발동합니다!
        if (Input.GetKeyDown(KeyCode.K))
        {
            currentKills++;
            if (currentKills > targetKills) currentKills = targetKills;
            UpdateKillUI();
            Debug.Log($"💀 [테스트] 좀비 처치! ({currentKills}/{targetKills})");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GainExperience(25f);
        }
    }
}