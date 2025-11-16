using Cysharp.Threading.Tasks;
using SaveLoad;
using Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Machamy.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Player;

public class HandManager : MonoBehaviour, IFieldTurnLogic, ISaveTarget
{
    public static HandManager Instance { get; private set; }

    [SerializeField] private DeckSO originDeckSO;
    public DeckSO DeckSO;

    private Transform _handRoot;
    private HandBox[] _hand;

    private HandBox _targetHandBox;
    private Camera _cam => Camera.main;
    private bool _onMouseDown;
    private bool _dragTileSet;

    private int _remainHand = 0;
    public int HandCount { get { return _remainHand; } }
    private int _handSize = 3;
    private Coordinate _lastDragCoor;

    public Guid Guid { get; init; }
    
    private async UniTask Awake()
    {
        DeckSO = Instantiate(originDeckSO);
        
        AddDeckData(TileOption.Gold, 5);
        
        // LogEx.Log($"Instance id : {GetInstanceID()} - {gameObject.scene.name}, {gameObject.name}");
        Instance = this;
        _hand = new HandBox[0];
        _dragTileSet = false;
        _onMouseDown = false;
        // _cam = Camera.main;
        _remainHand = 0;
        _lastDragCoor = new();
        await GameManager.WaitForInit();
        
        _handRoot = GameObject.FindWithTag("HandRoot").transform;

        SaveLoadManager.RegisterPendingSavable(this);
    }

    public void StageClear()
    {
        _dragTileSet = false;
        _onMouseDown = false;
        _remainHand = 0;
        _lastDragCoor = new();
        ResetHand(_handSize);
    }


    void Update()
    {
        if (!InputManager.Instance.IsPlayerInputEnabled)
            return;

        if (_onMouseDown)
        {
            Vector2 screenPos;
            if (Pointer.current != null)
                screenPos = Pointer.current.position.ReadValue();
            else
                screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));

            if (Vector2.Distance(_startPos, worldPos) > 0.5f)
            {
                if (!InputManager.Instance.ReadyItem)
                {
                    _onMouseDown = false;
                    _dragTileSet = true;
                    _targetHandBox.HoldTileSet.transform.localScale = Vector3.one;
                    _targetHandBox.HoldTileSet.SetOrderInTop();
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandBoxClick();
            }

            // 2. 모바일 (터치)
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    HandBoxClick();
                }
            }

            return;
        }

        if (_dragTileSet)
        {
            Vector2 screenPos;
            if (Pointer.current != null)
                screenPos = Pointer.current.position.ReadValue();
            else
                screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));
            Coordinate dragCoor = worldPos.ToCoor(Field.Instance.TileOffset);
            if (dragCoor != _lastDragCoor)
            {
                _lastDragCoor = dragCoor;
                Field.Instance.ClearSilhouette();
                Field.Instance.ShowSilhouette(_targetHandBox.HoldTileSet, dragCoor);
            }

            _targetHandBox.HoldTileSet.transform.position = worldPos;

            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                PlaceTileSet(worldPos);
            }

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    PlaceTileSet(worldPos);
                }
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

    public void AddDeckData(TileOption tileOption, int amount)
    {
        OffsetTileData newOffSetTileData = new OffsetTileData();
        newOffSetTileData.TileData = new TileData(tileOption, 1);
        newOffSetTileData.Coor = new Coordinate(0, 0);
        
        TileSetData newTileSetData = new TileSetData();
        newTileSetData.Data.Add(newOffSetTileData);
        newTileSetData.Size = 0;
        newTileSetData.Offset = 0;
        
        DeckData newDeckData = new DeckData(newTileSetData);
        newDeckData.Count = amount;
        
        
        DeckSO.Deck.Add(newDeckData);
    }
    
    public void RemoveDeckData(DeckData data)
    {
        DeckSO.Deck.Remove(data);
    }

    public void SetItemIcon(InputManager.eActiveItemType item)
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_hand[i] == null || _hand[i].IsUsed)
                continue;
            _hand[i].SetItemIcon(item);
        }
    }

    public void RemoveItemIcon()
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            _hand[i]?.RemoveItemIcon();
        }
    }

    private void HandBoxClick()
    {
        _onMouseDown = false;
        _dragTileSet = false;
        InputManager.Instance.HandBoxClick(_targetHandBox);
    }

    private void PlaceTileSet(Vector2 worldPos)
    {
        _targetHandBox.HoldTileSet.SetOrderInHand();
        Field.Instance.ClearSilhouette();

        if (!InputManager.Instance.IsPlayerInputEnabled)
            return;

        if (Field.Instance.TryPlace(_targetHandBox.HoldTileSet, worldPos.ToCoor(Field.Instance.TileOffset), out var placeTiles))
        {
            _remainHand--;
            InputManager.Instance.PlaceTileSet(worldPos, _targetHandBox, placeTiles);
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
    }


    private Vector2 _startPos;

    private void HandBoxMouseDown(HandBox target)
    {
        if (target.IsUsed || TurnManager.Instance.State != TurnState.Player || !InputManager.Instance.IsPlayerInputEnabled)
            return;

        Vector2 screenPos;
        if (Pointer.current != null)
            screenPos = Pointer.current.position.ReadValue();
        else
            screenPos = Mouse.current.position.ReadValue();
        _startPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));
        _targetHandBox = target;
        _onMouseDown = true;
    }

    public bool CanPlace()
    {
        return Field.Instance.TryPlaceAllTileSet(_hand.ToList(), PlayerStatus.Current.inventory.CurrentItem == InputManager.eActiveItemType.Rotate, PlayerStatus.Current.inventory.currentItemCount);
    }
    

    public void ResetHand(int handSize)
    {
        RemoveItemIcon();
        _handSize = handSize;
        
        // 기존 hand 배열의 HandBox들을 Pool에 반환
        if (_hand != null)
        {
            for (int i = 0; i < _hand.Length; i++)
            {
                if (_hand[i] != null)
                    Pool<HandBox>.Return(_hand[i]);
            }
        }
        
        // 배열을 새로 초기화 (중요!)
        _hand = new HandBox[0];
        _remainHand = 0;
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
            _hand[i] = handBox;
        }

        RefreshGameObject().Forget();
    }

    private async UniTask RefreshGameObject()
    {
        await UniTask.NextFrame();
        for (int i = 0; i < _hand.Length; i++)
        {
            _hand[i].gameObject.SetActive(false);
            _hand[i].gameObject.SetActive(true);
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

    public void LoadData(GameData data)
    {
        ResetHand(data.HandCount);

        _hand = new HandBox[data.HandCount];
        for (int i = 0; i < data.HandCount; i++)
        {
            HandBox handBox;
            if (data.HandData[i] == null)
                handBox = Pool<HandBox>.Get();
            else
                handBox = Pool<HandBox, TileSetData>.Get(data.HandData[i]);

            handBox.transform.SetParent(_handRoot, false);
            handBox.RegisterDownEvent(HandBoxMouseDown);
            _hand[i] = handBox;
        }

        RefreshGameObject().Forget();
    }

    public void SaveData(ref GameData data)
    {
        data.HandData = new TileSetData[_handSize];
        data.HandCount = _handSize;

        for (int i = 0; i < _hand.Length; i++)
        {
            if (_hand[i].IsUsed)
            {
                data.HandData[i] = null;
                continue;
            }

            data.HandData[i] = _hand[i].HoldTileSet.Data;
        }
    }
}
