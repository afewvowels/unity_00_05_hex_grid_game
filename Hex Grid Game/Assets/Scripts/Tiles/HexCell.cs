using UnityEngine;

public class HexCell : MonoBehaviour
{
	public HexCoordinates coordinates;

	public Color hexColor;

	[SerializeField]
	HexCell[] neighbors;

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
}
