using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

public class Generator_Manager : MonoBehaviour
{
    [Header("UI Bind")]
    public Transform DeckUIRoot;
    public GameObject BoxPrefab;
    public GameObject DeckUI;

    [Header("Target Deck")]
    public DeckSO TargetDeck;

    [Header("Field Tile Sprite")]
    public List<Sprite> Sprites;

    private int _targetIndex;
    private TileSetData TargetData { get {  return TargetDeck.Deck[_targetIndex].TileSet; } }
    private List<TileSet> _tileSets;
    private OffsetTileData _targetTileOffsetData;
    private GameObject _addBox;
    private FieldClickManager _fieldClickManager;
    private List<Tile> _fieldTile;
    private Dictionary<Coordinate, Tile> _tileDic;

    void Start()
    {
        _fieldTile = new();
        _tileDic = new();
        _targetIndex = 0;
        _fieldClickManager = GetComponent<FieldClickManager>();

        SetBoxObj();
        if (TargetDeck == null)
            { Debug.LogWarning("Deck이 할당이 안되었습니다."); return; }
        _fieldClickManager.RegisterClickEvent(ClickTile);
    }

    private void OnDisable()
    {
        EditorUtility.SetDirty(TargetDeck);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void Update()
    {
        if (_targetTileOffsetData == null)
            return;

        for (int i = 0; i <= (int)TileOption.End; i++)
        {
            Key key = Key.Digit1 + i;

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                if (i == 0)
                {
                    TargetData.Data.Remove(_targetTileOffsetData);
                    Pool<Tile>.Return(_tileDic[_targetTileOffsetData.Coor]);
                    _targetTileOffsetData = null;
                    Debug.Log("삭제");
                    continue;
                }
                TileOption option = (TileOption)(i - 1);
                Debug.Log(option);
                TileData tileData = new(option, 1);;
                _targetTileOffsetData.TileData = tileData;
                Sprite sprite;
                if (Sprites.Count <= (int)option)
                    sprite = Sprites[Sprites.Count - 1];
                else
                    sprite = Sprites[(int)option];
                _tileDic[_targetTileOffsetData.Coor].ChangeSprite(sprite);
            }
        }
    }

    private void ConnectGenerator2TargetData()
    {
        for (int i = 0; i < _fieldTile.Count; i++)
            Pool<Tile>.Return(_fieldTile[i]);
        _fieldTile.Clear();
        _tileDic.Clear();

        if (TargetDeck.Deck.Count <= _targetIndex)
            return;

        for (int i = 0; i < TargetData.Data.Count; i++)
        {
            var offsetData = TargetData.Data[i];
            var tile = Pool<Tile, TileData>.Get(offsetData.TileData);
            Sprite sprite;
            if (Sprites.Count <= (int)offsetData.TileData.Option)
                sprite = Sprites[Sprites.Count - 1];
            else
                sprite = Sprites[(int)offsetData.TileData.Option];
            tile.ChangeSprite(sprite);

            _fieldTile.Add(tile);
            Debug.Log(offsetData.Coor);
            _tileDic[offsetData.Coor] = tile;
            tile.transform.localPosition = offsetData.Coor.ToWorld();
        }
    }

    private void ClickTile(Coordinate coor)
    {
        if (coor.CircleRadius >= 5)
            return;

        for (int i = 0; i < TargetData.Data.Count; i++)
        {
            if (TargetData.Data[i].Coor == coor)
            {
                _targetTileOffsetData = TargetData.Data[i];
                Debug.Log($"{coor.ToShortString()} 클릭 됨");
                return;
            }
        }
        OffsetTileData data = new() { Coor = coor };
        TargetData.Data.Add(data);
        _targetTileOffsetData = data;
        var tile = Pool<Tile, TileData>.Get(data.TileData);
        _fieldTile.Add(tile);
        _tileDic[coor] = tile;
        tile.transform.localPosition = data.Coor.ToWorld();
        tile.Coor = coor;

        Sprite sprite = Sprites[0];
        _tileDic[coor].ChangeSprite(sprite);

        Debug.Log($"{coor.ToShortString()} 생성 됨");
    }

    private void SetBoxObj()
    {
        var obj = Instantiate(BoxPrefab);
        RTPool.InitObjPool(obj, "Box");
        Destroy(obj);
    }

    private void TileSetBoxClick(int index)
    {
        _targetIndex = index;
        Debug.Log(index);
    }

    private void SetDeckUI()
    {
        List<GameObject> oldBoxs = new();
        foreach (Transform oldBox in DeckUIRoot.transform)
        {
            if (oldBox.name != DeckUIRoot.name)
                oldBoxs.Add(oldBox.gameObject);
            foreach(Transform oldBoxChild in oldBox.transform)
            {
                if (oldBoxChild.TryGetComponent<TileSet>(out TileSet oldTileSet))
                    Pool<TileSet>.Return(oldTileSet);
            }
        }
        for (int i = 0; i < oldBoxs.Count; i++)
            RTPool.Return(oldBoxs[i]);

        var addBox = RTPool.Get("Box");
        addBox.transform.SetParent(DeckUIRoot, false);
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => AddBoxLast(new DeckData()));
        var trigger = addBox.GetComponent<EventTrigger>();
        trigger.triggers.Clear();
        trigger.triggers.Add(entry);
        _addBox = addBox;

        _tileSets ??= new();

        for (int i = 0; i < _tileSets.Count; i++)
            Pool<TileSet>.Return(_tileSets[i]);

        _tileSets.Clear();

        var deck = TargetDeck.Deck;
        for (int i = 0; i < deck.Count; i++)
        {
            AddBox(deck[i], i);
        }
    }

    private void AddBox(DeckData data, int index)
    {
        var obj = RTPool.Get("Box");
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => TileSetBoxClick(index));
        var trigger = obj.GetComponent<EventTrigger>();
        trigger.triggers.Clear();
        trigger.triggers.Add(entry);
        obj.transform.SetParent(DeckUIRoot, false);
        _addBox.transform.SetParent(null);
        _addBox.transform.SetParent(DeckUIRoot);

        int maxRadius = 0;
        for (int j = 0; j < data.TileSet.Data.Count; j++)
        {
            maxRadius = Mathf.Max(data.TileSet.Data[j].Coor.CircleRadius, maxRadius);
        }

        float size = (maxRadius * 2 + 1 > 3) ? 5 / (Mathf.Sqrt(3) * (maxRadius * 2 + 1)) : 1;
        var tileSet = Pool<TileSet, TileSetData>.Get(data.TileSet);
        tileSet.transform.SetParent(DeckUIRoot.transform.GetChild(index), false);
        tileSet.transform.localScale = Vector2.one * size;
        tileSet.transform.localPosition = Vector3.zero;
    }

    private void AddBoxLast(DeckData data)
    {
        int index = TargetDeck.Deck.Count;
        TargetDeck.Deck.Add(data);
        AddBox(data, index);
    }

    public void RotateTileSet()
    {
        var offsetData = TargetData.Data;
        for (int i = 0; i < offsetData.Count; i++)
        {
            offsetData[i].Coor = offsetData[i].Coor.RotateR60();
        }
        ConnectGenerator2TargetData();
    }

    public void DeleteTileSet()
    {
        TargetDeck.Deck.RemoveAt(_targetIndex);
        _targetIndex = 0;
        ConnectGenerator2TargetData();
    }

    public void OpenDeckUI()
    {
        DeckUI.gameObject.SetActive(true);
        SetDeckUI();
    }

    public void CloseDeckUI()
    {
        ConnectGenerator2TargetData();
        DeckUI.gameObject.SetActive(false);
    }
}
