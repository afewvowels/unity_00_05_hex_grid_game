using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
	public int width;
	public int height;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	public Color defaultColor = Color.white;

	private Canvas gridCanvas;

	private HexCell[] cells;
	private HexMesh hexMesh;

	private void Awake()
	{
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[height * width];

		for (int z = 0, i = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	private void Start()
	{
		hexMesh.Triangulate(cells);
	}

	void CreateCell(int x, int z, int i)
	{

		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexDefinition.innerRadius * 2.0f);
		position.y = 0.0f;
		position.z = z * (HexDefinition.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);

		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.hexColor = defaultColor;

		if (x > 0)
		{
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0)
		{
			if ((z & 1) == 0)
			{
				cell.SetNeighbor(HexDirection.SE, cells[i - width]);
				if (x > 0)
				{
					cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
				}
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - width]);
				if (x < width - 1)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
				}
			}
		}

		CreateLabel(x, z, position, cell);
	}

	void CreateLabel(int x, int z, Vector3 position, HexCell cell)
	{
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}

	public void ColorCell(Vector3 position, Color color)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.hexColor = color;
		foreach (HexCell adjCell in cell.GetNeighbors())
		{
			adjCell.hexColor = color;
		}
		hexMesh.Triangulate(cells);
		Debug.Log("touched at " + coordinates.ToString());
	}

	public void MoveObject(Vector3 position, GameObject go)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		go.transform.position = cell.transform.position;
		Debug.Log("moved");
	}

	//public HexCell[] FindAdjacentCells(HexCell cell)
	//{
	//	HexCell[] adjCells;



	//	return adjCells;
	//}

	public void FindPath(HexCell start, HexCell destination)
	{

	}
}
