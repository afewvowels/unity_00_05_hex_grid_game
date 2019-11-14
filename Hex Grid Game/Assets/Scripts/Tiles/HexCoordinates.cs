using UnityEngine;
using System.IO;

[System.Serializable]
public class HexCoordinates
{
	[SerializeField]
	private int x, z;

	public int X
	{
		get
		{
			return this.x;
		}
	}
	public int Y
	{
		get
		{
			return -X - Z;
		}
	}
	public int Z
	{
		get
		{
			return this.z;
		}
	}

	public HexCoordinates(int x, int z)
	{
        if (HexDefinition.Wrapping)
        {
            int oX = x + z / 2;
            if (oX < 0)
            {
                x += HexDefinition.wrapSize;
            }
            else if (oX >= HexDefinition.wrapSize)
            {
                x -= HexDefinition.wrapSize;
            }
        }


		this.x = x;
		this.z = z;
	}

	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		return new HexCoordinates(x - z / 2, z);
	}

	public static HexCoordinates FromPosition(Vector3 position)
	{
		float x = position.x / HexDefinition.innerDiameter;
		float y = -x;

		float offset = position.z / (HexDefinition.outerRadius * 3.0f);
		x -= offset;
		y -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x - y);

		if (iX + iY + iZ != 0)
		{
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x - y - iZ);

			if (dX > dY && dX > dZ)
			{
				iX = -iY - iZ;
			}
			else if (dZ > dY)
			{
				iZ = -iX - iY;
			}
		}

		return new HexCoordinates(iX, iZ);
	}

    public int DistanceTo (HexCoordinates other)
    {
        //return
        //    ((x < other.x ? other.x - x : x - other.x) +
        //    (Y < other.Y ? other.Y - Y : Y - other.Y) +
        //    (z < other.z ? other.z - z : z - other.z)) / 2;

        int xy =
            (x < other.x ? other.x - x : x - other.x) +
            (Y < other.Y ? other.Y - Y : Y - other.Y);

        if (HexDefinition.Wrapping)
        {
            other.x += HexDefinition.wrapSize;
            int xyWrapped =
                (x < other.x ? other.x - x : x - other.x) +
                (Y < other.Y ? other.Y - Y : Y - other.Y);
            if (xyWrapped < xy)
            {
                xy = xyWrapped;
            }
            else
            {
                other.x -= 2 * HexDefinition.wrapSize;
                xyWrapped =
                    (x < other.x ? other.x - x : x - other.x) +
                    (Y < other.Y ? other.Y - Y : Y - other.Y);
                if (xyWrapped < xy)
                {
                    xy = xyWrapped;
                }
            }
        }

        return (xy + (z < other.z ? other.z - z : z - other.z)) / 2;
    }

	public override string ToString()
	{
		return "(" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + ")";
	}

	public string ToStringOnSeparateLines()
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}

    public void Save(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(z);
    }

    public static HexCoordinates Load (BinaryReader reader)
    {
        int xCoord = reader.ReadInt32();
        int zCoord = reader.ReadInt32();
        HexCoordinates c = new HexCoordinates(xCoord, zCoord);
        return c;
    }
}
