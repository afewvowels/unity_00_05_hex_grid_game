using UnityEngine;

public class MoveObject : MonoBehaviour
{
	public HexGrid hexGrid;
	// Start is called before the first frame update
	void Start()
	{
		hexGrid.MoveObject(new Vector3(1.0f, 1.0f, 1.0f), this.gameObject);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleInput();
		}
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			hexGrid.MoveObject(hit.point, this.gameObject);
		}
	}
}
