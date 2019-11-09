using UnityEngine;

public class UnitSpawnRandom : MonoBehaviour
{
	public GameObject go;

	private HexGrid hexGrid;

	private UnitList unitList;

	void Start()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		unitList = this.GetComponent<UnitList>();
	}

	public void RandomSpawnUnit()
	{
		HexCell location = hexGrid.GetRandomHexCell();
		if (!location.GetIsOccupied())
		{
			go.GetComponent<UnitInfo>().SetCurrentHexID(location.GetCellID());
			go.GetComponent<UnitInfo>().SetDestinationHexID(location.GetCellID());
			location.SetIsOccupied(true);
			GameObject temp = (GameObject)Instantiate(go, location.transform.position, new Quaternion(0.0f, Random.Range(0.0f, 360.0f), 0.0f, 0.0f));
			unitList.AddUnit(temp);
			hexGrid.GetHexCellByID(go.GetComponent<UnitInfo>().GetCurrentHexID()).SetIsOccupied(true);
		}
	}
}
