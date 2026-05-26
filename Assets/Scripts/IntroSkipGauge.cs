using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSkipGauge : MonoBehaviour
{
    [Header("씬 설정")]
    [SerializeField] private string nextSceneName = "MyLobby"; 

    [Header("시간 설정")]
    [SerializeField] private float maxHoldTime = 2f;           

    [Header("UI 연결")]
    [SerializeField] private Image circleGaugeImage;           
    [SerializeField] private GameObject loadingText;           

    private float currentHoldTime = 0f;                        

    void Start()
    {
        if (circleGaugeImage != null) circleGaugeImage.fillAmount = 0f;
        if (loadingText != null) loadingText.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (loadingText != null) loadingText.SetActive(true);

            currentHoldTime += Time.deltaTime;

            if (circleGaugeImage != null)
            {
                circleGaugeImage.fillAmount = currentHoldTime / maxHoldTime;
            }

            if (currentHoldTime >= maxHoldTime)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            currentHoldTime = 0f;

            if (circleGaugeImage != null) circleGaugeImage.fillAmount = 0f;
            if (loadingText != null) loadingText.SetActive(false); 
        }
    }
}