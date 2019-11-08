using UnityEngine;

public class UnitActive : MonoBehaviour
{
	[SerializeField]
	private GameObject activeUnit;

	public void SetActiveUnit(GameObject unit)
	{
		activeUnit = unit;
	}

	public GameObject GetActiveUnit()
	{
		return activeUnit;
	}
}
