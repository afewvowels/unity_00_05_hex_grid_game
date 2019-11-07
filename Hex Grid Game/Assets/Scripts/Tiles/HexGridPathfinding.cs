using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridPathfinding : MonoBehaviour
{
	private HexGrid hexGrid;

	[SerializeField]
	private List<HexCell> pathHexes;

	[SerializeField]
	private List<HexCell> sortedPathHexes;

	private bool isFound;

	void Awake()
	{
		hexGrid = GetComponent<HexGrid>();
	}

	public void ResetPathing()
	{
		ResetSortedPathHexes();
		ResetIsFound();
		pathHexes = new List<HexCell>();
	}

	private void ResetSortedPathHexes()
	{
		sortedPathHexes = new List<HexCell>();
	}

	private void ResetIsFound()
	{
		isFound = false;
	}

	public void SetStartCell(int startCellID)
	{
		sortedPathHexes.Clear();
		hexGrid.GetHexCellByID(startCellID).SetOrigin();
		sortedPathHexes.Add(hexGrid.GetHexCellByID(startCellID));
	}

	public void FindPath(HexCell destination)
	{
		sortedPathHexes = sortedPathHexes.OrderBy(x => x.GetDjikstraCost()).ToList();

		HexCell activeCell = sortedPathHexes[0];
		sortedPathHexes.RemoveAt(0);

		activeCell.SetDjikstraBlack();
		foreach (HexCell neighbor in activeCell.GetNeighbors())
		{
			if (neighbor != null && neighbor.GetDjikstraColor() != HexCell.DjikstraColor.black)
			{
				int newCost = activeCell.GetDjikstraCost() + neighbor.GetMoveCost();

				if (newCost < neighbor.GetDjikstraCost() && !isFound && neighbor.GetDjikstraColor() != HexCell.DjikstraColor.black)
				{
					neighbor.SetDjikstraGrey();
					neighbor.SetDjikstraCost(newCost);
					neighbor.SetParentCellID(activeCell.GetCellID());
					if (neighbor.GetCellID() == destination.GetCellID())
					{
						isFound = true;
					}
					else
					{
						sortedPathHexes.Add(neighbor);
					}
				}
			}
		}

		if (!isFound)
		{
			FindPath(destination);
		}
	}

	public void BuildHexPathList(HexCell destination)
	{
		pathHexes = new List<HexCell>();

		pathHexes.Add(destination);

		HexCell tempCell = hexGrid.cells[destination.GetParentCellID()];

		pathHexes.Add(tempCell);

		while (tempCell.GetCellID() != tempCell.GetParentCellID())
		{
			tempCell = hexGrid.cells[tempCell.GetParentCellID()];
			pathHexes.Add(tempCell);
		}

		foreach (HexCell cell in pathHexes)
		{
			cell.hexColor = Color.white;
		}
		isFound = false;
		hexGrid.RedrawGrid();
	}

	public List<HexCell> GetHexPathList()
	{
		return pathHexes;
	}
}
