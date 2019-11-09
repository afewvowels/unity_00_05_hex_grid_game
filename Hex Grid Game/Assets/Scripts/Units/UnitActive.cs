using UnityEngine;

public class UnitActive : MonoBehaviour
{
	[SerializeField]
	private GameObject activeUnit;

    [SerializeField]
    private int startCellID;

    [SerializeField]
    private int destinationCellID;

	public void SetActiveUnit(GameObject unit)
	{
		activeUnit = unit;
        UpdatePathIDs();
	}

	public GameObject GetActiveUnit()
	{
		return activeUnit;
	}

    public void UpdatePathIDs()
    {
        startCellID = activeUnit.GetComponent<UnitInfo>().GetCurrentHexID();
        destinationCellID = activeUnit.GetComponent<UnitInfo>().GetDestinationHexID();
    }

    public int GetStartID()
    {
        return startCellID;
    }

    public int GetDestinationID()
    {
        return destinationCellID;
    }
}
