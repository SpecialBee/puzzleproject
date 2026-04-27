using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [Header("몬스터 기본 스탯 (1스테이지 기준)")]
    public int baseMaxHp = 50;
    public int baseAttack = 5;
    public int baseGimmickChance = 30;

    [Header("현재 스테이지 적용 스탯")]
    public int stage = 1;
    public int maxHp;
    public int currentHp;
    public int attackDamage;

    [Header("UI 연결 (TextMeshPro)")]
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;

    [Header("몬스터 기믹(스킬) 세팅")]
    [Tooltip("이 몬스터가 사용할 수 있는 스킬(GimmickData)들을 넣어주세요.")]
    public List<GimmickData> availableGimmicks;

    [Tooltip("현재 적용된 기믹 발동 확률 (자동 계산됨)")]
    [Range(0, 100)] public int gimmickChance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. DataManager에서 현재 층수를 가져옵니다. (없으면 테스트용 1층)
        if (DataManager.Instance != null)
        {
            stage = DataManager.Instance.currentStage;
        }
        else
        {
            stage = 1;
        }

        // 2. 층수에 비례한 곱연산 스케일링 (난이도 뻥튀기)
        float multiplier = 1f + ((stage - 1) * 0.2f); // 1층당 체력/공격력 20% 상승

        maxHp = Mathf.RoundToInt(baseMaxHp * multiplier);
        currentHp = maxHp;
        attackDamage = Mathf.RoundToInt(baseAttack * multiplier);

        // 3. 기믹 확률은 1층당 10%씩 독해지며, 최대 80%로 제한
        gimmickChance = baseGimmickChance + ((stage - 1) * 10);
        if (gimmickChance > 80) gimmickChance = 80;

        UpdateUI();
        Debug.Log($"👾 {stage}층 몬스터 등장! (체력: {maxHp}, 공격력: {attackDamage}, 기믹확률: {gimmickChance}%)");
    }

    // 플레이어에게 두들겨 맞을 때 불려갈 함수
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;

        UpdateUI();
        Debug.Log($"[전투] 몬스터가 {damage}의 데미지를 입었습니다! 남은 체력: {currentHp}");

        if (currentHp == 0)
        {
            Debug.Log("🎉 몬스터 처치 성공! RewardManager를 호출합니다.");

            // [수정됨] PlayerManager의 패널을 직접 켜는 대신, 원래대로 RewardManager를 부릅니다.
            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.ShowRewardPopup();
            }
        }
    }

    public void UpdateUI()
    {
        if (stageText != null) stageText.text = $"STAGE {stage}";
        if (hpText != null) hpText.text = $"monster HP : {currentHp} / {maxHp}";
        if (attackText != null) attackText.text = $"Attack : {attackDamage}";
    }

    // ---------------------------------------------------
    // 몬스터 공격 및 기믹 사용 로직
    // ---------------------------------------------------
    public void PerformMonsterTurn()
    {
        // 1. 기본 공격 수행
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.TakeDamage(attackDamage);
        }

        // 2. 주사위를 굴려 기믹 발동 여부 결정
        if (availableGimmicks != null && availableGimmicks.Count > 0)
        {
            int diceRoll = Random.Range(0, 100); // 0 ~ 99
            if (diceRoll < gimmickChance)
            {
                // 기믹 목록 중 하나를 무작위로 골라 실행!
                int randomGimmickIndex = Random.Range(0, availableGimmicks.Count);
                availableGimmicks[randomGimmickIndex].Execute();
            }
        }
    }
}