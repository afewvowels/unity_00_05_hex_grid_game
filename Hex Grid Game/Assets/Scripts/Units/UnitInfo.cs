using System.Collections.Generic;
using UnityEngine;

public class UnitInfo : MonoBehaviour
{
	[SerializeField]
	private int currentHexID;

	[SerializeField]
	private int destinationHexID;

	private HexGrid hexGrid;
	private HexGridPathfinding hexGridPathfinding;

	[SerializeField]
	private List<HexCell> path;

	private void Start()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		hexGridPathfinding = hexGrid.GetComponent<HexGridPathfinding>();
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleClick();
		}
		if (currentHexID != destinationHexID)
		{
			MoveUnit();
		}
	}

	public void HandleClick()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (currentHexID == destinationHexID && Physics.Raycast(inputRay, out hit))
		{
			HexCell destination = hexGrid.GetClickedCell(hit.point);
			hexGridPathfinding.SetStartCell(currentHexID);
			hexGridPathfinding.FindPath(destination);
			hexGridPathfinding.BuildHexPathList(destination);
			destinationHexID = destination.GetCellID();
			path = hexGridPathfinding.GetHexPathList();
		}
	}

	public int GetCurrentHexID()
	{
		return currentHexID;
	}

	public void SetCurrentHexID(int id)
	{
		currentHexID = id;
	}

	public int GetDestinationHexID()
	{
		return destinationHexID;
	}

	public void SetDestinationHexID(int id)
	{
		destinationHexID = id;
	}

	private void MoveUnit()
	{
		if (path.Count > 0)
		{
			HexCell target = path[path.Count - 1];

			this.transform.LookAt(target.transform.position);

			float distance = Vector3.Distance(this.transform.position, target.transform.position);

			if (Mathf.Abs(distance) > 0.5f)
			{
				this.transform.position += this.transform.forward * 0.5f;
			}
			else if (Mathf.Abs(distance) <= 0.5f)
			{
				path.RemoveAt(path.Count - 1);
			}
		}
		else
		{
			currentHexID = destinationHexID;
			hexGrid.ResetGrid();
		}
	}
}
