using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	public Color[] colors;

	public HexGrid hexGrid;

	private Color activeColor;

	private void Awake()
	{
		SelectColor(0);
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
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
			hexGrid.ColorCell(hit.point, activeColor);
		}
	}
}
