using UnityEngine;

public static class Ext
{
	/// <summary>
	/// Calculates the position of an object stacked in a line, with an additional rotation applied to the stacking direction.
	/// </summary>
	/// <param name="index">The index of the object in the stack (0-based).</param>
	/// <param name="position">The starting position of the stack.</param>
	/// <param name="direction">The direction in which to stack objects (should be normalized).</param>
	/// <param name="eulerRotation">Euler angles (in degrees) to rotate the stacking direction.</param>
	/// <param name="slotOffset">The distance between each stacked object.</param>
	/// <returns>The calculated position for the object at the given index, with rotation applied.</returns>
	public static Vector3 GetStackedPosition(this Vector3 position, int index, Vector3 direction, Vector3 eulerRotation, float slotOffset)
	{
		Vector3 offset = index * slotOffset * direction;
		Quaternion rotation = Quaternion.Euler(eulerRotation); // Convert Vector3 rotation to Quaternion
		return position + rotation * offset; // Apply rotation to the offset
	}
	public static Vector3 WithY(this Vector3 vector, float newY)
	{
		return new Vector3(vector.x, newY, vector.z);
	}
	public static Vector3 AddY(this Vector3 vector, float yOffset)
	{
		return new Vector3(vector.x, vector.y + yOffset, vector.z);
	}

	public static Vector3 WithX(this Vector3 vector, float newX)
	{
		return new Vector3(newX, vector.y, vector.z);
	}
	public static Vector3 AddX(this Vector3 vector, float xOffset)
	{
		return new Vector3(vector.x + xOffset, vector.y, vector.z);
	}

	public static Vector3 WithZ(this Vector3 vector, float newZ)
	{
		return new Vector3(vector.x, vector.y, newZ);
	}
	public static Vector3 AddZ(this Vector3 vector, float zOffset)
	{
		return new Vector3(vector.x, vector.y, vector.z + zOffset);
	}
}