using UnityEngine;
using UnityEngine.SceneManagement;

public class StageNode : MonoBehaviour
{
    [Header("이동할 씬 이름 세팅")]
    [Tooltip("여기에 이동하고 싶은 씬의 이름을 정확히 적어주세요.")]
    public string targetSceneName;

    [Header("로그라이크 데이터 연동 (선택사항)")]
    [Tooltip("이 버튼이 메인 메뉴의 '새 게임 시작' 버튼이라면 체크하세요.")]
    public bool isNewGameNode = false;

    [Tooltip("이 버튼이 맵의 '전투 노드'라면, 이 노드의 스테이지 레벨(층수)을 적어주세요.")]
    public int stageLevel = 1;

    // 버튼을 클릭했을 때 실행될 함수
    public void OnClickNode()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("⚠️ 목적지 씬 이름이 인스펙터에 비어있습니다! 이름을 적어주세요.");
            return;
        }

        // ---------------------------------------------------
        // [모듈의 핵심] 씬을 넘기기 전에 DataManager와 연동합니다!
        // ---------------------------------------------------
        if (DataManager.Instance != null)
        {
            if (isNewGameNode)
            {
                // 메인 메뉴에서 맵으로 갈 때: 이전 기록을 싹 지우고 새 게임 준비
                DataManager.Instance.ResetDataForNewRun();
            }
            else
            {
                // 맵에서 전투 등 특정 스테이지로 갈 때: 현재 진입하는 층수 저장
                DataManager.Instance.currentStage = stageLevel;
            }
        }

        Debug.Log($"🗺️ 모듈 작동: [{targetSceneName}] 씬으로 이동합니다! (스테이지: {stageLevel})");
        SceneManager.LoadScene(targetSceneName);
    }
}