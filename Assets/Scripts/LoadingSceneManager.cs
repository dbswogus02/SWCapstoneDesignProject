using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("로딩 UI 요소들 (D90 슬라이더 연동)")]
    [SerializeField] private Slider loadingSlider;         // D90 Slider 본체 지정
    [SerializeField] private TextMeshProUGUI loadingText;   // 퍼센트 및 상태 표시 텍스트

    // ⭐ 핵심: 이 static 변수에 가고자 하는 '목표 씬 이름'을 넣어주고 이 씬을 켜면 됩니다!
    public static string nextSceneName;

    void Start()
    {
        // 씬이 시작되면 슬라이더를 0으로 초기화합니다.
        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
        }

        // 자동으로 비동기 로딩 코루틴 시작
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // 💡 예외 처리: 만약 실수로 nextSceneName을 안 적고 로딩창을 켰다면 
        // 에러 방지를 위해 기본적으로 로비나 미션UI로 가게 안전장치를 둡니다.
        if (string.IsNullOrEmpty(nextSceneName))
        {
            nextSceneName = "MissionUI"; // 우진님의 메인 미션 UI 씬 이름으로 적어두세요!
        }

        // 백그라운드에서 다음 씬을 비동기로 로드하기 시작합니다.
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        
        // 로딩이 100% 다 완료되어도 바로 화면이 넘어가지 않도록 붙잡아둡니다.
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            // 유니티 자체의 실제 로딩은 90%(0.9f)에서 완료되어 멈춥니다.
            if (op.progress < 0.9f)
            {
                // 실제 로딩 속도에 맞춰서 슬라이더(캐릭터 위치)를 부드럽게 이동
                if (loadingSlider != null) 
                    loadingSlider.value = Mathf.Lerp(loadingSlider.value, op.progress, timer);

                if (loadingText != null) 
                    loadingText.text = $"Loading . . . {(int)(loadingSlider.value * 100)}%";
            }
            // 유니티 로딩이 90%를 넘어서 끝나면, 나머지 10%를 가짜로 채우며 연출합니다.
            else
            {
                if (loadingSlider != null) 
                    loadingSlider.value = Mathf.Lerp(loadingSlider.value, 1f, timer);

                if (loadingText != null) 
                    loadingText.text = $"Loading . . . {(int)(loadingSlider.value * 100)}%";

                // 슬라이더와 달리는 캐릭터가 끝(100% = 1.0f)에 도달했다면
                if (loadingSlider != null && loadingSlider.value >= 1f)
                {
                    // 잡아두었던 씬을 활성화하여 인게임 혹은 목표 화면으로 전환합니다!
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}