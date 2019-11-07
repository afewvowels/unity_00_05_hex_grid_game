using UnityEngine;

public class UnitSpawnRandom : MonoBehaviour
{
	public GameObject go;

	private HexGrid hexGrid;

	// Start is called before the first frame update
	void Start()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>();
		HexCell location = hexGrid.GetRandomHexCell();
		go.GetComponent<UnitInfo>().SetCurrentHexID(location.GetCellID());
		go.GetComponent<UnitInfo>().SetDestinationHexID(location.GetCellID());
		Instantiate(go, location.transform.position, new Quaternion(0.0f, Random.Range(0.0f, 360.0f), 0.0f, 0.0f));
	}
}
