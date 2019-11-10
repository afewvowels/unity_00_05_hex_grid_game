using UnityEngine;

public class AlignCamera : MonoBehaviour
{
	private GameObject hexGrid;
	private Collider[] hexMesh;

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
		hexMesh = hexGrid.GetComponentsInChildren<Collider>();

        Bounds gridBounds = new Bounds(transform.position, Vector3.one);

        foreach (Collider bound in hexMesh)
        {
            gridBounds.Encapsulate(bound.bounds);
        }
         
		this.transform.position = gridBounds.center;

        GameObject.FindGameObjectWithTag("MainCamera").transform.position = new Vector3(gridBounds.center.x, gridBounds.center.x, -gridBounds.center.x / 2.0f);
    }
}
