using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [Header("몬스터 스탯")]
    public int stage = 1; // [추가] 현재 스테이지 번호
    public int maxHp = 50;
    public int currentHp;
    public int attackDamage = 5; // 매 턴 몬스터가 가할 고정 데미지

    [Header("UI 연결 (TextMeshPro)")]
    public TextMeshProUGUI stageText; // [추가] 스테이지 표시용 텍스트
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;

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
            // [바로 이 부분 추가!] 보상 팝업창 호출
            RewardManager.Instance.ShowRewardPopup();
        }
    }

    public void UpdateUI()
    {
        // [수정] 텍스트에 몇 스테이지인지도 보여주면 좋습니다!
        if (stageText != null) stageText.text = $"STAGE {stage}"; // 전용 UI에 스테이지 표시
        if (hpText != null) hpText.text = $"monster HP : {currentHp} / {maxHp}";
        if (attackText != null) attackText.text = $"Attack : {attackDamage}";
    }

    // [추가할 함수] 몬스터 레벨업 및 부활!
    public void SetupNextStage()
    {
        stage++;
        maxHp += 20; // 다음 스테이지마다 체력이 20씩 더 많아집니다.
        attackDamage += 3; // 공격력도 3씩 셉니다.
        currentHp = maxHp; // 체력 꽉 채워서 부활

        UpdateUI();
        Debug.Log($"스테이지 {stage} 시작! 몬스터가 더 강력해졌습니다.");
    }

}