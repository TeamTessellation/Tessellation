using System;

public readonly struct TileScoreRule
{
    public bool AwardsScore { get; }
    public ScoreManager.ScoreValueType ScoreType { get; }
    public bool AwardsCoin { get; }
    public bool AppliesDoubleMultiplier { get; }

    public TileScoreRule(
        bool awardsScore,
        ScoreManager.ScoreValueType scoreType,
        bool awardsCoin = false,
        bool appliesDoubleMultiplier = false)
    {
        AwardsScore = awardsScore;
        ScoreType = scoreType;
        AwardsCoin = awardsCoin;
        AppliesDoubleMultiplier = appliesDoubleMultiplier;
    }
}

public static class TileScoreRules
{
    public static float CalculateLinePhaseMultiplier(
        int lineCount,
        int doubleTileCount,
        float comboStep,
        float doubleTileMultiplier)
    {
        float multiplier = 1f + Math.Max(0, lineCount - 1) * comboStep;
        for (int i = 0; i < Math.Max(0, doubleTileCount); i++)
            multiplier *= doubleTileMultiplier;

        return multiplier;
    }

    public static TileScoreRule Get(eTileEventType eventType, TileOption option)
    {
        if (eventType == eTileEventType.Place)
            return new TileScoreRule(true, ScoreManager.ScoreValueType.BasePlaceScore);

        if (eventType == eTileEventType.LineClear)
        {
            return option switch
            {
                TileOption.Boom => new TileScoreRule(false, default),
                TileOption.Default => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseLineClearScore),
                TileOption.Gold => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseBonusScore, awardsCoin: true),
                TileOption.Double => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseBonusScore,
                    appliesDoubleMultiplier: true),
                _ => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseBonusScore),
            };
        }

        if (eventType == eTileEventType.Burst)
        {
            return option switch
            {
                TileOption.Boom => new TileScoreRule(false, default),
                TileOption.Bonus => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseBonusScore),
                TileOption.Gold => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseBurstScore, awardsCoin: true),
                _ => new TileScoreRule(true, ScoreManager.ScoreValueType.BaseBurstScore),
            };
        }

        return new TileScoreRule(false, default);
    }
}
