using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // [수정 - S-01] 싱글톤 중복 체크 추가
    public static SkillManager Instance;

    [Header("타겟팅 상태")]
    public SkillData currentTargetingSkill = null;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    public void CastSkill(SkillData skill)
    {
        if (PlayerManager.Instance.mana < skill.manaCost)
        {
            Debug.LogWarning("💧 마나가 부족합니다!");
            return;
        }

        if (skill.requiresTarget)
        {
            currentTargetingSkill = skill;
            Debug.Log($"🎯 [{skill.skillName}] 발동 대기! 타겟을 클릭하세요.");
        }
        else
        {
            if (PlayerManager.Instance.UseMana(skill.manaCost))
                ExecuteSkill(skill);
        }
    }

    public void ExecuteSkill(SkillData skill, TileSlot targetSlot = null)
    {
        switch (skill.effect)
        {
            case SkillData.SkillEffect.RerollHand:
                HandManager.Instance.DrawTiles(5);
                break;

            case SkillData.SkillEffect.DestroyTarget:
                if (targetSlot != null && targetSlot.transform.childCount > 0)
                {
                    Destroy(targetSlot.transform.GetChild(0).gameObject);
                    GridManager.Instance.boardData[targetSlot.gridX, targetSlot.gridY] = null;
                    targetSlot.SetLock(0);
                }
                break;

            // [수정 - B-05/⑥] 미구현 스킬: warning 출력으로 silent fail 방지
            case SkillData.SkillEffect.MakeJoker:
            case SkillData.SkillEffect.ManaShield:
            case SkillData.SkillEffect.Heal:
            case SkillData.SkillEffect.DirectDamage:
            case SkillData.SkillEffect.SkipMonsterTurn:
            case SkillData.SkillEffect.CleanseBoard:
                Debug.LogWarning($"⚠️ [{skill.skillName}] 스킬 효과({skill.effect})는 아직 구현되지 않았습니다. 마나를 소모하지 않습니다.");
                // 마나 환불: UseMana가 이미 호출된 경우를 대비해 되돌려 줍니다.
                PlayerManager.Instance.mana += skill.manaCost;
                PlayerManager.Instance.UpdateUI();
                currentTargetingSkill = null;
                return; // 로그만 남기고 종료 (스킬 발동 성공 메시지 출력 안 함)

            default:
                Debug.LogWarning($"⚠️ [{skill.skillName}] 알 수 없는 스킬 효과: {skill.effect}");
                currentTargetingSkill = null;
                return;
        }

        Debug.Log($"🌟 스킬 발동 성공! [{skill.skillName}]");
        currentTargetingSkill = null;
    }
}