using UnityEngine;

public class UnitSelect : MonoBehaviour
{
	private HexGrid hexGrid;

	private UnitList unitList;

	private void Awake()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		unitList = this.GetComponent<UnitList>();
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleInput();
		}
	}

	private void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			if (hit.collider.gameObject.CompareTag("player"))
			{
				Debug.Log("clicked object");
				foreach (GameObject unit in unitList.GetUnits())
				{
					if (unit.GetComponent<UnitInfo>().GetIsActive())
					{
						unit.GetComponent<UnitInfo>().SetIsActive(false);
					}
				}
				hit.collider.gameObject.GetComponent<UnitInfo>().SetIsActive(true);
			}
		}
	}
}
