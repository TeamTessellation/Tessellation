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
        Boom,
        End
    }

    public static InputManager Instance { get; private set; }

    public bool IsPlayerInputEnabled { get; private set; }

    public Action<HandBox> UseItemAction { get; private set; }

    private PlayerInputData _playerInputData;

    private bool _dataReady;
    private bool _isTurnEnd;

    private void Awake()
    {
        Instance = this;
        _dataReady = false;
        _isTurnEnd = true;
    }

    private void BindBtn()
    {
        var itemRoot = GameObject.FindWithTag("ItemRoot").transform;
        for (int i = 0; i < (int)Item.End; i++)
        {
            var ItemHold = Pool<ItemHold>.Get();
            ItemHold.transform.SetParent(itemRoot, false);
            ItemHold.RegisterClickEvent(SetItem, UseItem, (Item)i);
        }
        
    }

    private void UseItem()
    {
        Debug.Log("아이템 사용");
        bool success = HandManager.Instance.UseItemToTargetHandBox(UseItemAction);
        Debug.Log(success);
    }

    public void SetItem(Item item)
    {
        switch (item)
        {
            case Item.Add:
                UseItemAction = AddTileSetItem;
                break;
            case Item.Delete:
                UseItemAction = DeleteTileSetItem;
                break;
            case Item.Boom:
                UseItemAction = BoomItem;
                break;
        }
    }

    public void RotateTileSet(HandBox handBox) => RotateTileSetItem(handBox);

    private void RotateTileSetItem(HandBox handBox)
    {
        handBox.HoldTileSet.Rotate();
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
        return _isTurnEnd;
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
