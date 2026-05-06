using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 필수

public class IntroController : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenuScenes"; // 씬 이름 확인 필수!
    public float holdDuration = 2.0f;

    private float timer = 0f;
    private bool isLoading = false;

    // UI 연결을 위한 변수
    public Image progressBar;
    public GameObject gaugeParent; // 평소에 숨겨두고 싶다면 사용

    void Start()
    {
        if (progressBar != null) progressBar.fillAmount = 0;
        if (gaugeParent != null) gaugeParent.SetActive(false); // 처음엔 숨김
    }

    void Update()
    {
        if (isLoading) return;

        if (Input.GetKey(KeyCode.Space))
        {
            if (gaugeParent != null) gaugeParent.SetActive(true); // 누를 때만 표시

            timer += Time.deltaTime;

            // 게이지 업데이트: (현재 시간 / 목표 시간) 비율로 fillAmount 설정
            if (progressBar != null)
            {
                progressBar.fillAmount = timer / holdDuration;
            }

            if (timer >= holdDuration)
            {
                StartNextScene();
            }
        }
        else
        {
            // 키를 떼면 초기화
            timer = 0f;
            if (progressBar != null) progressBar.fillAmount = 0;
            if (gaugeParent != null) gaugeParent.SetActive(false); // 떼면 다시 숨김
        }
    }

    void StartNextScene()
    {
        isLoading = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}