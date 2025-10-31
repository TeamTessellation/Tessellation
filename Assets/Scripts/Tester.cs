using UnityEngine;

public class Tester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tile obj = Pool<Tile>.Get();
        // PoolAble�� ��ӹ��� Tile Ǯ�� �ؼ� �޾ƿ´�
        TileData test = new();
        Tile obj2 = Pool<Tile, TileData>.Get(test);
        // Tile�� PoolAble<TData> �� �޾ƿ������� ������ ���� Ǯ���� �����ϴ�
        b = obj;
        Pool.InitObjPool(obj.gameObject, "Test");
        // ��Ÿ�ӿ��� �޾ƿ� Tile ������Ʈ�� ������� Ǯ�� �����Ѵ�
        a = Pool.Get("Test");
        // Ǯ���� �޾ƿ´�
    }

    GameObject a;
    Tile b;

    private void Awakev()
    {
        Pool.Return(a);
        Pool<Tile>.Return(b);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
