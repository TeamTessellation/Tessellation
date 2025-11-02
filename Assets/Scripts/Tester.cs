using UnityEngine;

public class Tester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tile obj = Pool<Tile>.Get();
        // PoolAble를 상속받은 Tile 풀링 해서 받아온다
        TileData test = new();
        Tile obj2 = Pool<Tile, TileData>.Get(test);
        // Tile은 PoolAble<TData> 를 받아왔음으로 데이터 세팅 풀링도 가능하다
        b = obj;
        RTPool.InitObjPool(obj.gameObject, "Test");
        // 런타임에서 받아온 Tile 오브젝트를 기반으로 풀링 생성한다
        a = RTPool.Get("Test");
        // 풀링을 받아온다
    }

    GameObject a;
    Tile b;

    private void Awakev()
    {
        RTPool.Return(a);
        Pool<Tile>.Return(b);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
