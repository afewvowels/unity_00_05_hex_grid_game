﻿using UnityEngine;

public class HexCell : MonoBehaviour
{
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
    private int elevation;

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexDefinition.elevationStep;
            transform.localPosition = position;
        }
    }

	public enum DjikstraColor { white, grey, black };

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
}
