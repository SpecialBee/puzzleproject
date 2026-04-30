using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [Header("로그라이크 진행 데이터")]
    public int currentStage = 1;

    [Header("플레이어 스탯 (유지용)")]
    public int maxHp = 100;
    public int currentHp = 100;
    public int baseAttack = 0;   // 보상으로 얻은 영구 공격력
    public int baseDefense = 0;  // [추가 - B-04/①] 보상으로 얻은 영구 방어력
    public int currentMana = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1. 새로운 게임(런)을 시작할 때 초기화
    public void ResetDataForNewRun()
    {
        currentStage = 1;
        maxHp = 100;
        currentHp = maxHp;
        baseAttack = 0;
        baseDefense = 0; // [추가 - ①] 영구 방어력도 함께 초기화
        currentMana = 0;
        Debug.Log("🔄 데이터 초기화 완료: 새로운 모험을 시작합니다.");
    }

    // 2. 전투 씬으로 진입할 때 (스테이지 선택 맵에서 호출)
    //    stageNumber를 직접 받아 currentStage에 저장합니다.
    public void LoadBattleScene(int stageNumber)
    {
        currentStage = stageNumber;
        SceneManager.LoadScene("BattleScene");
    }

    // 3. 전투 승리 후 맵 씬으로 귀환할 때 호출
    //    [수정 - B-01] 이 시점에 currentStage를 +1 해줍니다.
    //    (스테이지 선택 씬이 완성되면 LoadBattleScene에 직접 올바른 값을 넘기는 방식으로 교체 가능)
    public void LoadStageSelectScene()
    {
        currentStage++; // [B-01] 클리어한 스테이지를 넘어 다음 층으로 진행
        Debug.Log($"✅ 스테이지 {currentStage - 1} 클리어! 다음 목표: {currentStage}층");
        SceneManager.LoadScene("StageSelect");
    }
}