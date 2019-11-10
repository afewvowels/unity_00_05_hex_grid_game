﻿using UnityEngine;

public static class HexDefinition
{
    public const float outerToInner = 0.866025404f;
    public const float innerToOuter = 1.0f / outerToInner;

    public const float outerRadius = 10.0f;
	public const float innerRadius = outerRadius * outerToInner;

	public const float solidFactor = 0.8f;
	public const float blendFactor = 1.0f - solidFactor;

	public const float elevationStep = 2.0f;

	public const int terracesPerSlope = 2;
	public const int terraceSteps = terracesPerSlope * 2 + 1;

	public const float horizontalTerraceStepSize = 1.0f / (float)terraceSteps;
	public const float verticalTerraceStepSize = 1.0f / (float)(terracesPerSlope + 1);

	public static Texture2D noiseSource;

	public const float cellDisplacementStrength = 3.0f;
	public const float noiseScale = 0.003f;
	public const float elevationDisplacementStrength = 1.5f;

    public const int chunkSizeX = 5;
    public const int chunkSizeZ = 5;

    public const float streamBedElevationOffset = -1.75f;

    //public const float riverSurfaceElevationOffset = -0.5f;
    public const float waterElevationOffset = -0.5f;

    public enum HexEdgeType
	{
		Flat,
		Slope,
		Cliff
    }

    public static Vector3[] corners =
    {
		new Vector3(0.0f, 0.0f, outerRadius),
		new Vector3(innerRadius, 0.0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0.0f, 0.5f * -outerRadius),
		new Vector3(0.0f ,0.0f, -outerRadius),
		new Vector3(-innerRadius, 0.0f, 0.5f * -outerRadius),
		new Vector3(-innerRadius, 0.0f, 0.5f * outerRadius),
		new Vector3(0.0f, 0.0f, outerRadius)
	};

	public static Vector3 GetFirstCorner(HexDirection direction)
	{
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner(HexDirection direction)
	{
		return corners[(int)direction + 1];
	}

	public static Vector3 GetFirstSolidCorner(HexDirection direction)
	{
		return corners[(int)direction] * solidFactor;
	}

	public static Vector3 GetSecondSolidCorner(HexDirection direction)
	{
		return corners[(int)direction + 1] * solidFactor;
	}

	public static Vector3 GetBridge(HexDirection direction)
	{
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
	}

	public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
	{
		float h = step * HexDefinition.horizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((step + 1) / 2) * HexDefinition.verticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}

	public static Color TerraceLerp(Color a, Color b, int step)
	{
		float h = step * HexDefinition.horizontalTerraceStepSize;
		return Color.Lerp(a, b, h);
	}

	public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
	{
		if (elevation1 == elevation2)
		{
			return HexEdgeType.Flat;
		}
		int delta = elevation2 - elevation1;
		if (Mathf.Abs(delta) <= 2 && Mathf.Abs(delta) > 0)
		{
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}

	public static Vector4 SampleNoise (Vector3 position)
	{
		return noiseSource.GetPixelBilinear(
			position.x * noiseScale,
			position.z * noiseScale
		);
	}

    public static Vector3 GetSolidEdgeMiddle(HexDirection direction)
    {
        return
            (corners[(int)direction] + corners[(int)direction + 1]) *
            (0.5f * solidFactor);
    }

    public static Vector3 Displace(Vector3 position)
    {
        Vector4 sample = HexDefinition.SampleNoise(position);

        position.x += NormalizeDisplace(sample.x);
        position.z += NormalizeDisplace(sample.z);

        return position;
    }

    public static float NormalizeDisplace(float input)
    {
        return ((input * 2.0f) - 1.0f) * cellDisplacementStrength;
    }
}
