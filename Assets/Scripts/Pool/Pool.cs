using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PoolManager
{
    public static Transform S_GlobalRoot;
    public static Transform S_OriObjRoot;
    public const string Obj_Root = "PoolObj/";

    static PoolManager()
    {
        S_GlobalRoot = (GameObject.Find("@Pool") ?? new GameObject("@Pool")).transform;
        S_OriObjRoot = (GameObject.Find("@OriObj") ?? new GameObject("@OriObj")).transform;
        S_OriObjRoot.SetParent(S_GlobalRoot);
        GameObject.DontDestroyOnLoad(S_GlobalRoot);
    }
}

/// <summary>
/// TData 입력값을 통한 오브젝트 세팅을 지원하는 Pool Calss
/// </summary>
/// <typeparam name="T">오브젝트 클래스</typeparam>
/// <typeparam name="TData">입력 데이터 클래스</typeparam>
public static class Pool<T, TData> where T : MonoBehaviour, IPoolAble<TData>
{
    /// <summary>
    /// 풀에 저장 된 오브젝트를 Init후 받는다.
    /// </summary>
    /// <param name="data">Init용 Data</param>
    public static T Get(TData data)
    {
        var obj = Pool<T>.Get();
        obj.Set(data);
        return obj;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 반환한다.
    /// </summary>
    public static void Return(T obj) => Pool<T>.Return(obj);
}

public static class Pool<T> where T : MonoBehaviour, IPoolAble
{
    private static Stack<T> _pool;
    private static GameObject _prefab;

    private static Transform _root;

    private static Transform _activeRoot;
    private static Transform _poolRoot;

    private static bool _isReady = false;

    /// <summary>
    /// Init시 기본 사이즈
    /// </summary>
    private static int _size = 5;

    static Pool()
    {
        InitRoot();
        InitData();
        InitPool(_size);
    }

    private static void Generate()
    {
        T obj = GameObject.Instantiate(_prefab, _poolRoot).GetComponent<T>();
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
    }

    private static void InitRoot()
    {
        string poolName = $"@{typeof(T).Name}";
        _root = new GameObject($"{poolName}_Root").transform;
        _root.SetParent(PoolManager.S_GlobalRoot);
        _activeRoot = new GameObject($"@Active").transform;
        _poolRoot = new GameObject($"@Pool").transform;
        _activeRoot.SetParent(_root);
        _poolRoot.SetParent(_root);
    }

    private static void InitData()
    {
        var attr = (PoolSizeAttribute?)Attribute.GetCustomAttribute(typeof(T), typeof(PoolSizeAttribute));
        _size = attr?.Size ?? _size;
        _prefab = Resources.Load<GameObject>($"{PoolManager.Obj_Root}{typeof(T).Name}");
        _pool = new();
    }

    /// <summary>
    /// Pool 기본 세팅, 가능 / 불가능 여부를 체크한 뒤 size만큼 기본 오브젝트를 생성한다.
    /// </summary>
    /// <param name="size">Init 사이즈</param>
    private static void InitPool(int size)
    {
        if (!(CheckPossible())) return;
        if (_isReady) return;

        for (int i = 0; i < size; i++)
            Generate();

        _isReady = true;
    }

    /// <summary>
    /// 풀에 저장 된 오브젝트를 받는다.
    /// </summary>
    public static T Get()
    {
        while (_pool.Count <= 0) Generate();
        var obj = _pool.Pop();
        obj.gameObject.SetActive(true);
        obj.transform.SetParent(_activeRoot);
        return obj;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 반환한다.
    /// </summary>
    public static void Return(T obj)
    {
        obj.Reset();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_poolRoot);
        _pool.Push(obj);
    }

    private static bool CheckPossible()
    {
        if (_prefab == null)
        {
            Debug.LogWarning($"{typeof(T).Name} class의 Pool Prefab이 Resources/PoolObj에 존재하지 않습니다.");
            return false;
        }
        if (_prefab.GetComponent<T>() == null)
        {
            Debug.LogWarning($"{typeof(T).Name} class의 Pool Prefab이 {typeof(T).Name} class component를 가지고 있지 않습니다.");
        }

        return (_prefab != null);
    }
}

public static class Pool
{
    private class ObjPool
    {
        public string Key;
        public int Size;
        public Stack<GameObject> Pool;
        public Transform Root;
        public Transform ActiveRoot;
        public Transform PoolRoot;
        public GameObject OriObj;
        public bool IsReady = false;
    }

    private static Dictionary<string, ObjPool> _objPool;

    static Pool() => _objPool = new();

    private static ObjPool GetObjPool(string key)
    {
        if (_objPool.TryGetValue(key, out ObjPool objPool)) return objPool;
        else
        {
            Debug.LogWarning($"{key}값에는 ObjPool이 할당되어있지않습니다.");
            return null;
        }
    }

    private static void Generate(ObjPool objPool)
    {
        GameObject obj = GameObject.Instantiate(objPool.OriObj, objPool.Root);
        obj.name = $"{obj.name}_{objPool.Key}";
        obj.SetActive(false);
        obj.transform.SetParent(objPool.PoolRoot);
        objPool.Pool.Push(obj);
    }

    private static void InitRoot(ObjPool objPool)
    {
        string poolName = $"@{objPool.Key}";
        objPool.Root = new GameObject($"{poolName}_Root").transform;
        objPool.Root.SetParent(PoolManager.S_GlobalRoot);
        objPool.ActiveRoot = new GameObject($"@Active").transform;
        objPool.PoolRoot = new GameObject($"@Pool").transform;
        objPool.ActiveRoot.SetParent(objPool.Root);
        objPool.PoolRoot.SetParent(objPool.Root);
    }

    /// <summary>
    /// Pool 기본 세팅, 가능 / 불가능 여부를 체크한 뒤 size만큼 기본 오브젝트를 생성한다.
    /// </summary>
    /// <param name="size">Init 사이즈</param>
    private static void InitPool(ObjPool objPool)
    {
        if (objPool.IsReady) return;

        for (int i = 0; i < objPool.Size; i++)
            Generate(objPool);

        objPool.IsReady = true;
    }

    /// <summary>
    /// ObjPool을 세팅한다.
    /// 필수적으로 최초 1회 호출해줘야한다.
    /// </summary>
    /// <param name="target">풀링하기 원하는 오브젝트 원형</param>
    /// <param name="key">저장 될 key 값 (기본값 - 오브젝트 이름)</param>
    /// <param name="size">기본 크기 (기본값 - 5)</param>
    public static void InitObjPool(GameObject target, string key = "", int size = 5)
    {
        if (key == "") key = target.name;

        ObjPool objPool = new();
        objPool.Key = key;
        objPool.Size = size;
        var oriObj = GameObject.Instantiate(target);
        oriObj.transform.SetParent(PoolManager.S_OriObjRoot);
        objPool.OriObj = oriObj;
        objPool.Pool = new();
        _objPool[key] = objPool;

        InitRoot(objPool);
        InitPool(objPool);
    }

    /// <summary>
    /// 풀에 저장 된 오브젝트를 받는다.
    /// </summary>
    public static GameObject Get(string key)
    {
        var objPool = GetObjPool(key);
        if (objPool == null) return null;

        while (objPool.Pool.Count <= 0) Generate(objPool);
        var obj = objPool.Pool.Pop();
        obj.SetActive(true);
        obj.transform.SetParent(objPool.ActiveRoot);
        return obj;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 반환한다.
    /// </summary>
    public static void Return(GameObject obj)
    {
        string key = obj.name.Split("_").Last();
        obj.SetActive(false);

        var objPool = GetObjPool(key);
        if (objPool == null)
        {
            GameObject.Destroy(obj);
            Debug.LogWarning("런타임에서 생성한 ObjPool에서 Get한 오브젝트의 이름을 변경하면 안됩니다." +
                "\n소속을 찾을 수 없어서 제거합니다.");
            return;
        }

        obj.transform.SetParent(objPool.PoolRoot);
        objPool.Pool.Push(obj);
    }
}


/// <summary>
/// Init이 필요한 풀링용 오브젝트
/// </summary>
/// <typeparam name="T">Init용 데이터</typeparam>
public interface IPoolAble<TData> : IPoolAble
{
    void Set(TData data);
}

/// <summary>
/// Init이 필요하지 않는 풀링용 오브젝트
/// </summary>
public interface IPoolAble
{
    void Reset();
}
