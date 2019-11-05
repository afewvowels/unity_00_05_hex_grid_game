using UnityEngine;

public class AlignCamera : MonoBehaviour
{
	private GameObject hexGrid;
	private Collider hexMesh;

	private bool isCamPositioned;
	// Start is called before the first frame update
	void Start()
	{
		isCamPositioned = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (!isCamPositioned)
		{
			PlaceCamera();
		}
	}

	void PlaceCamera()
	{
		hexGrid = GameObject.FindGameObjectWithTag("hexgrid");
		hexMesh = hexGrid.GetComponentInChildren<Collider>();

		this.transform.position = hexMesh.bounds.center;

		GameObject.FindGameObjectWithTag("MainCamera").transform.position = new Vector3(hexMesh.bounds.center.x, hexMesh.bounds.center.x * 2.0f, hexMesh.bounds.center.z);
	}
}
