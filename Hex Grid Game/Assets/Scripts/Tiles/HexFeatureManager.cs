using UnityEngine;

public class HexFeatureManager : MonoBehaviour
{
    public HexFeatureCollection[] urbanCollections, farmCollections, plantCollections;
    public Transform container;
    public Transform wallTower;
    public Transform bridge;
    public Transform[] special;

    public HexMesh walls;

    public void Clear()
    {
        if (container)
        {
            Destroy(container.gameObject);
        }
        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
        walls.Clear();
    }

    public void Apply()
    {
        walls.Apply();
    }

    public void AddFeature (HexCell cell, Vector3 position)
    {
        if (cell.IsSpecial)
        {
            return;
        }

        HexHash hash = HexDefinition.SampleHashGrid(position);
        Transform prefab = PickPrefab(urbanCollections, cell.UrbanLevel, hash.a, hash.d);
        Transform otherPrefab = PickPrefab(farmCollections, cell.FarmLevel, hash.b, hash.d);
        float usedHash = hash.a;
        if (prefab)
        {
            if (otherPrefab && hash.b < hash.a)
            {
                prefab = otherPrefab;
                usedHash = hash.b;
            }
        }
        else if (otherPrefab)
        {
            prefab = otherPrefab;
            usedHash = hash.b;
        }
        otherPrefab = PickPrefab(plantCollections, cell.PlanetLevel, hash.c, hash.d);
        if (prefab)
        {
            if (otherPrefab && hash.c < usedHash)
            {
                prefab = otherPrefab;
            }
        }
        else if (otherPrefab)
        {
            prefab = otherPrefab;
        }
        else
        {
            return;
        }
        Transform instance = Instantiate(prefab);
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = HexDefinition.Displace(position);
        instance.localRotation = Quaternion.Euler(0.0f, 360.0f * hash.e, 0.0f);
        instance.SetParent(container, false);
    }

    public void AddSpecialFeature (HexCell cell, Vector3 position)
    {
        Transform instance = Instantiate(special[cell.SpecialIndex - 1]);
        instance.localPosition = HexDefinition.Displace(position);
        HexHash hash = HexDefinition.SampleHashGrid(position);
        instance.localRotation = Quaternion.Euler(0.0f, 360.0f * hash.e, 0.0f);
        instance.SetParent(container, false);
    }

    private Transform PickPrefab(HexFeatureCollection[] collection, int level, float hash, float choice)
    {
        if (level > 0)
        {
            float[] thresholds = HexDefinition.GetFeatureThresholds(level - 1);
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (hash < thresholds[i])
                {
                    return collection[i].Pick(choice);
                }
            }
        }
        return null;
    }

    public void AddWall(
        EdgeVertices near, HexCell nearCell,
        EdgeVertices far, HexCell farCell,
        bool hasRiver, bool hasRoad
        )
    {
        if (nearCell.Walled != farCell.Walled &&
            !nearCell.IsUnderwater && !farCell.IsUnderwater &&
            nearCell.GetEdgeType(farCell) != HexDefinition.HexEdgeType.Cliff)
        {
            AddWallSegment(near.v1, far.v1, near.v2, far.v2);
            if (hasRiver || hasRoad)
            {
                AddWallCap(near.v2, far.v2);
                AddWallCap(near.v4, far.v4);
            }
            else
            {
                AddWallSegment(near.v2, far.v2, near.v3, far.v3);
                AddWallSegment(near.v3, far.v3, near.v4, far.v4);
            }
            AddWallSegment(near.v4, far.v4, near.v5, far.v5);
        }
    }

    public void AddWall(
        Vector3 c1, HexCell cell1,
        Vector3 c2, HexCell cell2,
        Vector3 c3, HexCell cell3
        )
    {
        if (cell1.Walled)
        {
            if (cell2.Walled)
            {
                if (!cell3.Walled)
                {
                    AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
                }
            }
            else if (cell3.Walled)
            {
                AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
            }
            else
            {
                AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
            }
        }
        else if (cell2.Walled)
        {
            if (cell3.Walled)
            {
                AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
            }
            else
            {
                AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
            }
        }
        else if (cell3.Walled)
        {
            AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
        }
    }

    private void AddWallSegment(
        Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight, bool addTower = false)
    {
        nearLeft = HexDefinition.Displace(nearLeft);
        farLeft = HexDefinition.Displace(farLeft);
        nearRight = HexDefinition.Displace(nearRight);
        farRight = HexDefinition.Displace(farRight);

        Vector3 left = HexDefinition.WallLerp(nearLeft, farLeft);
        Vector3 right = HexDefinition.WallLerp(nearRight, farRight);

        Vector3 leftThicknessOffset = HexDefinition.WallThicknessOffset(nearLeft, farLeft);
        Vector3 rightThicknessOffset = HexDefinition.WallThicknessOffset(nearRight, farRight);

        float leftTop = left.y + HexDefinition.wallHeight;
        float rightTop = right.y + HexDefinition.wallHeight;

        Vector3 v1, v2, v3, v4;
        v1 = v3 = left - leftThicknessOffset;
        v2 = v4 = right - rightThicknessOffset;
        v3.y = leftTop;
        v4.y = rightTop;
        walls.AddQuadNonDisplaced(v1, v2, v3, v4);

        Vector3 t1 = v3, t2 = v4;

        v1 = v3 = left + leftThicknessOffset;
        v2 = v4 = right + rightThicknessOffset;
        v3.y = leftTop;
        v4.y = rightTop;
        walls.AddQuadNonDisplaced(v2, v1, v4, v3);

        walls.AddQuadNonDisplaced(t1, t2, v3, v4);

        if (addTower)
        {
            Transform towerInstance = Instantiate(wallTower);
            towerInstance.transform.localPosition = (left + right) * 0.5f;
            Vector3 rightDirection = right - left;
            rightDirection.y = 0.0f;
            towerInstance.transform.right = rightDirection;
            towerInstance.SetParent(container, false);
        }
    }

    private void AddWallSegment(
        Vector3 pivot, HexCell pivotCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
        )
    {
        if (pivotCell.IsUnderwater)
        {
            return;
        }

        bool hasLeftWall = !leftCell.IsUnderwater &&
                            pivotCell.GetEdgeType(leftCell) != HexDefinition.HexEdgeType.Cliff;
        bool hasRightWall = !rightCell.IsUnderwater &&
                            pivotCell.GetEdgeType(rightCell) != HexDefinition.HexEdgeType.Cliff;

        if (hasLeftWall)
        {
            if (hasRightWall)
            {
                bool hasTower = false;
                if (leftCell.Elevation == rightCell.Elevation)
                {
                    HexHash hash = HexDefinition.SampleHashGrid((pivot + left + right) * (1.0f / 3.0f));
                    hasTower = hash.e < HexDefinition.wallTowerThreshold;
                }
                AddWallSegment(pivot, left, pivot, right, hasTower);
            }
            else if (leftCell.Elevation < rightCell.Elevation)
            {
                AddWallWedge(pivot, left, right);
            }
            else
            {
                AddWallCap(pivot, left);
            }
        }
        else if (hasRightWall)
        {
            if (rightCell.Elevation < leftCell.Elevation)
            {
                AddWallWedge(right, pivot, left);
            }
            else
            {
                AddWallCap(right, pivot);
            }
        }
    }

    private void AddWallCap(Vector3 near, Vector3 far)
    {
        near = HexDefinition.Displace(near);
        far = HexDefinition.Displace(far);

        Vector3 center = HexDefinition.WallLerp(near, far);
        Vector3 thickness = HexDefinition.WallThicknessOffset(near, far);

        Vector3 v1, v2, v3, v4;

        v1 = v3 = center - thickness;
        v2 = v4 = center + thickness;
        v3.y = v4.y = center.y + HexDefinition.wallHeight;
        walls.AddQuadNonDisplaced(v1, v2, v3, v4);
    }

    private void AddWallWedge(Vector3 near, Vector3 far, Vector3 point)
    {
        near = HexDefinition.Displace(near);
        far = HexDefinition.Displace(far);
        point = HexDefinition.Displace(point);

        Vector3 center = HexDefinition.WallLerp(near, far);
        Vector3 thickness = HexDefinition.WallThicknessOffset(near, far);

        Vector3 v1, v2, v3, v4;
        Vector3 pointTop = point;
        point.y = center.y;

        v1 = v3 = center - thickness;
        v2 = v4 = center + thickness;
        v3.y = v4.y = pointTop.y = center.y + HexDefinition.wallHeight;

        walls.AddQuadNonDisplaced(v1, point, v3, pointTop);
        walls.AddQuadNonDisplaced(point, v2, pointTop, v4);
        walls.AddNonDisplacedTriangle(pointTop, v3, v4);
    }

    public void AddBridge(Vector3 roadCenter1, Vector3 roadCenter2)
    {
        roadCenter1 = HexDefinition.Displace(roadCenter1);
        roadCenter2 = HexDefinition.Displace(roadCenter2);
        Transform instance = Instantiate(bridge);
        instance.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
        instance.forward = roadCenter2 - roadCenter1;
        float length = Vector3.Distance(roadCenter1, roadCenter2);
        instance.localScale = new Vector3(
            1.0f, 1.0f, length * (1.0f / HexDefinition.bridgeDesignLength));
        instance.SetParent(container, false);
    }
}
