using Cysharp.Threading.Tasks;
using Player;
using Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour, IFieldTurnLogic
{
    public static HandManager Instance { get; private set; }

    public DeckSO DeckSO;

    private Transform _handRoot;
    private HandBox[] _hand;

    private HandBox _targetHandBox;
    private bool _onMouseDown;
    private bool _dragTileSet;
    private Camera _cam;
    private int _remainHand = 0;
    private int _handSize = 3;
    private HandBox _mouseOnHandBox;

    public bool IsPlayerInputEnabled => throw new NotImplementedException();

    private void Awake()
    {
        Instance = this;
        _hand = new HandBox[0];
        _handRoot = GameObject.FindWithTag("HandRoot").transform;
        _dragTileSet = false;
        _onMouseDown = false;
        _cam = Camera.main;
        _remainHand = 0;
    }

    void Update()
    {
        if (_onMouseDown)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));

            if (Vector2.Distance(_startPos, worldPos) > 0.5f)
            {
                _onMouseDown = false;
                _dragTileSet = true;
                _targetHandBox.HoldTileSet.transform.localScale = Vector3.one;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandBoxClick();
            }

            return;
        }

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

    public DeckData GetDeckData(TileSetData data)
    {
        for (int i = 0; i < DeckSO.Deck.Count; i++)
        {
            if (DeckSO.Deck[i].TileSet == data)
                return DeckSO.Deck[i];
        }
#if UNITY_EDITOR
        Debug.Log("해당 되는 타일셋을 덱에서 찾지 못함");
#endif
        return null;
    }

    public void RemoveDeckData(DeckData data)
    {
        DeckSO.Deck.Remove(data);
    }

    private void HandBoxClick()
    {
        _onMouseDown = false;
        _dragTileSet = false;
        InputManager.Instance.RotateTileSet(_targetHandBox);
        /*
        if (InputManager.Instance.UseItemAction == null)
            InputManager.Instance.RotateTileSet(_targetHandBox);
        else
            InputManager.Instance.UseItemAction?.Invoke(_targetHandBox);
        */
    }

    public bool UseItemToTargetHandBox(Action<HandBox> useItemAction)
    {
        if (_mouseOnHandBox == null) return false;
        useItemAction?.Invoke(_mouseOnHandBox);

        return true;
    }

    private void PlaceTileSet(Vector2 worldPos)
    {
        if(Field.Instance.TryPlace(_targetHandBox.HoldTileSet, worldPos.ToCoor(Field.Instance.TileOffset), out var placeTiles))
        {
            InputManager.Instance.PlaceTileSet(worldPos, _targetHandBox, placeTiles);

            _remainHand--;
            if (_remainHand <= 0)
                UseAllHand();
        }
        else
            FailPlace();

        _targetHandBox = null;
        _dragTileSet = false;
    }

    private void FailPlace()
    {
        _targetHandBox.SetOnHand();
    }

    private void UseAllHand()
    {
        _targetHandBox = null;
        _dragTileSet = false;
        SetHand();
    }


    private Vector2 _startPos;

    private void HandBoxMouseDown(HandBox target)
    {
        if (target.IsUsed || TurnManager.Instance.State != TurnState.Player)
            return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        _startPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));
        _targetHandBox = target;
        _onMouseDown = true;
    }

    private void HandBoxMouseEnter(HandBox target)
    {
        _mouseOnHandBox = target;
    }

    private void HandBoxMouseExit(HandBox target)
    {
        _mouseOnHandBox = null;
    }

    public void ResetHand(int handSize)
    {
        _handSize = handSize;
        for (int i = 0; i < _hand.Length; i++)
            Pool<HandBox>.Return(_hand[i]);
    }

    public void SetHand()
    {
        _remainHand = _handSize;
        for (int i = 0; i < _hand.Length; i++)
            Pool<HandBox>.Return(_hand[i]);

        _hand = new HandBox[_handSize];
        var tileSetDatas = GetRadomTileSetDataInGroup(_handSize);
        for (int i = 0; i < tileSetDatas.Length; i++)
        {
            var handBox = Pool<HandBox, TileSetData>.Get(tileSetDatas[i]);
            handBox.transform.SetParent(_handRoot, false);
            handBox.RegisterDownEvent(HandBoxMouseDown);
            handBox.RegisterEnterEvent(HandBoxMouseEnter);
            handBox.RegisterExitEvent(HandBoxMouseExit);
            _hand[i] = handBox;
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

    public async UniTask TileSetDraw(CancellationToken token)
    {
        SetHand();
        await UniTask.CompletedTask;
    }
}
