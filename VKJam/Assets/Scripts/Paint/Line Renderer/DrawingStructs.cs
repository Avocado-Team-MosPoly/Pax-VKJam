using System;
using UnityEngine;
using Unity.Netcode;

public struct Vector3Short : INetworkSerializable
{
    public short x;
    public short y;
    public short z;

    public Vector3Short(int x, int y, int z)
    {
        this.x = (short)x;
        this.y = (short)y;
        this.z = (short)z;
    }
    public Vector3Short(short x, short y, short z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static float Distance(Vector3Short a, Vector3Short b)
    {
        Vector3Short direction = b - a;
        float distance = Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y + direction.z * direction.z);
        
        return distance;
    }

    public static Vector3Short operator - (Vector3Short a, Vector3Short b)
    {
        return new Vector3Short
        {
            x = (short)(b.x - a.x),
            y = (short)(b.y - a.y),
            z = (short)(b.z - a.z)
        };
    }

    public static explicit operator Vector3Short(Vector3 vector3)
    {
        return new Vector3Short((short)vector3.x, (short)vector3.y, (short)vector3.z);
    }
    public static implicit operator Vector3(Vector3Short vector3Short)
    {
        return new Vector3(vector3Short.x, vector3Short.y, vector3Short.z);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref z);
    }

    public override string ToString()
    {
        return $"({nameof(Vector3Short)}({x}, {y}, {z})";
    }
}

[Serializable]
public struct RenderTextureSettings
{
    public int width;
    public int height;
    public RenderTextureFormat format;
    public TextureWrapMode wrapMode;
    public FilterMode filterMode;
}
