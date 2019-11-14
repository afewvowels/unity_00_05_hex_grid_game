using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    public Bounds gridBounds;

    private Transform swivel, stick;

    private float zoom = 1.0f;

    [SerializeField]
    public float stickMinZoom, stickMaxZoom;

    [SerializeField]
    public float swivelMinZoom, swivelMaxZoom;

    [SerializeField]
    public float moveSpeedMinZoom, moveSpeedMaxZoom;

    [SerializeField]
    public float rotationSpeed;

    private float rotationAngle;

    public HexGrid hexGrid;

    static HexMapCamera instance;

    public static bool Locked
    {
        set
        {
            instance.enabled = !value;
        }
    }

    private void Awake()
    {
        instance = this;
        swivel = transform.GetChild(0);
        stick = transform.GetChild(0);

        //float posX = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>().GetSizeX();
        //float posZ = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>().GetSizeZ();

        //this.transform.position = new Vector3(posX, 50.0f, 0.0f);
        ValidatePosition();
    }

    private void FixedUpdate()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (System.Math.Abs(zoomDelta) > 0.0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (System.Math.Abs(rotationDelta) > 0.0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (System.Math.Abs(xDelta) > 0.0f || System.Math.Abs(zDelta) > 0.0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    private void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0.0f, distance, 0.0f);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
    }

    private void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction =
            transform.localRotation *
            new Vector3(xDelta, 0.0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = hexGrid.wrapping ? WrapPosition(position) : ClampPosition(position);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float xMax =
            (hexGrid.cellCountX - 0.5f) *
            HexDefinition.innerDiameter;
        position.x = Mathf.Clamp(position.x, 0.0f, xMax);

        float zMax =
            (hexGrid.cellCountZ - 1.0f) *
            (1.5f * HexDefinition.outerRadius);
        position.z = Mathf.Clamp(position.z, 0.0f, zMax);

        return position;
    }

    private void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0.0f)
        {
            rotationAngle += 360.0f;
        }
        else if (rotationAngle >= 360.0f)
        {
            rotationAngle -= 360.0f;
        }

        transform.localRotation = Quaternion.Euler(0.0f, rotationAngle, 0.0f);
    }

    public static void ValidatePosition()
    {
        float posX = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>().GetSizeX();
        float posZ = GameObject.FindGameObjectWithTag("hexgrid").GetComponent<HexGrid>().GetSizeZ();

        instance.transform.position = new Vector3(posX, posZ / 3.0f, 0.0f);
    }

    private Vector3 WrapPosition (Vector3 position)
    {
        float width = hexGrid.cellCountX * HexDefinition.innerDiameter;
        while (position.x < 0.0f)
        {
            position.x += width;
        }

        while (position.x > width)
        {
            position.x -= width;
        }

        float zMax = (hexGrid.cellCountZ - 1) * (1.5f * HexDefinition.outerRadius);
        position.z = Mathf.Clamp(position.z, 0.0f, zMax);

        hexGrid.CenterMap(position.x);
        return position;
    }
}
