using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridPathfinding : MonoBehaviour
{
	private HexGrid hexGrid;

    [SerializeField]
    private HexCell start, destination;

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

	public void SetStartCell(HexCell start)
	{
		sortedPathHexes.Clear();
		start.SetOrigin();
		sortedPathHexes.Add(start);
	}

	public void FindPath(HexCell destination)
	{
		// Sort the fringe
		sortedPathHexes = sortedPathHexes.OrderBy(x => x.GetDjikstraCost()).ToList();

		// Get the fringe element with lowest cost
		HexCell activeCell = sortedPathHexes[0];
		// Remove that from the fringe
		sortedPathHexes.RemoveAt(0);

		// Set the cell we're looking at to black
		activeCell.SetDjikstraBlack();

		// Look at each neighbor
		foreach (HexCell neighbor in activeCell.GetNeighbors())
		{
			// If the neighbor exists (we could be at the edge of the game board) && we haven't already been there
			if (neighbor != null && neighbor.GetDjikstraColor() != HexCell.DjikstraColor.black && !neighbor.GetIsOccupied())
			{
				// Calculate the potential new cost of the neighbor
				int newCost = activeCell.GetDjikstraCost() + neighbor.GetMoveCost();

				// If the neighbor's Djikstra cost is > than the new cost + other conditions
				if (newCost < neighbor.GetDjikstraCost() && !isFound && neighbor.GetDjikstraColor() != HexCell.DjikstraColor.black)
				{
					// Add the neighbor to the fringe
					neighbor.SetDjikstraGrey();
					// Set the neighbor's Djikstra cost
					neighbor.SetDjikstraCost(newCost);
					// Link the neighbor to this active cell
					neighbor.SetParentCellID(activeCell.GetCellID());
					// Are we there yet?
					if (neighbor.GetCellID() == destination.GetCellID())
					{
						isFound = true;
					}
					else
					{
						// Add the neighbor to the finge list
						sortedPathHexes.Add(neighbor);
					}
				}
			}
		}

		// Recurse (?)
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
		}
		isFound = false;
		hexGrid.RedrawGrid();
	}

	public List<HexCell> GetHexPathList()
	{
		return pathHexes;
	}
}
