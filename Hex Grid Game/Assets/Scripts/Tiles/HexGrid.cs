using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    //public int width;
    //public int height;

    public int chunkCountX = 4;
    public int chunkCountZ = 3;

    [SerializeField]
    private int cellCountX;
    [SerializeField]
    private int cellCountZ;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	public Color defaultColor = Color.white;

	private Canvas gridCanvas;
	private bool isFound;

	public HexCell[] cells;
	public HexMesh hexMesh;

	public List<HexCell> sortedHexes;
	public List<HexCell> path;

	public List<int> pathIndexes;

	public GameObject player;

	public Texture2D noiseSource;

    public HexGridChunk chunkPrefab;
    public HexGridChunk[] chunks;

	private void Awake()
	{
		HexDefinition.noiseSource = noiseSource;
		InitializeHexGrid();
	}

	private void Start()
	{
		//hexMesh.Triangulate(cells);
		isFound = false;
		sortedHexes = new List<HexCell>();
		path = new List<HexCell>();
		pathIndexes = new List<int>();
		//Instantiate(player, this.GetRandomHexCell().transform.position, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
	}

	private void OnEnable()
	{
		HexDefinition.noiseSource = noiseSource;
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
        cellCountX = chunkCountX * HexDefinition.chunkSizeX;
        cellCountZ = chunkCountZ * HexDefinition.chunkSizeZ;

		//gridCanvas = GetComponentInChildren<Canvas>();
		//hexMesh = GetComponentInChildren<HexMesh>();

        CreateChunks();
        CreateCells();
	}

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
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
		position.x = (x + z * 0.5f - z / 2) * (HexDefinition.innerRadius * 2.0f);
		position.y = 0.0f;
		position.z = z * (HexDefinition.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);

		//cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.SetCellID(i);

		if (x > 0)
		{
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
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
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}

		cell.Elevation = 0;

        //CreateLabel(x, z, position, cell);

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

		//Paint a label showing coordinates
		//label.text = cell.coordinates.ToStringOnSeparateLines();

		label.tag = "label";
		label.text = cell.GetDjikstraCost().ToString();
	}

	public void ColorCell(Vector3 position, Color color)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.hexColor = color;
		foreach (HexCell adjCell in cell.GetNeighbors())
		{
			adjCell.hexColor = color;
		}
		//hexMesh.Triangulate(cells);
		Debug.Log("touched at " + coordinates.ToString());
	}

	public HexCell GetClickedCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		HexCell cell = cells[index];

		return cell;
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
			cells[pathIndexes[i]].hexColor = Color.white;
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
			cell.SetColor();
		}
		sortedHexes.Clear();
		path.Clear();
		pathIndexes.Clear();
		//hexMesh.Triangulate(cells);
	}

    public float GetSizeX()
    {
        return cellCountX * HexDefinition.innerRadius;
    }

    public float GetSizeZ()
    {
        return cellCountZ * HexDefinition.outerRadius;
    }
}
