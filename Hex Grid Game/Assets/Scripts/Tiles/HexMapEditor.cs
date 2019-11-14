using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class HexMapEditor : MonoBehaviour
{
	public HexGrid hexGrid;

	public GameObject player;
    public GameObject unitsRoot;

    public int activeElevation;
    public int activeWaterLevel;
    private int activeUrbanLevel;
    private int activeFarmLevel;
    private int activePlantLevel;
    private int activeSpecialIndex;

    private bool applyElevation = true;
    private bool applyWaterLevel = true;
    private bool applyUrbanLevel;
    private bool applyFarmLevel;
    private bool applyPlantLevel;
    private bool applySpecialIndex;
    private bool isDrag;

    private HexDirection dragDirection;

    private HexCell previousCell;

    private int brushSize;

    private int activeTerrainTypeIndex;

    public Material terrainMaterial;

    private enum OptionalToggle
    {
        Ignore, Yes, No
    }

    private OptionalToggle riverMode, roadMode, walledMode;

    private void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
        Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        SetEditMode(true);
    }

    private void FixedUpdate()
	{
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
            {
                HandleInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    CreateUnit();
                }
                return;
            }
        }
        previousCell = null;
	}

    public void ShowGrid (bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    HexCell GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

	void HandleInput()
    {
        HexCell currentCell = GetCellUnderCursor();

        if (currentCell)
		{
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);

            previousCell = currentCell;
		}
        else
        {
            previousCell = null;
        }
	}

    private void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetHexCell(new HexCoordinates(x, z)));
            }
        }

        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetHexCell(new HexCoordinates(x, z)));
            }
        }
    }

    private void EditCell(HexCell cell)
    {
        if(cell)
        {
            if (activeTerrainTypeIndex >= 0)
            {
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }

            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }

            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }

            if (applySpecialIndex)
            {
                cell.SpecialIndex = activeSpecialIndex;
            }

            if (applyUrbanLevel)
            {
                cell.UrbanLevel = activeUrbanLevel;
            }

            if (applyFarmLevel)
            {
                cell.FarmLevel = activeFarmLevel;
            }

            if (applyPlantLevel)
            {
                cell.PlanetLevel = activePlantLevel;
            }

            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }

            if (roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }

            if (walledMode != OptionalToggle.Ignore)
            {
                cell.Walled = walledMode == OptionalToggle.Yes;
            }

            else if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    private void ValidateDrag(HexCell currentCell)
    {
        for (
            dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++
            )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }

        isDrag = false;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetEditMode (bool toggle)
    {
        enabled = toggle;
    }

    private void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit)
        {
            hexGrid.AddUnit(Instantiate(HexUnit.unitPrefab), cell, Random.Range(0.0f, 360.0f));
        }
    }

    private void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }

    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetUrbanLevel (float level)
    {
        activeUrbanLevel = (int)level;
    }

    public void SetApplyFarmLevel (bool toggle)
    {
        applyFarmLevel = toggle;
    }

    public void SetFarmLevel (float level)
    {
        activeFarmLevel = (int)level;
    }

    public void SetApplyPlantLevel (bool toggle)
    {
        applyPlantLevel = toggle;
    }

    public void SetPlantLevel (float level)
    {
        activePlantLevel = (int)level;
    }

    public void SetWalledMode (int mode)
    {
        walledMode = (OptionalToggle)mode;
    }

    public void SetApplySpecialIndex(bool toggle)
    {
        applySpecialIndex = toggle;
    }

    public void SetSpecialIndex(float index)
    {
        activeSpecialIndex = (int)index;
    }
}
