using Cysharp.Threading.Tasks;
using Player;
using Stage;
using System.Threading;
using UnityEngine;

public class InputManager : MonoBehaviour, IPlayerTurnLogic
{
    public static InputManager Instance { get; private set; }

    public bool IsPlayerInputEnabled { get; private set; }

    private PlayerInputData _playerInputData;

    private bool _dataReady;
    private bool _isTurnEnd;

    private void Awake()
    {
        Instance = this;
        _dataReady = false;
        _isTurnEnd = true;
    }

    public void PlaceTileSet(Vector2 worldPos, HandBox handBox)
    {
        _playerInputData = new(handBox.HoldTileSet.Tiles);
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
}
