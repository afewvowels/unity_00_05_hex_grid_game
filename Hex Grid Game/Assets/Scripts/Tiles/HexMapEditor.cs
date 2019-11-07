using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	public Color[] colors;

	public HexGrid hexGrid;

	private Color activeColor;

	private HexCell start, destination;

	public GameObject player;

	private void Awake()
	{
		SelectColor(0);
		start = null;
		destination = null;
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			HandleInput();
		}
	}

	public void SelectColor(int index)
	{
		activeColor = colors[index];
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			//hexGrid.ColorCell(hit.point, activeColor);
			if (start == null && destination == null)
			{
				hexGrid.path.Clear();
				hexGrid.sortedHexes.Clear();
				foreach (GameObject label in GameObject.FindGameObjectsWithTag("label"))
				{
					Destroy(label);
				}
				foreach (HexCell cell in hexGrid.cells)
				{
					cell.ResetDjikstra();
					cell.SetColor();
				}
				hexGrid.hexMesh.Triangulate(hexGrid.cells);
				start = hexGrid.GetClickedCell(hit.point);
				Debug.Log(start.hexColor);
			}

			else if (start != null && destination == null)
			{
				destination = hexGrid.GetClickedCell(hit.point);
				Debug.Log(start.hexColor + "," + destination.hexColor);
			}

			else if (start != null && destination != null)
			{
				start.SetOrigin();
				start.SetParentCellID(start.GetCellID());
				hexGrid.sortedHexes.Add(start);
				hexGrid.FindPath(destination);
				//hexGrid.FollowPath();
				start = null;
				destination = null;
			}
		}
	}
}
