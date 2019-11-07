using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
	private bool isMoving;
	private bool isDestinationReached;
	public bool hasDestination;

	private HexCell hexDestination;

	private HexGrid hexGridLocal;

	private HexCell start;

	public List<int> moves;
	// Start is called before the first frame update
	void Start()
	{
		hexGridLocal = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		start = hexGridLocal.GetRandomHexCell();
		start.SetOrigin();
		this.transform.position = start.transform.position;

		isMoving = false;
		isDestinationReached = false;
		hasDestination = false;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleInput();
		}

		if (hasDestination)
		{
			MoveThisObject();
		}
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (!hasDestination && Physics.Raycast(inputRay, out hit))
		{
			//FollowPath(hexGridLocal.GetClickedCell(hit.point));
			hexDestination = hexGridLocal.GetClickedCell(hit.point);
			hasDestination = true;
			hexGridLocal.sortedHexes.Add(start);
			hexGridLocal.FindPath(hexDestination);
			moves = hexGridLocal.GetPathIndexes();
			if (moves.Count != 0 && moves[moves.Count - 1] == 0)
			{
				moves.RemoveAt(moves.Count - 1);
			}
			else
			{
				Debug.Log("error moves is 0. Start: " + start.GetCellID().ToString() + ", Dest: " + hexDestination.GetCellID().ToString());
			}
			start = hexDestination;
		}
	}

	void FollowPath(HexCell destination)
	{
		hexGridLocal.ResetGrid();

		start.SetOrigin();

		hexGridLocal.sortedHexes.Clear();

		hexGridLocal.sortedHexes.Add(start);

		hexGridLocal.FindPath(destination);
		List<int> indexes = hexGridLocal.GetPathIndexes();
	}

	void MoveThisObject()
	{
		if (moves.Count != 0)
		{
			HexCell destination = hexGridLocal.GetHexCellByID(moves[moves.Count - 1]);

			this.transform.LookAt(destination.transform.position);

			if (Mathf.Abs(Vector3.Distance(this.transform.position, destination.transform.position)) > 0.25f)
			{
				this.transform.position += this.transform.forward * 0.5f;
			}
			else
			{
				moves.RemoveAt(moves.Count - 1);
			}
		}
		else
		{
			hasDestination = false;
			hexGridLocal.ResetGrid();
			start.SetOrigin();
		}
	}
}
