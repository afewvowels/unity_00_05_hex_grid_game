using UnityEngine;

public class HexCell : MonoBehaviour
{
    public enum DjikstraColor { white, grey, black };

    public HexCoordinates coordinates;

	public Color hexColor;

	private bool isOccupied;

	public int startCellID;

	[SerializeField]
	private DjikstraColor color;

	[SerializeField]
	private int djikstraCost;

	[SerializeField]
	private int parentCellID;

	[SerializeField]
	int cellID;

	[SerializeField]
	int moveCost;

	[SerializeField]
	HexCell[] neighbors;

	[SerializeField]
	private int elevation = int.MinValue;

    [SerializeField]
    bool[] roads;

    [SerializeField]
    private int waterLevel = int.MinValue;

    public HexGridChunk chunk;

    private bool hasIncomingRiver, hasOutgoingRiver;

    [SerializeField]
    private HexDirection incomingRiverDirection, outgoingRiverDirection;

	public int Elevation
	{
		get
		{
			return elevation;
		}
		set
		{
            if (elevation == value)
            {
                return;
            }
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexDefinition.elevationStep;
			position.y += (HexDefinition.SampleNoise(position).y * 2.0f - 1.0f) * HexDefinition.elevationDisplacementStrength;
			transform.localPosition = position;

            if(
                hasOutgoingRiver &&
                elevation < GetNeighbor(outgoingRiverDirection).elevation
                )
            {
                RemoveOutgoingRiver();
            }
            if (
                hasIncomingRiver &&
                elevation > GetNeighbor(incomingRiverDirection).elevation
                )
            {
                RemoveIncomingRiver();
            }

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }

            Refresh();
		}
	}

    public Color Color
    {
        get
        {
            return hexColor;
        }
        set
        {
            if (hexColor == value)
            {
                return;
            }
            hexColor = value;
            Refresh();
        }
    }

	public Vector3 Position
	{
		get
		{
			return transform.localPosition;
		}
	}

    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiverDirection
    {
        get
        {
            return incomingRiverDirection;
        }
    }

    public HexDirection OutgoingRiverDirection
    {
        get
        {
            return outgoingRiverDirection;
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiverDirection : outgoingRiverDirection;
        }
    }

    public float StreamBedY
    {
        get
        {
            return (elevation + HexDefinition.streamBedElevationOffset) * HexDefinition.elevationStep;
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return (elevation + HexDefinition.waterElevationOffset) *
                HexDefinition.elevationStep;
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return (waterLevel + HexDefinition.waterElevationOffset) *
                HexDefinition.elevationStep;
        }
    }

    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            Refresh();
        }
    }

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }

	private void Awake()
	{
		ResetDjikstra();
		moveCost = Mathf.RoundToInt(Random.Range(1.0f, 4.0f));
		if (moveCost == 4)
		{
			moveCost = 100;
		}
		SetColor();
		SetIsOccupied(false);
	}

	public void SetColor()
	{
		switch (moveCost)
		{
			case 1:
				this.hexColor = Color.blue;
				break;
			case 2:
				this.hexColor = Color.green;
				break;
			case 3:
				this.hexColor = Color.yellow;
				break;
			case 100:
				this.hexColor = Color.red;
				break;
			default:
				this.hexColor = Color.black;
				break;
		}
	}

	public HexCell GetNeighbor(HexDirection direction)
	{
		return neighbors[(int)direction];
	}

	public HexCell[] GetNeighbors()
	{
		return neighbors;
	}

	public void SetNeighbor(HexDirection direction, HexCell cell)
	{
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public void SetCellID(int id)
	{
		cellID = id;
	}

	public int GetCellID()
	{
		return cellID;
	}

	public void SetDjikstraGrey()
	{
		color = DjikstraColor.grey;
	}

	public void SetDjikstraBlack()
	{
		color = DjikstraColor.black;
	}

	public void SetOrigin()
	{
		djikstraCost = 0;
		SetDjikstraGrey();
		parentCellID = cellID;
	}

	public int GetDjikstraCost()
	{
		return djikstraCost;
	}

	public void SetDjikstraCost(int cost)
	{
		djikstraCost = cost;
	}

	public DjikstraColor GetDjikstraColor()
	{
		return color;
	}

	public int GetMoveCost()
	{
		return moveCost;
	}

	public void ResetDjikstra()
	{
		color = DjikstraColor.white;
		djikstraCost = 9999;
		parentCellID = cellID;
	}

	public void SetParentCellID(int id)
	{
		parentCellID = id;
	}

	public int GetParentCellID()
	{
		return parentCellID;
	}

	public bool GetIsOccupied()
	{
		return isOccupied;
	}

	public void SetIsOccupied(bool isOccupied)
	{
		this.isOccupied = isOccupied;
	}

	public HexDefinition.HexEdgeType GetEdgeType(HexDirection direction)
	{
		return HexDefinition.GetEdgeType(elevation, neighbors[(int)direction].elevation);
	}

	public HexDefinition.HexEdgeType GetEdgeType(HexCell otherCell)
	{
		return HexDefinition.GetEdgeType(elevation, otherCell.elevation);
	}

    private void Refresh()
    {
        if(chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiverDirection);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiverDirection);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiverDirection == direction)
        {
            return;
        }

        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }

        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiverDirection == direction)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiverDirection = direction;
        //RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiverDirection = direction.Opposite();
        //neighbor.RefreshSelfOnly();

        SetRoad((int)direction, false);
    }

    private void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            hasIncomingRiver && incomingRiverDirection == direction ||
            hasOutgoingRiver && outgoingRiverDirection == direction;
    }

    public bool HasRoadThroughEdge (HexDirection direction)
    {
        return roads[(int)direction];
    }

    public void RemoveRoads()
    {
        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
            GetElevationDifference(direction) <= 1)
        {
            SetRoad((int)direction, true);
        }
    }

    public void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    public int GetElevationDifference (HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return difference >= 0 ? difference : -difference;
    }
}
