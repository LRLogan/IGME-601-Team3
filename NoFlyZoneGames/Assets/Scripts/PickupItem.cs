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

    [Header("Area Of Effect")]
    // Optional. The trigger SphereCollider or BoxCollider that defines this
    // item's AOE. If left empty, the first trigger sphere or box collider found
    // on this object or its children is used. Items without one simply show no
    // AOE preview.
    [SerializeField] private Collider aoeCollider;

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

    /// <summary>
    /// Gets whether this item has an area of effect.
    /// </summary>
    public bool HasAoe => aoeCollider != null;

    private void Awake()
    {
        // Automatically find the Rigidbody if one was not assigned.
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Automatically find the AOE trigger if one was not assigned.
        if (aoeCollider == null)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>(true))
            {
                if (col.isTrigger && (col is SphereCollider || col is BoxCollider))
                {
                    aoeCollider = col;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Describes the shape of an item's area of effect in world units.
    /// </summary>
    public struct AoeShape
    {
        /// <summary>
        /// The primitive that matches the AOE: Sphere or Cube.
        /// </summary>
        public PrimitiveType type;

        /// <summary>
        /// The full size of the AOE. A sphere uses its diameter on every axis.
        /// </summary>
        public Vector3 size;

        /// <summary>
        /// The AOE center relative to the item's pivot, with the item's current rotation removed.
        /// </summary>
        public Vector3 centerOffset;

        /// <summary>
        /// The AOE collider's rotation relative to the item.
        /// </summary>
        public Quaternion rotationOffset;
    }

    /// <summary>
    /// Gets the item's area of effect as a world-space shape.
    /// </summary>
    /// <param name="shape">The shape, size and placement of the AOE relative to the item.</param>
    /// <returns>True if the item has an AOE.</returns>
    public bool TryGetAoe(out AoeShape shape)
    {
        shape = default;

        if (aoeCollider == null)
        {
            return false;
        }

        // Only collider settings and transforms are read (not Collider.bounds),
        // so this still works if an item effect disables the collider while held.
        Transform colliderTransform = aoeCollider.transform;
        Vector3 scale = colliderTransform.lossyScale;
        Vector3 localCenter;

        if (aoeCollider is BoxCollider box)
        {
            shape.type = PrimitiveType.Cube;
            shape.size = Vector3.Scale(box.size, new Vector3(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)));
            localCenter = box.center;
        }
        else if (aoeCollider is SphereCollider sphere)
        {
            // Unity scales a SphereCollider by the largest axis of its lossy scale.
            float maxScale = Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z));

            shape.type = PrimitiveType.Sphere;
            shape.size = Vector3.one * (sphere.radius * maxScale * 2f);
            localCenter = sphere.center;
        }
        else if (aoeCollider is CapsuleCollider capsule) 
        {
            shape.type = PrimitiveType.Capsule;
            float radius = capsule.radius;
            float height = capsule.height;

            Vector3 absScale = new Vector3(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z));

            // Scale the capsule according to its direction. (capsules can go along x y or z axis)
            switch (capsule.direction)
            {
                case 0: // X axis
                    shape.size = new Vector3(
                        height * absScale.x,
                        radius * 2f * absScale.y,
                        radius * 2f * absScale.z);
                    break;

                case 1: // Y axis
                    shape.size = new Vector3(
                        radius * 2f * absScale.x,
                        height * absScale.y,
                        radius * 2f * absScale.z);
                    break;

                case 2: // Z axis
                    shape.size = new Vector3(
                        radius * 2f * absScale.x,
                        radius * 2f * absScale.y,
                        height * absScale.z);
                    break;
            }

            localCenter = capsule.center;
        }
        else
        {
            return false;
        }

        Quaternion inverseItemRotation = Quaternion.Inverse(transform.rotation);

        shape.centerOffset = inverseItemRotation *
            (colliderTransform.TransformPoint(localCenter) - transform.position);
        shape.rotationOffset = inverseItemRotation * colliderTransform.rotation;

        return true;
    }

    /// <summary>
    /// Picks up the item and attaches it to the player's hold point.
    /// </summary>
    /// <param name="holdPoint">The transform where the item will be held.</param>
    public void PickUp(Transform holdPoint)
    {
        isHeld = true;

        // When hoolding, immediately turn off the item
        SetEffectsActive(false);

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

    // Check if the interacted item has IItemEffect interface
    // Turn the item off when picked up
    private void SetEffectsActive(bool active)
    {
        foreach (MonoBehaviour component in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (component is IItemEffect effect)
            {
                effect.SetEffectActive(active);
            }
        }
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

        // After placed the item, set item to active
        SetEffectsActive(true);
    }
}
