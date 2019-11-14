using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexUnit : MonoBehaviour
{
    [SerializeField]
    private HexCell location;

    private HexCell currentTravelLocation;

    private float orientation;

    public static HexUnit unitPrefab;

    [SerializeField]
    private List<HexCell> pathToTravel;

    private const float travelSpeed = 4.0f;
    private const float rotationSpeed = 180.0f;

    public HexGrid Grid { get; set; }

    private const int visionRange = 3;

    public int VisionRange
    {
        get
        {
            return visionRange;
        }
    }

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                Grid.DecreaseVisibility(location, visionRange);
                location.Unit = null;
            }
            location = value;
            value.Unit = this;
            Grid.IncreaseVisibility(location, visionRange);
            transform.localPosition = value.Position;
            Grid.MakeChildOfColumn(transform, value.ColumnIndex);
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0.0f, value, 0.0f);
        }
    }

    private void OnEnable()
    {
        if (location)
        {
            transform.localPosition = location.Position;
            if (currentTravelLocation)
            {
                Grid.IncreaseVisibility(location, visionRange);
                Grid.DecreaseVisibility(currentTravelLocation, visionRange);
                currentTravelLocation = null;
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (pathToTravel == null || pathToTravel.Count == 0)
    //    {
    //        return;
    //    }

    //    Vector3 a, b, c = pathToTravel[0].Position;

    //    for (int i = 1; i < pathToTravel.Count; i++)
    //    {
    //        a = c;
    //        b = pathToTravel[i - 1].Position;
    //        c = (b + pathToTravel[i].Position) * 0.5f;
    //        for (float t = 0.0f; t < 1.0f; t += 0.1f)
    //        {
    //            Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2.0f);
    //        }
    //    }

    //    a = c;
    //    b = pathToTravel[pathToTravel.Count - 1].Position;
    //    c = b;
    //    for (float t = 0.0f; t < 1.0f; t += 0.1f)
    //    {
    //        Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2.0f);
    //    }
    //}

    public void ValidatePosition()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        if (location)
        {
            Grid.DecreaseVisibility(location, visionRange);
        }
        location.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load (BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();
        grid.AddUnit(Instantiate(unitPrefab), grid.GetHexCell(coordinates), orientation);
    }

    public bool IsValidDestination (HexCell cell)
    {
        return !cell.IsUnderwater && !cell.Unit;
    }

    public void Travel (List<HexCell> path)
    {
        location.Unit = null;
        location = path[path.Count - 1];
        location.Unit = this;
        pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }

    private IEnumerator TravelPath()
    {
        Vector3 a, b, c = pathToTravel[0].Position;
        yield return LookAt(pathToTravel[1].Position);
        if (!currentTravelLocation)
        {
            currentTravelLocation = pathToTravel[0];
        }
        Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
        int currentColumn = currentTravelLocation.ColumnIndex;

        float t = Time.deltaTime * travelSpeed;
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            currentTravelLocation = pathToTravel[i];
            a = c;
            b = pathToTravel[i - 1].Position;

            int nextColumn = currentTravelLocation.ColumnIndex;
            if (currentColumn != nextColumn)
            {
                if (nextColumn < currentColumn - 1)
                {
                    a.x -= HexDefinition.innerDiameter * HexDefinition.wrapSize;
                    b.x -= HexDefinition.innerDiameter * HexDefinition.wrapSize;
                }
                else if (nextColumn > currentColumn + 1)
                {
                    a.x += HexDefinition.innerDiameter * HexDefinition.wrapSize;
                    b.x += HexDefinition.innerDiameter * HexDefinition.wrapSize;
                }
                Grid.MakeChildOfColumn(transform, nextColumn);
                currentColumn = nextColumn;
            }

            c = (b + currentTravelLocation.Position) * 0.5f;
            Grid.IncreaseVisibility(pathToTravel[i], VisionRange);

            for (; t < 1.0f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0.0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }
            Grid.DecreaseVisibility(pathToTravel[i], visionRange);
            t -= 1.0f;
        }
        currentTravelLocation = null;

        a = c;
        b = location.Position;
        c = b;
        Grid.IncreaseVisibility(location, visionRange);
        for (; t < 1.0f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0.0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        transform.localPosition = location.Position;

        ListPool<HexCell>.Add(pathToTravel);
        pathToTravel = null;
    }

    private IEnumerator LookAt (Vector3 point)
    {
        if (HexDefinition.Wrapping)
        {
            float xDistance = point.x - transform.localPosition.x;
            if (xDistance < -HexDefinition.innerRadius * HexDefinition.wrapSize)
            {
                point.x += HexDefinition.innerRadius * HexDefinition.wrapSize;
            }
            else if (xDistance > HexDefinition.innerRadius * HexDefinition.wrapSize)
            {
                point.x -= HexDefinition.innerDiameter * HexDefinition.wrapSize;
            }
        }

        point.y = transform.localPosition.y;

        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);

        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0.0f)
        {
            float speed = rotationSpeed / angle;

            for (
            float t = Time.deltaTime * speed;
            t < 1.0f;
            t += Time.deltaTime * speed
            )
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;
    }
}
