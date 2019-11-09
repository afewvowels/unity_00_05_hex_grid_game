using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
	Mesh hexMesh;
	MeshCollider meshCollider;
	List<Vector3> vertices;
	List<int> triangles;
	List<Color> colors;

	private void Awake()
	{
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();
		hexMesh.name = "Hex Mesh";
		vertices = new List<Vector3>();
		triangles = new List<int>();
		colors = new List<Color>();
	}

	public void Triangulate(HexCell[] cells)
	{
		hexMesh.Clear();
		vertices.Clear();
		triangles.Clear();
		colors.Clear();

		for (int i = 0; i < cells.Length; i++)
		{
			Triangulate(cells[i]);
		}

		hexMesh.vertices = vertices.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.colors = colors.ToArray();
		hexMesh.RecalculateNormals();

		meshCollider.sharedMesh = hexMesh;
	}

	private void Triangulate(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			Triangulate(d, cell);
		}
	}

	private void Triangulate(HexDirection direction, HexCell cell)
	{
		Vector3 center = cell.Position;
		Vector3 v1 = center + HexDefinition.GetFirstSolidCorner(direction);
		Vector3 v2 = center + HexDefinition.GetSecondSolidCorner(direction);

		AddTriangle(center, v1, v2);
		AddTriangleColor(cell.hexColor);

		if (direction <= HexDirection.SE)
		{
			TriangulateConnection(direction, cell, v1, v2);
		}
	}

	private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
	{
		HexCell neighbor = cell.GetNeighbor(direction);

		if (neighbor == null)
		{
			return;
		}

		Vector3 bridge = HexDefinition.GetBridge(direction);
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;
		v3.y = v4.y = neighbor.Position.y;

		if (cell.GetEdgeType(direction) == HexDefinition.HexEdgeType.Slope)
		{
			TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
		}
		else
		{
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(cell.hexColor, neighbor.hexColor);
		}

		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null)
		{
			Vector3 v5 = v2 + HexDefinition.GetBridge(direction.Next());
			v5.y = nextNeighbor.Position.y;
			if (cell.Elevation <= neighbor.Elevation)
			{
				if (cell.Elevation <= nextNeighbor.Elevation)
				{
					TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
				}
				else
				{
					TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
				}
			}
			else if (neighbor.Elevation <= nextNeighbor.Elevation)
			{
				TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
			}
			else
			{
				TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
			}
		}
	}

	private void TriangulateEdgeTerraces(
			Vector3 beginLeft,
			Vector3 beginRight,
			HexCell beginCell,
			Vector3 endLeft,
			Vector3 endRight,
			HexCell endCell
			)
	{
		Vector3 v3 = HexDefinition.TerraceLerp(beginLeft, endLeft, 1);
		Vector3 v4 = HexDefinition.TerraceLerp(beginRight, endRight, 1);
		Color c2 = HexDefinition.TerraceLerp(beginCell.hexColor, endCell.hexColor, 1);

		AddQuad(beginLeft, beginRight, v3, v4);
		AddQuadColor(beginCell.hexColor, c2);

		for (int i = 2; i < HexDefinition.terraceSteps; i++)
		{
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c2;
			v3 = HexDefinition.TerraceLerp(beginLeft, endLeft, i);
			v4 = HexDefinition.TerraceLerp(beginRight, endRight, i);
			c2 = HexDefinition.TerraceLerp(beginCell.hexColor, endCell.hexColor, i);

			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2);
		}

		AddQuad(v3, v4, endLeft, endRight);
		AddQuadColor(c2, endCell.hexColor);
	}

	private void TriangulateCorner(
			Vector3 bottom, HexCell bottomCell,
			Vector3 left, HexCell leftCell,
			Vector3 right, HexCell rightCell
			)
	{
		HexDefinition.HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		HexDefinition.HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

		if (leftEdgeType == HexDefinition.HexEdgeType.Slope)
		{
			if (rightEdgeType == HexDefinition.HexEdgeType.Slope)
			{
				TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
			else if (rightEdgeType == HexDefinition.HexEdgeType.Flat)
			{
				TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
			}
			else
			{
				TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (rightEdgeType == HexDefinition.HexEdgeType.Slope)
		{
			if (leftEdgeType == HexDefinition.HexEdgeType.Flat)
			{
				TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else
			{
				TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (leftCell.GetEdgeType(rightCell) == HexDefinition.HexEdgeType.Slope)
		{
			if (leftCell.Elevation < rightCell.Elevation)
			{
				TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else
			{
				TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
			}
		}
		else
		{
			AddTriangle(bottom, left, right);
			AddTriangleColor(bottomCell.hexColor, leftCell.hexColor, rightCell.hexColor);
		}

	}

	private void TriangulateCornerTerraces(
			Vector3 begin, HexCell beginCell,
			Vector3 left, HexCell leftCell,
			Vector3 right, HexCell rightCell
			)
	{
		Vector3 v3 = HexDefinition.TerraceLerp(begin, left, 1);
		Vector3 v4 = HexDefinition.TerraceLerp(begin, right, 1);
		Color c3 = HexDefinition.TerraceLerp(beginCell.hexColor, leftCell.hexColor, 1);
		Color c4 = HexDefinition.TerraceLerp(beginCell.hexColor, rightCell.hexColor, 1);

		AddTriangle(begin, v3, v4);
		AddTriangleColor(beginCell.hexColor, c3, c4);

		for (int i = 2; i < HexDefinition.terraceSteps; i++)
		{
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;

			v3 = HexDefinition.TerraceLerp(begin, left, i);
			v4 = HexDefinition.TerraceLerp(begin, right, i);
			c3 = HexDefinition.TerraceLerp(beginCell.hexColor, leftCell.hexColor, i);
			c4 = HexDefinition.TerraceLerp(beginCell.hexColor, rightCell.hexColor, i);

			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}

		AddQuad(v3, v4, left, right);
		AddQuadColor(c3, c4, leftCell.hexColor, rightCell.hexColor);
	}

	private void TriangulateCornerTerracesCliff(
			Vector3 begin, HexCell beginCell,
			Vector3 left, HexCell leftCell,
			Vector3 right, HexCell rightCell
			)
	{
		float b = Mathf.Abs(1.0f / (float)(rightCell.Elevation - beginCell.Elevation));

		Vector3 boundary = Vector3.Lerp(begin, right, b);
		Color boundaryColor = Color.Lerp(beginCell.hexColor, rightCell.hexColor, b);

		TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

		if (leftCell.GetEdgeType(rightCell) == HexDefinition.HexEdgeType.Slope)
		{
			TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
		}
		else
		{
			AddTriangle(left, right, boundary);
			AddTriangleColor(leftCell.hexColor, rightCell.hexColor, boundaryColor);
		}
	}

	private void TriangulateCornerCliffTerraces(
			Vector3 begin, HexCell beginCell,
			Vector3 left, HexCell leftCell,
			Vector3 right, HexCell rightCell
			)
	{
		float b = Mathf.Abs(1.0f / (float)(leftCell.Elevation - beginCell.Elevation));

		Vector3 boundary = Vector3.Lerp(begin, left, b);
		Color boundaryColor = Color.Lerp(beginCell.hexColor, leftCell.hexColor, b);

		TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

		if (leftCell.GetEdgeType(rightCell) == HexDefinition.HexEdgeType.Slope)
		{
			TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
		}
		else
		{
			AddTriangle(left, right, boundary);
			AddTriangleColor(leftCell.hexColor, rightCell.hexColor, boundaryColor);
		}
	}

	private void TriangulateBoundaryTriangle(
			Vector3 begin, HexCell beginCell,
			Vector3 left, HexCell leftCell,
			Vector3 boundary, Color boundaryColor
			)
	{
		Vector3 v2 = HexDefinition.TerraceLerp(begin, left, 1);
		Color c2 = HexDefinition.TerraceLerp(beginCell.hexColor, leftCell.hexColor, 1);

		AddTriangle(begin, v2, boundary);
		AddTriangleColor(beginCell.hexColor, c2, boundaryColor);

		for (int i = 2; i < HexDefinition.terraceSteps; i++)
		{
			Vector3 v1 = v2;
			Color c1 = c2;

			v2 = HexDefinition.TerraceLerp(begin, left, i);
			c2 = HexDefinition.TerraceLerp(beginCell.hexColor, leftCell.hexColor, i);

			AddTriangle(v1, v2, boundary);
			AddTriangleColor(c1, c2, boundaryColor);
		}

		AddTriangle(v2, left, boundary);
		AddTriangleColor(c2, leftCell.hexColor, boundaryColor);
	}

	private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = vertices.Count;

		vertices.Add(Displace(v1));
		vertices.Add(Displace(v2));
		vertices.Add(Displace(v3));

		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	private void AddTriangleColor(Color color)
	{
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	private void AddTriangleColor(Color c1, Color c2, Color c3)
	{
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		int vertexIndex = vertices.Count;
		vertices.Add(Displace(v1));
		vertices.Add(Displace(v2));
		vertices.Add(Displace(v3));
		vertices.Add(Displace(v4));
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

	private void AddQuadColor(Color c1, Color c2)
	{
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}

	private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
	{
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}

	private Vector3 Displace(Vector3 position)
	{
		Vector4 sample = HexDefinition.SampleNoise(position);

		position.x += NormalizeDisplace(sample.x);
		//position.y += NormalizeDisplace(sample.y);
		position.z += NormalizeDisplace(sample.z);

		return position;
	}

	private float NormalizeDisplace(float input)
	{
		return ((input * 2.0f) - 1.0f) * HexDefinition.cellDisplacementStrength;
	}
}
