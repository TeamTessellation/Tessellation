using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Player;
using Stage;
using System.Threading;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IPlayerTurnLogic, IBasicTurnLogic
{
    [System.Serializable]
    public enum Item
    {
        Add,
        Delete,
        Rotate,
        End
    }

    public static InputManager Instance { get; private set; }

    public bool IsPlayerInputEnabled { get; private set; }

    public Action<HandBox> UseItemAction { get; private set; }

    private PlayerInputData _playerInputData;

    private bool _dataReady;
    private bool _isTurnEnd;
    private Item _readyItem;

    private void Awake()
    {
        Instance = this;
        _dataReady = false;
        _isTurnEnd = true;
        _readyItem = Item.End;
    }

    private void BindBtn()
    {
        var itemRoot = GameObject.FindWithTag("ItemRoot").transform;
        var itemHold = Pool<ItemHold>.Get();
        itemHold.transform.SetParent(itemRoot, false);
        itemHold.RegisterClickEvent(SetItem, Item.Rotate);
        itemHold.transform.localScale = Vector3.one;
    }

    public void HandBoxClick(HandBox target)
    {
        if (_readyItem == Item.End)
            return;

        UseItem(target);
    }

    private void UseItem(HandBox target)
    {
        Debug.Log("아이템 사용");
        UseItemAction?.Invoke(target);
        HandManager.Instance.RemoveItemIcon();
    }

    public void SetItem(Item item)
    {
        if (_readyItem == item)
        {
            _readyItem = Item.End;
            HandManager.Instance.RemoveItemIcon();
            UseItemAction = null;
        }

        _readyItem = item;

        switch (item)
        {
            case Item.Add:
                UseItemAction = AddTileSetItem;
                break;
            case Item.Delete:
                UseItemAction = DeleteTileSetItem;
                break;
            case Item.Rotate:
                UseItemAction = RotateTileSetItem;
                break;
        }
        HandManager.Instance.SetItemIcon(item);
    }

    public void RotateTileSet(HandBox handBox) => RotateTileSetItem(handBox);

    private void RotateTileSetItem(HandBox handBox)
    {
        handBox.HoldTileSet.Rotate();
        handBox.SetOnHand();
    }

    private void AddTileSetItem(HandBox handBox)
    {
        var deck = HandManager.Instance.GetDeckData(handBox.HoldTileSet.Data);
        deck.Count++;
    }

    private void DeleteTileSetItem(HandBox handBox)
    {
        var deck = HandManager.Instance.GetDeckData(handBox.HoldTileSet.Data);
        deck.Count--;

        if (deck.Count <= 0)
            HandManager.Instance.RemoveDeckData(deck);
    }

    private void BoomItem(HandBox handBox)
    {

    }

    public void PlaceTileSet(Vector2 worldPos, HandBox handBox, List<Tile> tiles)
    {
        _playerInputData = new(tiles);
        _dataReady = true;
        _isTurnEnd = true;
        handBox.Use();
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        IsPlayerInputEnabled = enabled;
    }

    public async UniTask<PlayerInputData> WaitForPlayerReady(CancellationToken token)
    {
        await UniTask.WaitUntil(() => _dataReady);
        _dataReady = false;
        return _playerInputData;
    }

    public bool IsPlayerCanDoAction()
    {
        return _isTurnEnd; // ToDo 핸드 수정
    }

    public async UniTask OnTurnStart(int turnCount, CancellationToken token)
    {
        BindBtn();
        await UniTask.CompletedTask;
    }

    public async UniTask OnTurnEnd(int turnCount, CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}
