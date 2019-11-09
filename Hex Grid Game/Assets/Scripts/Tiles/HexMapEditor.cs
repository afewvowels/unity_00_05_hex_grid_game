using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	public Color[] colors;

	public HexGrid hexGrid;

	private Color activeColor;

	public GameObject player;

    public int activeElevation;

	private void Awake()
	{
		SelectColor(0);
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

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
            EditCell(hexGrid.GetClickedCell(hit.point));
		}
	}

    private void EditCell(HexCell cell)
    {
        cell.hexColor = activeColor;
        cell.Elevation = activeElevation;
        hexGrid.RedrawGrid();
    }
}
