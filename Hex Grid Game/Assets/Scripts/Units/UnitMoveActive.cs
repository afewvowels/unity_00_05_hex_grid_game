using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMoveActive : MonoBehaviour
{
    private UnitActive unitActive;

    private HexGrid hexGrid;

    private HexGridPathfinding hexGridPathfinding;

    [SerializeField]
    private HexCell start;

    [SerializeField]
    private HexCell destination;

    [SerializeField]
    private List<HexCell> path;

    private bool isMoving;

    private void Awake()
    {
        GameObject hexBoard = GameObject.FindGameObjectWithTag("hexgrid");
        hexGrid = hexBoard.GetComponent<HexGrid>();
        hexGridPathfinding = hexBoard.GetComponent<HexGridPathfinding>();

        unitActive = this.GetComponent<UnitActive>();

        path = new List<HexCell>();

        isMoving = false;
    }

    private void FixedUpdate()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0) && unitActive.GetActiveUnit() != null && Physics.Raycast(inputRay, out hit) && !isMoving)
        {
            start = hexGrid.GetHexCellByID(unitActive.GetStartID());
            destination = hexGrid.GetClickedCell(hit.point);
            if (start != destination)
            {
                MoveActiveUnit();
            }
        }
        else if (isMoving)
        {
            MoveUnit();
        }
    }

    private void MoveActiveUnit()
    {
        hexGridPathfinding.SetStartCell(start);
        hexGridPathfinding.FindPath(destination);
        hexGridPathfinding.BuildHexPathList(destination);
        path = hexGridPathfinding.GetHexPathList();
        isMoving = true;
    }

    private void MoveUnit()
    {
        GameObject unit = unitActive.GetActiveUnit();

        if (path.Count > 0)
        {
            HexCell target = path[path.Count - 1];

            unit.transform.LookAt(target.transform.position);

            float distance = Vector3.Distance(unit.transform.position, target.transform.position);

            if (Mathf.Abs(distance) > 0.5f)
            {
                unit.transform.position += unit.transform.forward * 0.5f;
            }
            else if (Mathf.Abs(distance) <= 0.5f)
            {
                path.RemoveAt(path.Count - 1);
            }
        }
        else
        {
            unit.GetComponent<UnitInfo>().SetCurrentHexID(destination.GetCellID());
            unitActive.UpdatePathIDs();
            hexGrid.ResetGrid();
            hexGridPathfinding.ResetPathing();
            start.SetIsOccupied(false);
            destination.SetIsOccupied(true);
            isMoving = false;
        }
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }
}
