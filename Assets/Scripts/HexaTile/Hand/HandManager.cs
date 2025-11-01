using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    public DeckSO DeckSO;
    public Transform HandRoot;

    private HandBox[] _hand;

    private HandBox _targetHandBox;
    private bool _dragTileSet;
    private Camera _cam;
    private Vector3 _tileSetOriScale;

    private void Start()
    {
        _hand = new HandBox[0];
        _dragTileSet = false;
        _cam = Camera.main;
        SetHand(3);
    }

    void Update()
    {
        if (_dragTileSet)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));

            _targetHandBox.HoldTileSet.transform.position = worldPos;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                PlaceTileSet(worldPos);
            }
        }
    }

    private void PlaceTileSet(Vector2 worldPos)
    {
        if(Field.Instance.TryPlace(_targetHandBox.HoldTileSet, worldPos.ToCoor()))
        {
            _targetHandBox.Use();
        }
        else
            FailPlace();

        _targetHandBox = null;
        _dragTileSet = false;
    }

    private void FailPlace()
    {
        _targetHandBox.HoldTileSet.transform.localPosition = Vector3.zero;
        _targetHandBox.HoldTileSet.transform.localScale = _tileSetOriScale;
    }

    private void HandBoxMouseDown(HandBox target)
    {
        if (target.IsUsed)
            return;

        _targetHandBox = target;
        _dragTileSet = true;
        _tileSetOriScale = _targetHandBox.HoldTileSet.transform.localScale;
        _targetHandBox.HoldTileSet.transform.localScale = Vector3.one;
    }


    public void SetHand(int handSize)
    {
        for (int i = 0; i < _hand.Length; i++)
            Pool<HandBox>.Return(_hand[i]);

        _hand = new HandBox[handSize];
        var tileSetDatas = GetRadomTileSetDataInGroup(handSize);
        for (int i = 0; i < tileSetDatas.Length; i++)
        {
            var handBox = Pool<HandBox, TileSetData>.Get(tileSetDatas[i]);
            handBox.transform.SetParent(HandRoot, false);
            handBox.RegisterClickEvent(HandBoxMouseDown);
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
