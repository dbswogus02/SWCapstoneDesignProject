using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class StoreManager : MonoBehaviour
{
    [Header("=== 상점 보유 자금 ===")]
    [SerializeField] private Text myMoneyText; 

    [Header("=== 상점 탈출 버튼 ===")]
    [SerializeField] private Button backButton;

    [Header("=== 1. 야구배트 (Bat) ===")]
    [SerializeField] private Text batLevelText;       
    [SerializeField] private Button batUpgradeButton;  
    private int batCost = 50;                         

    [Header("=== 2. 샷건 (Shotgun) ===")]
    [SerializeField] private Text shotgunLevelText;   
    [SerializeField] private Button shotgunUpgradeButton; 
    private int shotgunCost = 150;                     

    [Header("=== 3. 전기톱 (Chainsaw) ===")]
    [SerializeField] private Text chainsawLevelText;  
    [SerializeField] private Button chainsawUpgradeButton; 
    private int chainsawCost = 100;                    

    private int currentMoney = 0;
    private int batLv = 0;
    private int shotgunLv = 0;
    private int chainsawLv = 0;

    void OnEnable()
    {
        LoadStoreData();
        UpdateStoreUI();

        if (backButton != null) { backButton.onClick.RemoveAllListeners(); backButton.onClick.AddListener(OnClickBackToMissionUI); }
        if (batUpgradeButton != null) { batUpgradeButton.onClick.RemoveAllListeners(); batUpgradeButton.onClick.AddListener(UpgradeBat); }
        if (shotgunUpgradeButton != null) { shotgunUpgradeButton.onClick.RemoveAllListeners(); shotgunUpgradeButton.onClick.AddListener(UpgradeShotgun); }
        if (chainsawUpgradeButton != null) { chainsawUpgradeButton.onClick.RemoveAllListeners(); chainsawUpgradeButton.onClick.AddListener(UpgradeChainsaw); }
    }

    private void LoadStoreData()
    {
        currentMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
        batLv = PlayerPrefs.GetInt("Bat_Lv", 0);
        shotgunLv = PlayerPrefs.GetInt("Shotgun_Lv", 0);
        chainsawLv = PlayerPrefs.GetInt("Chainsaw_Lv", 0);
    }

    private void UpdateStoreUI()
    {
        if (myMoneyText != null) myMoneyText.text = $"현재 잔액 $ {currentMoney} 있습니다.";

        // ✨ 우진님 피드백 반영: 깔끔하게 단축된 포맷
        if (batLevelText != null) batLevelText.text = $"강화 단계: Lv {batLv:D2}";
        if (shotgunLevelText != null) shotgunLevelText.text = $"강화 단계: Lv {shotgunLv:D2}";
        if (chainsawLevelText != null) chainsawLevelText.text = $"강화 단계: Lv {chainsawLv:D2}";
    }

    public void OnClickBackToMissionUI()
    {
        Time.timeScale = 1f;
        LoadingSceneManager.nextSceneName = "MissionUI"; 
        SceneManager.LoadScene("LoadingUI"); 
    }
    
    public void UpgradeBat()
    {
        if (currentMoney >= batCost)
        {
            currentMoney -= batCost; 
            batLv++;                

            PlayerPrefs.SetInt("PlayerMoney", currentMoney);
            PlayerPrefs.SetInt("Bat_Lv", batLv);
            PlayerPrefs.Save();

            UpdateStoreUI(); 
            Debug.Log($"🏏 야구배트 강화 성공! 현재 레벨: {batLv}");
        }
        else
        {
            Debug.Log("❌ [잔액 부족] 의뢰 보상금이 부족합니다!");
        }
    }

    public void UpgradeShotgun()
    {
        if (currentMoney >= shotgunCost)
        {
            currentMoney -= shotgunCost;
            shotgunLv++;

            PlayerPrefs.SetInt("PlayerMoney", currentMoney);
            PlayerPrefs.SetInt("Shotgun_Lv", shotgunLv);
            PlayerPrefs.Save();

            UpdateStoreUI();
            Debug.Log($"💥 샷건 강화 성공! 현재 레벨: {shotgunLv}");
        }
        else
        {
            Debug.Log("❌ [잔액 부족] 의뢰 보상금이 부족합니다!");
        }
    }

    public void UpgradeChainsaw()
    {
        if (currentMoney >= chainsawCost)
        {
            currentMoney -= chainsawCost;
            chainsawLv++;

            PlayerPrefs.SetInt("PlayerMoney", currentMoney);
            PlayerPrefs.SetInt("Chainsaw_Lv", chainsawLv);
            PlayerPrefs.Save();

            UpdateStoreUI();
            Debug.Log($"🪚 전기톱 강화 성공! 현재 레벨: {chainsawLv}");
        }
        else
        {
            Debug.Log("❌ [잔액 부족] 의뢰 보상금이 부족합니다!");
        }
    }
}