using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct Coordinate
{
    /// <summary>
    /// 2D 좌펴계 기준 좌표, 사실상 3D의 x, y만 가져오는거와 동일
    /// </summary>
    public Vector2Int Pos;
    /// <summary>
    /// 3D 좌표계 기준 좌표
    /// </summary>
    public Vector3Int Pos3D { get { return new Vector3Int(Pos.x, Pos.y, -(Pos.x + Pos.y)); } }
    /// <summary>
    /// 본인이 속한 Circle 반지름
    /// </summary>
    public int CircleRadius { get { return Mathf.Max(Mathf.Abs(Pos3D.x), Mathf.Abs(Pos3D.y), Mathf.Abs(Pos3D.z)); } }

    public Coordinate(int x, int y)
    {
        Pos = new Vector2Int(x, y);
    }

    public Coordinate(Vector2 pos)
    {
        Pos = new Vector2Int((int)pos.x, (int)pos.y);
    }

    public Coordinate(Vector3 pos)
    {
        Pos = new Vector2Int((int)pos.x, (int)pos.y);
    }

    public static Coordinate operator +(Coordinate c, Direction dir)
    {
        return dir switch

        {
            Direction.RU => c + new Coordinate(1, -1),
            Direction.R => c + new Coordinate(1, 0),
            Direction.RD => c + new Coordinate(0, 1),
            Direction.LD => c + new Coordinate(-1, 1),
            Direction.L => c + new Coordinate(-1, 0),
            Direction.LU => c + new Coordinate(0, -1),
            _ => c
        };
    }

    public static Coordinate operator -(Coordinate c, Direction dir)
    {
        return dir switch

        {
            Direction.RU => c - new Coordinate(1, -1),
            Direction.R => c - new Coordinate(1, 0),
            Direction.RD => c - new Coordinate(0, 1),
            Direction.LD => c - new Coordinate(-1, 1),
            Direction.L => c - new Coordinate(-1, 0),
            Direction.LU => c - new Coordinate(0, -1),
            _ => c
        };
    }

    public static Coordinate operator +(Coordinate c, Coordinate other)
    {
        return new() { Pos = c.Pos + other.Pos };
    }

    public static Coordinate operator +(Coordinate c, Vector2 other)
    {
        return c + new Coordinate(other);
    }

    public static Coordinate operator -(Coordinate c, Coordinate other)
    {
        return new() { Pos = c.Pos - other.Pos };
    }

    public static bool operator ==(Coordinate left, Coordinate right)
    {
        return (left.Pos == right.Pos);
    }

    public static bool operator !=(Coordinate left, Coordinate right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"Coor(x {Pos3D.x}, y {Pos3D.y}, z {Pos3D.z} / 속한 테두리 - {CircleRadius})";
    }

    public override bool Equals(object obj)
    {
        return obj is Coordinate coordinate &&
               Pos.Equals(coordinate.Pos);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Pos);
    }
}

public static class CoorExtension
{
    public static string ToShortString(this Coordinate coor)
    {
        return $"({coor.Pos3D.x}, {coor.Pos3D.y}. {coor.Pos3D.z})";
    }

    public static Vector2 ToWorld(this Coordinate coor, float size = 1)
    {
        float x = coor.Pos3D.x * Mathf.Sqrt(3) + coor.Pos3D.y * Mathf.Sqrt(3) * 0.5f;
        float y = -coor.Pos3D.y * 1.5f;
        return new Vector2(x, y) * size;
    }

    public static Vector2 ToWorld(this Coordinate coor, Vector2 offset, float size = 1)
    {
        return ToWorld(coor) * size + offset;
    }

    public static Coordinate ToCoor(this Vector2 world, Vector2 offset, float size = 1)
    {
        return ToCoor((world - offset) / size);
    }

    public static Coordinate ToCoor(this Vector2 world, float size = 1)
    {
        world /= size;
        float cy = -world.y / 1.5f;
        float cx = (world.x / Mathf.Sqrt(3f)) - (cy * 0.5f);
        float cz = -(cx + cy);

        int rx = Mathf.RoundToInt(cx);
        int ry = Mathf.RoundToInt(cy);
        int rz = Mathf.RoundToInt(cz);

        float dx = Mathf.Abs(rx - cx);
        float dy = Mathf.Abs(ry - cy);
        float dz = Mathf.Abs(rz - cz);

        if (dx > dy && dx > dz)
            rx = -ry - rz;
        else if (dy > dz)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Coordinate(rx, ry);
    }

    public static Coordinate ToCoor(this Vector3 world)
    {
        return ((Vector2)world).ToCoor();
    }

    public static Coordinate FromXY(this Vector2Int xy)
    {
        return new Coordinate(xy.x, xy.y);
    }

    public static Coordinate FromXZ(this Vector2Int xz)
    {
        return new Coordinate(xz.x, -xz.x -xz.y);
    }

    public static Coordinate FromYZ(this Vector2Int yz)
    {
        return new Coordinate(-yz.x - yz.y, yz.x);
    }

    public static Coordinate RotateR60(this Coordinate coor)
    {
        return new Coordinate(-coor.Pos3D.y, -coor.Pos3D.z);
    }
}
