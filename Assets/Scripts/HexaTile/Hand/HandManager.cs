using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public DeckSO DeckSO;
    public GameObject HandBox;


    private void Start()
    {
        SetHand(3);
    }

    private void 

    public void SetHand(int handSize)
    {
        var tileSetDatas = GetRadomTileSetDataInGroup(handSize);
        for (int i = 0; i < tileSetDatas.Length; i++)
        {
            Pool<TileSet, TileSetData>.Get(tileSetDatas[i]);
        }
    }

    private TileSetData[] GetRadomTileSetDataInGroup(int dataCount = 1)
    {
        TileSetData[] result = new TileSetData[dataCount];
        var deck = DeckSO.Deck;
        List<TileSetData> list = deck.Select(x => x.TileSet).ToList();

        if (list.Count <= dataCount)
            return list.ToArray();

        Dictionary<int, TileSetData> groupDic = new();

        int count = 0;
        for (int i = 0; i < deck.Count; i++)
        {
            for (int j = 0; j < deck[i].Count; j++)
                { groupDic[count] = deck[i].TileSet; count++; }
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
