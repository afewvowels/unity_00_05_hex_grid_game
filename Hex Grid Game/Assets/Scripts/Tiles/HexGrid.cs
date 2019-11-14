using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public int chunkCountX;
    public int chunkCountZ;

    public int cellCountX;
    public int cellCountZ;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	public Canvas gridCanvas;
	private bool isFound;

	public HexCell[] cells;
	public HexMesh hexMesh;

	public List<HexCell> sortedHexes;
	public List<HexCell> path;

	public List<int> pathIndexes;

	public GameObject player;
    public GameObject unitsRoot;

    public Texture2D noiseSource;

    public HexGridChunk chunkPrefab;
    public HexGridChunk[] chunks;

    private HexCellPriorityQueue searchFringe;
    private int searchFringePhase;

    private HexCell currentPathFrom, currentPathTo;

    [SerializeField]
    private bool currentPathExists;

    [SerializeField]
    private List<HexUnit> units = new List<HexUnit>();

    public HexUnit unitPrefab;

    private HexCellShaderData cellShaderData;

    public HexCellShaderData ShaderData { get; set; }

    public int seed;

    public bool wrapping;

    private Transform[] columns;

    private int currentCenterColumnIndex = -1;

    public bool HasPath
    {
        get
        {
            return currentPathExists;
        }
    }

	private void Awake()
	{
		HexDefinition.noiseSource = noiseSource;
        HexDefinition.InitializeHashGrid(seed);
        HexUnit.unitPrefab = unitPrefab;
        cellShaderData = gameObject.AddComponent<HexCellShaderData>();
        cellShaderData.Grid = this;
        CreateMap(cellCountX, cellCountZ, wrapping);
	}

	private void Start()
	{
		isFound = false;
		sortedHexes = new List<HexCell>();
		path = new List<HexCell>();
		pathIndexes = new List<int>();
	}

	private void OnEnable()
	{
        if(!HexDefinition.noiseSource)
        {
            HexDefinition.noiseSource = noiseSource;
            HexDefinition.InitializeHashGrid(seed);
            HexUnit.unitPrefab = unitPrefab;
            HexDefinition.wrapSize = wrapping ? cellCountX : 0;
            ResetVisibility();
        }
	}

    public bool CreateMap(int x, int z, bool wrapping)
    {
        if (
            x <= 0 || x % HexDefinition.chunkSizeX != 0 ||
            z <= 0 || z % HexDefinition.chunkSizeZ != 0
            )
        {
            Debug.Log("Unsupported map size.");
            return false;
        }

        cellCountX = x;
        cellCountZ = z;
        this.wrapping = wrapping;
        currentCenterColumnIndex = -1;
        HexDefinition.wrapSize = wrapping ? cellCountX : 0;

        ClearPath();
        ClearUnits();

        if (columns != null)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                Destroy(columns[i].gameObject);
            }
        }

        InitializeHexGrid();
        return true;
    }

    public HexCell GetCellByOffset(int xOffset, int zOffset)
    {
        return cells[xOffset + zOffset * cellCountX];
    }

	public HexCell GetHexCellByID(int id)
	{
		return cells[id];
	}

	public HexCell GetRandomHexCell()
	{
		return cells[Mathf.RoundToInt(Random.Range(0.0f, cellCountX * cellCountZ - 1.0f))];
	}

    public HexCell GetHexCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;
        if (z < 0 || z >= cellCountZ)
        {
            return null;
        }

        int x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX)
        {
            return null;
        }

        return cells[x + z * cellCountX];
    }

	public void InitializeHexGrid()
	{
        chunkCountX = cellCountX / HexDefinition.chunkSizeX;
        chunkCountZ = cellCountZ / HexDefinition.chunkSizeZ;

        cellShaderData.Initialize(cellCountX, cellCountZ);

        CreateChunks();
        CreateCells();
	}

    void CreateChunks()
    {
        columns = new Transform[chunkCountX];
        for (int x = 0; x < chunkCountX; x++)
        {
            columns[x] = new GameObject("Column").transform;
            columns[x].SetParent(transform, false);
        }

        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(columns[x], false);
            }
        }
    }

    void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

	void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * HexDefinition.innerDiameter;
		position.y = 0.0f;
		position.z = z * (HexDefinition.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.ShaderData = cellShaderData;
        cell.Index = i;
        cell.ColumnIndex = x / HexDefinition.chunkSizeX;
        cell.ShaderData = cellShaderData;

        if (wrapping)
        {
            cell.Explorable = z > 0 && z < cellCountZ - 1;
        }
        else
        {
            cell.Explorable = x > 0 && z > 0 && x < (cellCountX - 1) && z < (cellCountZ - 1);
        }

		cell.SetCellID(i);

        if (x > 0)
		{
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            if (wrapping && x == cellCountX - 1)
            {
                cell.SetNeighbor(HexDirection.E, cells[i - x]);
            }
		}
		if (z > 0)
		{
			if ((z & 1) == 0)
			{
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0)
				{
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
                else if (wrapping)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
                }
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
                else if (wrapping)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX * 2 + 1]);
                }
			}
        }

        CreateLabel(x, z, position, cell);

        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
	}

    void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexDefinition.chunkSizeX;
        int chunkZ = z / HexDefinition.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexDefinition.chunkSizeX;
        int localZ = z - chunkZ * HexDefinition.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexDefinition.chunkSizeX, cell);
    }

	void CreateLabel(int x, int z, Vector3 position, HexCell cell)
	{
		Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);

        cell.uiRect = label.rectTransform;

        label.text = null;

        label.tag = "label";
    }

	public void ColorCell(Vector3 position, Color color)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		HexCell cell = cells[index];
		//cell.Color = color;
		//foreach (HexCell adjCell in cell.GetNeighbors())
		//{
		//	adjCell.Color = color;
		//}
		//hexMesh.Triangulate(cells);
		Debug.Log("touched at " + coordinates.ToString());
	}

    public HexCell GetCell(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return GetClickedCell(hit.point);
        }
        return null;
    }

	public HexCell GetClickedCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        //int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        //HexCell cell = cells[index];
        //return cell;
        return GetHexCell(coordinates);
	}

	public void MoveObject(Vector3 position, GameObject go)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		HexCell cell = cells[index];
		go.transform.position = cell.transform.position;
		Debug.Log("moved");
	}

	public void FindPath(HexCell destination)
	{
		int index = 0;
		sortedHexes = sortedHexes.OrderBy(x => x.GetDjikstraCost()).ToList();

		HexCell activeCell = sortedHexes[index];
		sortedHexes.RemoveAt(index);

		Debug.Log("cell id: " + activeCell.GetCellID() + ", djikstra cost: " + activeCell.GetDjikstraCost());
		activeCell.SetDjikstraBlack();
		foreach (HexCell neighbor in activeCell.GetNeighbors())
		{
			if (neighbor != null && neighbor.GetDjikstraColor() != HexCell.DjikstraColor.black)
			{
				int newCost = activeCell.GetDjikstraCost() + neighbor.GetMoveCost();

				if (newCost < neighbor.GetDjikstraCost() && !isFound && neighbor.GetDjikstraColor() != HexCell.DjikstraColor.black)
				{
					neighbor.SetDjikstraGrey();
					neighbor.SetDjikstraCost(newCost);
					neighbor.SetParentCellID(activeCell.GetCellID());
					//CreateLabel(0, 0, neighbor.transform.position, neighbor);
					if (neighbor.GetCellID() == destination.GetCellID())
					{
						BuildArray(destination);
						FollowPath();
						isFound = true;
					}
					else
					{
						sortedHexes.Add(neighbor);
					}
				}
			}
		}

		//RedrawGrid();

		if (!isFound)
		{
			FindPath(destination);
		}
	}

	public void RedrawGrid()
	{
		//hexMesh.Triangulate(cells);
	}

	public void BuildArray(HexCell destination)
	{
		Debug.Log("build array");
		path.Clear();
		pathIndexes.Clear();
		path.Add(destination);
		pathIndexes.Add(destination.GetCellID());

		HexCell tempCell = cells[destination.GetParentCellID()];

		path.Add(tempCell);
		pathIndexes.Add(tempCell.GetCellID());

		while (tempCell.GetCellID() != tempCell.GetParentCellID())
		{
			tempCell = cells[tempCell.GetParentCellID()];
			path.Add(tempCell);
			pathIndexes.Add(tempCell.GetCellID());
		}
		Debug.Log("finished building array");
	}

	public void FollowPath()
	{
		ColorPath();
	}

	public void ColorPath()
	{
		for (int i = 0; i < path.Count - 1; i++)
		{
			//cells[pathIndexes[i]].Color = Color.white;
		}
		//hexMesh.Triangulate(cells);
	}

	public List<int> GetPathIndexes()
	{
		return pathIndexes;
	}

	public void ResetGrid()
	{
		foreach (HexCell cell in cells)
		{
			cell.ResetDjikstra();
		}
		sortedHexes.Clear();
		path.Clear();
		pathIndexes.Clear();
	}

    public float GetSizeX()
    {
        return cellCountX * HexDefinition.innerRadius;
    }

    public float GetSizeZ()
    {
        return cellCountZ * HexDefinition.outerRadius;
    }

    public void Save (BinaryWriter writer)
    {
        writer.Write(cellCountX);
        writer.Write(cellCountZ);
        writer.Write(wrapping);
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Save(writer);
        }

        writer.Write(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader, int header)
    {
        ClearPath();
        ClearUnits();

        int x = 20;
        int z = 15;

        if (header >= 1)
        {
            x = reader.ReadInt32();
            z = reader.ReadInt32();
        }

        bool wrapping = header >= 5 ? reader.ReadBoolean() : false;

        if (x != cellCountX || z != cellCountZ || this.wrapping != wrapping)
        {
            if (!CreateMap(x, z, wrapping))
            {
                return;
            }
        }

        bool originalImmediateMode = cellShaderData.ImmediateMode;
        cellShaderData.ImmediateMode = true;

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Load(reader, header);
        }

        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Refresh();
        }

        if (header >= 2)
        {
            int unitCount = reader.ReadInt32();
            for (int i = 0; i < unitCount; i++)
            {
                HexUnit.Load(reader, this);
            }
        }

        cellShaderData.ImmediateMode = originalImmediateMode;
    }

    public void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists = Search(fromCell, toCell, speed);
        ShowPath(speed);
    }

    bool Search (HexCell fromCell, HexCell toCell, int speed)
    {
        searchFringePhase += 2;
        if (searchFringe == null)
        {
            searchFringe = new HexCellPriorityQueue();
        }
        else
        {
            searchFringe.Clear();
        }

        fromCell.EnableHighlight(Color.blue);

        fromCell.SearchPhase = searchFringePhase;
        fromCell.Distance = 0;
        searchFringe.Enqueue(fromCell);

        while (searchFringe.Count > 0)
        {
            HexCell current = searchFringe.Dequeue();
            current.SearchPhase += 1;

            if (current == toCell)
            {
                return true;
            }

            int currentTurn = (current.Distance - 1) / speed;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.SearchPhase > searchFringePhase)
                {
                    continue;
                }
                if (neighbor.IsUnderwater || neighbor.Unit)
                {
                    continue;
                }

                HexDefinition.HexEdgeType edgeType = current.GetEdgeType(neighbor);

                if (edgeType == HexDefinition.HexEdgeType.Cliff)
                {
                    continue;
                }

                int moveCost;

                if (current.HasRoadThroughEdge(d))
                {
                    moveCost = 1;
                }
                else
                {
                    moveCost = edgeType == HexDefinition.HexEdgeType.Flat ? 5 : 10;
                }

                int distance = current.Distance + moveCost;
                int turn = (distance - 1) / speed;
                if (turn > currentTurn)
                {
                    distance = turn * speed + moveCost;
                }

                if (neighbor.SearchPhase < searchFringePhase)
                {
                    neighbor.SearchPhase = searchFringePhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                    searchFringe.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFringe.Change(neighbor, oldPriority);
                }
            }
        }
        return false;
    }

    private void ShowPath (int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                int turn = (current.Distance - 1) / speed;
                current.SetLabel(turn.ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
        }
        currentPathFrom.EnableHighlight(Color.blue);
        currentPathTo.EnableHighlight(Color.red);
    }

    public void ClearPath()
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.DisableHighlight();
            currentPathExists = false;
        }
        else if (currentPathFrom)
        {
            currentPathFrom.DisableHighlight();
            currentPathTo.DisableHighlight();
        }
        currentPathFrom = currentPathTo = null;
    }

    private void ClearUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Die();
        }
        units.Clear();
    }

    public void AddUnit (HexUnit unit, HexCell location, float orientation)
    {
        units.Add(unit);
        unit.Grid = this;
        unit.Location = location;
        unit.Orientation = orientation;
    }

    public void RemoveUnit(HexUnit unit)
    {
        units.Remove(unit);
        unit.Die();
    }

    public void ShowUI (bool visible)
    {
        for (int i = 0; i < chunks.Count(); i++)
        {
            chunks[i].ShowUI(visible);
        }
    }

    public List<HexCell> GetPath()
    {
        if (!currentPathExists)
        {
            return null;
        }
        List<HexCell> currentPath = ListPool<HexCell>.Get();
        for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
        {
            currentPath.Add(c);
        }
        currentPath.Add(currentPathFrom);
        currentPath.Reverse();
        return currentPath;
    }

    private List<HexCell> GetVisibleCells (HexCell fromCell, int range)
    {
        List<HexCell> visibleCells = ListPool<HexCell>.Get();

        searchFringePhase += 2;
        if(searchFringe == null)
        {
            searchFringe = new HexCellPriorityQueue();
        }
        else
        {
            searchFringe.Clear();
        }

        range += fromCell.ViewElevation;
        fromCell.SearchPhase = searchFringePhase;
        fromCell.Distance = 0;
        searchFringe.Enqueue(fromCell);

        HexCoordinates fromCoordinates = fromCell.coordinates;
        while(searchFringe.Count > 0)
        {
            HexCell current = searchFringe.Dequeue();
            current.SearchPhase += 1;
            visibleCells.Add(current);

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null ||
                    neighbor.SearchPhase > searchFringePhase ||
                    !neighbor.Explorable)
                {
                    continue;
                }

                int distance = current.Distance + 1;
                if (distance + neighbor.ViewElevation > range ||
                    distance > fromCoordinates.DistanceTo(neighbor.coordinates))
                {
                    continue;
                }
                if (distance > range)
                {
                    continue;
                }

                if (neighbor.SearchPhase < searchFringePhase)
                {
                    neighbor.SearchPhase = searchFringePhase;
                    neighbor.Distance = distance;
                    neighbor.SearchHeuristic = 0;
                    searchFringe.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    searchFringe.Change(neighbor, oldPriority);
                }
            }
        }

        return visibleCells;
    }

    public void IncreaseVisibility (HexCell fromCell, int range)
    {
        List<HexCell> visibleCells = GetVisibleCells(fromCell, range);
        for (int i = 0; i < visibleCells.Count; i++)
        {
            visibleCells[i].IncreaseVisibility();
        }
        ListPool<HexCell>.Add(visibleCells);
    }

    public void DecreaseVisibility (HexCell fromCell, int range)
    {
        List<HexCell> visibleCells = GetVisibleCells(fromCell, range);
        for (int i = 0; i < visibleCells.Count; i++)
        {
            visibleCells[i].DecreaseVisibility();
        }
        ListPool<HexCell>.Add(visibleCells);
    }

    public void ResetVisibility()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].ResetVisibility();
        }
        for (int i = 0; i < units.Count; i++)
        {
            HexUnit unit = units[i];
            IncreaseVisibility(unit.Location, unit.VisionRange);
        }
    }

    public void CenterMap(float xPosition)
    {
        int centerColumnIndex = (int)(xPosition / (HexDefinition.innerDiameter * HexDefinition.chunkSizeX));

        int minColumnIndex = centerColumnIndex - chunkCountX / 2;
        int maxColumnIndex = centerColumnIndex + chunkCountX / 2;

        Vector3 position;
        position.y = position.z = 0.0f;
        for (int i = 0; i < columns.Length; i++)
        {
            if (i < minColumnIndex)
            {
                position.x = chunkCountX * (HexDefinition.innerDiameter * HexDefinition.chunkSizeX);
            }
            else if (i > maxColumnIndex)
            {
                position.x = chunkCountX * -(HexDefinition.innerDiameter * HexDefinition.chunkSizeX);
            }
            else
            {
                position.x = 0.0f;
            }
            columns[i].localPosition = position;
        }


        if (centerColumnIndex == currentCenterColumnIndex)
        {
            return;
        }
        currentCenterColumnIndex = centerColumnIndex;
    }

    public void MakeChildOfColumn (Transform child, int columnIndex)
    {
        child.SetParent(columns[columnIndex], false);
    }
}
