using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [Header("몬스터 스탯")]
    public int maxHp = 50;
    public int currentHp;
    public int attackDamage = 5; // 매 턴 몬스터가 가할 고정 데미지

    [Header("UI 연결 (TextMeshPro)")]
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
        if (currentHp < 0) currentHp = 0; // 체력이 마이너스가 되지 않게 방지

        UpdateUI();
        Debug.Log($"[전투] 몬스터가 {damage}의 데미지를 입었습니다! 남은 체력: {currentHp}");

        if (currentHp == 0)
        {
            Debug.Log("🎉 몬스터 처치 성공! (승리 로직 발동 예정)");
        }
    }

    public void UpdateUI()
    {
        if (hpText != null) hpText.text = $"Monster HP : {currentHp} / {maxHp}";
        if (attackText != null) attackText.text = $"Monster Attack : {attackDamage}";
    }
}