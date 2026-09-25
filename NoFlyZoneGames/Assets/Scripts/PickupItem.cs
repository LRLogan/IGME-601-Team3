using UnityEngine;

/// <summary>
/// Allows an object to be picked up and placed by the player.
/// </summary>
public class PickupItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [Header("Placement")]
    // Defines how much space around the item's center must fit on the surface.
    [SerializeField] private float placementRadius = 0.25f;

    // Tracks whether the item is currently being held by the player.
    private bool isHeld;

    /// <summary>
    /// Gets whether this item is currently being held by the player.
    /// </summary>
    public bool IsHeld => isHeld;

    /// <summary>
    /// Gets the radius used when checking whether the item can be placed.
    /// </summary>
    public float PlacementRadius => placementRadius;

    private void Awake()
    {
        // Automatically find the Rigidbody if one was not assigned.
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Picks up the item and attaches it to the player's hold point.
    /// </summary>
    /// <param name="holdPoint">The transform where the item will be held.</param>
    public void PickUp(Transform holdPoint)
    {
        isHeld = true;

        if (rb != null)
        {
            // Stop physics from controlling the object while it is held.
            rb.isKinematic = true;

            // Prevent the held object from colliding with the player or environment.
            rb.detectCollisions = false;
        }

        // Parent the item to the player's hold point.
        transform.SetParent(holdPoint, false);

        // Position and rotate the item relative to the hold point.
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Places the item at the specified position and rotation.
    /// </summary>
    /// <param name="position">The item's new world position.</param>
    /// <param name="rotation">The item's new world rotation.</param>
    public void Place(Vector3 position, Quaternion rotation)
    {
        isHeld = false;

        // Remove the item from the player's hold point.
        transform.SetParent(null);

        // Move the item to the calculated placement position.
        transform.SetPositionAndRotation(position, rotation);

        if (rb != null)
        {
            // Return control of the item to the physics system.
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }
}