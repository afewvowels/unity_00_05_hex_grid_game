using UnityEngine;

public static class HexDefinition
{

	public const float outerRadius = 10.0f;
	public const float innerRadius = outerRadius * 0.866025404f;

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
}
