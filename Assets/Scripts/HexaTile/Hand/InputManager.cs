using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Player;
using Stage;
using System.Threading;
using UnityEngine;
using System;
using Core;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour, IPlayerTurnLogic, IBasicTurnLogic
{
    [System.Serializable]
    public enum eActiveItemType
    {
        None,
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

    public bool ReadyItem => (_readyItem != eActiveItemType.End);
    private eActiveItemType _readyItem;

    private void Awake()
    {
        Instance = this;
        _dataReady = false;
        _readyItem = eActiveItemType.End;
    }

    private void BindBtn()
    {
        var itemRoot = GameObject.FindWithTag("ItemRoot").transform;

        if (itemRoot.transform.childCount > 0)
            return;

        var unSetBtn = itemRoot.transform.parent.GetChild(0).GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => UnSetItem());
        unSetBtn.triggers.Add(entry);

        var itemHold = Pool<ItemHold>.Get();
        itemHold.transform.SetParent(itemRoot, false);
        itemHold.RegisterClickEvent(SetItem, eActiveItemType.Rotate);
        itemHold.transform.localScale = Vector3.one;
    }

    public void HandBoxClick(HandBox target)
    {
        if (_readyItem == eActiveItemType.End)
            return;

        UseItem(target);
    }

    private void UseItem(HandBox target)
    {
        Debug.Log("아이템 사용");
        UseItemAction?.Invoke(target);
        _readyItem = eActiveItemType.End;
        HandManager.Instance.RemoveItemIcon();
        PlayerStatus playerStatus = GameManager.Instance.PlayerStatus;
        playerStatus.StageAbilityUseCount += 1;
    }

    public void SetItem(eActiveItemType item)
    {
        if (!HandManager.Instance.IsPlayerInputEnabled)
            return;

        if (_readyItem == item)
        {
            _readyItem = eActiveItemType.End;
            HandManager.Instance.RemoveItemIcon();
            UseItemAction = null;
        }

        _readyItem = item;

        switch (item)
        {
            case eActiveItemType.Add:
                UseItemAction = AddTileSetItem;
                break;
            case eActiveItemType.Delete:
                UseItemAction = DeleteTileSetItem;
                break;
            case eActiveItemType.Rotate:
                UseItemAction = RotateTileSetItem;
                break;
        }
        HandManager.Instance.SetItemIcon(item);
    }

    public void UnSetItem()
    {
        _readyItem = eActiveItemType.End;
        HandManager.Instance.RemoveItemIcon();
        UseItemAction = null;
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
        handBox.Use();
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        IsPlayerInputEnabled = enabled;
    }

    public async UniTask<PlayerInputData> WaitForPlayerReady(CancellationToken token)
    {
        await UniTask.WaitUntil(() => _dataReady || token.IsCancellationRequested, cancellationToken: token);
        _dataReady = false;
        return _playerInputData;
    }

    public bool IsPlayerCanDoAction()
    {
        return (HandManager.Instance.HandCount > 0); // ToDo 핸드 수정
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
