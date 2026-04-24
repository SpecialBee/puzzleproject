using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [Header("몬스터 스탯")]
    public int stage = 1;
    public int maxHp = 50;
    public int currentHp;
    public int attackDamage = 5;

    [Header("UI 연결 (TextMeshPro)")]
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;

    // ---------------------------------------------------
    // [추가된 부분] 몬스터 기믹(스킬) 세팅
    // ---------------------------------------------------
    [Header("몬스터 기믹(스킬) 세팅")]
    [Tooltip("이 몬스터가 사용할 수 있는 스킬(GimmickData)들을 넣어주세요.")]
    public List<GimmickData> availableGimmicks;

    [Tooltip("매 턴마다 기믹이 발동할 확률 (0 ~ 100%)")]
    [Range(0, 100)] public int gimmickChance = 30;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHp = maxHp;
        UpdateUI();
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
            Debug.Log("🎉 몬스터 처치 성공! 보상 창을 띄웁니다.");
            RewardManager.Instance.ShowRewardPopup();
        }
    }

    public void UpdateUI()
    {
        if (stageText != null) stageText.text = $"STAGE {stage}";
        if (hpText != null) hpText.text = $"monster HP : {currentHp} / {maxHp}";
        if (attackText != null) attackText.text = $"Attack : {attackDamage}";
    }

    // 몬스터 레벨업 및 부활!
    public void SetupNextStage()
    {
        stage++;
        maxHp += 20; // 다음 스테이지마다 체력이 20씩 더 많아집니다.
        attackDamage += 3; // 공격력도 3씩 셉니다.

        // [추가] 스테이지가 올라갈수록 기믹 발동 확률도 10%씩 독해집니다!
        gimmickChance += 10;
        if (gimmickChance > 80) gimmickChance = 80; // 하지만 최대 80%까지만 제한

        currentHp = maxHp; // 체력 꽉 채워서 부활

        UpdateUI();
        Debug.Log($"스테이지 {stage} 시작! 몬스터가 더 강력해졌습니다.");
    }

    // ---------------------------------------------------
    // [추가된 부분] 몬스터 공격 및 기믹 사용 로직
    // ---------------------------------------------------
    public void PerformMonsterTurn()
    {
        // 1. 기본 공격 수행 (기존에는 HandManager에서 하던 일)
        PlayerManager.Instance.TakeDamage(attackDamage);

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