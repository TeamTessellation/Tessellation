using Player;
using Stage;
using Unity.Android.Gradle;
using UnityEngine;

public class LimitHandler : MonoBehaviour
{
    public float ScoreIncrease = 1.5f;

    public enum Limit
    {
        //TimeAttack,
        LockTile,
        LockItem,
        ScoreIncrease,
        End
    }

    public static LimitHandler Instance {  get; private set; }
    public Limit CurrentLimit;

    public void Awake()
    {
        Instance = this;
    }

    public string GetLimitText()
    {
        switch (CurrentLimit)
        {
            case Limit.LockTile:
                return "고정 타일";
            case Limit.LockItem:
                return "아이템 잠금";
            case Limit.ScoreIncrease:
                return "점수 증가";
            default:
                return "오류";
        }
    }

    public void UnLimit()
    {
        ClearLimit();
    }

    public void SetLimit()
    {
        int selectLimit = Random.Range(0, (int)Limit.End);
        selectLimit = (int)Limit.LockItem;
        CurrentLimit = (Limit)selectLimit;
        Debug.Log(CurrentLimit);

        switch ((Limit)selectLimit)
        {
            case Limit.LockTile:
                Limit_LockTile();
                break;
            case Limit.LockItem:
                Limit_LockItem();
                break;
            case Limit.ScoreIncrease:
                Limit_ScoreIncrease();
                break;
            case Limit.End:
                break;
        }
    }

    private void ClearLimit()
    {
        Field.Instance.UnLockAllCell();
        InputManager.Instance.UnLockItem();
    }

    private void Limit_LockTile()
    {
        Field.Instance.LockCell(4);
    }

    private void Limit_LockItem()
    {
        InputManager.Instance.LockItem();
    }

    private void Limit_ScoreIncrease()
    {
        StageManager.Instance.CurrentStage.StageTargetScore = (int)(StageManager.Instance.CurrentStage.StageTargetScore * 1.5f);
    }

    private void Limit_LockOption()
    {

    }
}
