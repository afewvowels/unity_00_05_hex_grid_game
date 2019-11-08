using System.Collections.Generic;
using UnityEngine;

public class UnitList : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> units;

	private List<GameObject> unitsWithActions;

	void Start()
	{
		InitializeLists();
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void InitializeLists()
	{
		units = new List<GameObject>();
		unitsWithActions = new List<GameObject>();
	}

	public void SetAllUnitsInactive()
	{
		foreach (GameObject unit in units)
		{
			if (unit.GetComponent<UnitInfo>().GetIsActive())
			{
				unit.GetComponent<UnitInfo>().SetIsActive(false);
			}
		}
	}

	public List<GameObject> GetUnits()
	{
		return units;
	}

	public GameObject GetUnitWithAction()
	{
		if (unitsWithActions.Count > 0)
		{
			GameObject unit = unitsWithActions[0];
			unitsWithActions.RemoveAt(0);
			return unit;
		}

		return null;
	}

	public List<GameObject> GetUnitsWithAction()
	{
		return unitsWithActions;
	}

	public void AddUnit(GameObject unit)
	{
		units.Add(unit);
	}
}
