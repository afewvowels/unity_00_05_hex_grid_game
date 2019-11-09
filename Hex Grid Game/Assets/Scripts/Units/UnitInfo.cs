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

    [SerializeField]
    private string player;

	public GameObject ring;
	private GameObject ringInstance;

	private void Start()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		hexGridPathfinding = hexGrid.GetComponent<HexGridPathfinding>();
		SetIsActive(false);
		movePoints = 10;
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
