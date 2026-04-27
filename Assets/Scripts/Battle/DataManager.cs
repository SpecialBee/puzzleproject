using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 꼭 필요합니다!

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [Header("로그라이크 진행 데이터")]
    public int currentStage = 1;

    [Header("플레이어 스탯 (유지용)")]
    public int maxHp = 100;
    public int currentHp = 100;
    public int baseAttack = 0; // 보상으로 얻은 영구 공격력
    public int currentMana = 0;

    void Awake()
    {
        // 싱글톤 패턴 & 파괴 방지 (가장 중요한 핵심 로직!)
        if (Instance == null)
        {
            Instance = this;
            // 🚨 씬이 바뀌어도 이 오브젝트를 파괴하지 말라는 유니티 전용 명령어입니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 DataManager가 존재한다면 새로 생성된 짭(?)은 파괴합니다.
            Destroy(gameObject);
        }
    }

    // 1. 새로운 게임(런)을 시작할 때 초기화하는 함수
    public void ResetDataForNewRun()
    {
        currentStage = 1;
        maxHp = 100;
        currentHp = maxHp;
        baseAttack = 0;
        currentMana = 0;
        Debug.Log("🔄 데이터 초기화 완료: 새로운 모험을 시작합니다.");
    }

    // 2. 전투 씬으로 진입할 때 (스테이지 선택 맵에서 호출)
    public void LoadBattleScene(int stageNumber)
    {
        currentStage = stageNumber;
        SceneManager.LoadScene("BattleScene"); // 실제 전투 씬 이름으로 이동
    }

    // 3. 스테이지 선택(맵) 씬으로 귀환할 때 (전투 승리 시 호출)
    public void LoadStageSelectScene()
    {
        SceneManager.LoadScene("StageSelect");
    }
}