using System.Collections.Generic;
using UnityEngine;

public class UnitInfo : MonoBehaviour
{
	[SerializeField]
	private bool isActive;

	[SerializeField]
	private int currentHexID;

	[SerializeField]
	private int destinationHexID;

	private HexGrid hexGrid;
	private HexGridPathfinding hexGridPathfinding;

	[SerializeField]
	private List<HexCell> path;

	[SerializeField]
	private int movePoints;


	public GameObject ring;
	private GameObject ringInstance;

	private void Start()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		hexGridPathfinding = hexGrid.GetComponent<HexGridPathfinding>();
		SetIsActive(false);
		movePoints = 10;
	}

	private void FixedUpdate()
	{
		if (isActive && Input.GetMouseButtonDown(0))
		{
			SelectDestination();
		}
		else if (currentHexID != destinationHexID)
		{
			MoveUnit();
		}
	}

	public void SelectDestination()
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

	public bool GetIsActive()
	{
		return isActive;
	}

	public void SetIsActive(bool isActive)
	{
		this.isActive = isActive;

		if (this.isActive)
		{
			SelectUnit();
		}
		else if (!this.isActive)
		{
			DeselectUnit();
		}
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

	public void SelectUnit()
	{
		Vector3 pos = this.transform.position + new Vector3(0.0f, 2.0f, 0.0f);
		ringInstance = (GameObject)Instantiate(ring, pos, this.transform.rotation);
		ringInstance.transform.SetParent(this.transform);
		GameObject.FindGameObjectWithTag("unitsroot").GetComponent<UnitActive>().SetActiveUnit(this.gameObject);
	}

	public void DeselectUnit()
	{
		Destroy(ringInstance);
	}

	public void ResetMovePoints()
	{
		movePoints = 10;
	}
}
