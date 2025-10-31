using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public TileSetGroupSO GroupSO;

    public TileSetData[] GetRadomTileSetDataInGroup(int dataCount = 1)
    {
        TileSetData[] result = new TileSetData[dataCount];
        var group = GroupSO.Group;
        List<TileSetData> list = group.Select(x => x.TileSet).ToList();

        if (list.Count <= dataCount)
            return list.ToArray();

        Dictionary<int, TileSetData> groupDic = new();

        int count = 0;
        for (int i = 0; i < group.Count; i++)
        {
            for (int j = 0; j < group[i].Count; j++)
                { groupDic[count] = group[i].TileSet; count++; }
        }
        List<int> targetIndexs = new();
        var targetList = groupDic.Keys.ToList();

        while (targetIndexs.Count < dataCount)
        {
            int randomNum = UnityEngine.Random.Range(0, targetList.Count);
            int target = targetList[randomNum];
            targetList.RemoveAt(randomNum);
            targetIndexs.Add(target);
        }
        result = targetIndexs.Select(x => groupDic[x]).ToArray();

        return result;
    }
}
