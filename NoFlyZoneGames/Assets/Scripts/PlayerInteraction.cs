using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player interaction with pickupable objects and placement surfaces.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private InputActionReference interactAction;

    [Header("Placement")]
    [SerializeField] private LayerMask placementSurfaceLayer;
    [SerializeField] private float placementRange = 4f;

    // Small offset to prevent the item from slightly clipping into the surface.
    [SerializeField] private float placementPadding = 0.01f;

    // Extra space kept between a placed item and the player's colliders.
    [SerializeField] private float playerClearance = 0.05f;

    [Header("Preview")]
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;

    // The item currently being held by the player.
    private PickupItem heldItem;

    // The held item's solid (non-trigger) collider, cached when it is picked up.
    private Collider heldCollider;

    // The visual copy of the held item used for the placement preview.
    private GameObject placementPreview;

    // The position and rotation where the item can currently be placed.
    private Vector3 placementPosition;
    private Quaternion placementRotation;

    // Determines whether the current placement position is valid.
    private bool canPlace;

    // Public read-only interaction state
    public bool IsHoldingItem => heldItem != null;

    public bool CanPlaceHeldItem => heldItem != null && canPlace;

    public bool CanPickUpItem()
    {
        if (heldItem != null || playerCamera == null)
            return false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        PickupItem item =
            hit.collider.GetComponentInParent<PickupItem>();

        return item != null && !item.IsHeld;
    }

    private void OnEnable()
    {
        // Enable the interaction input action when this component is active.
        interactAction.action.Enable();

        // Call OnInteract whenever the interaction button is pressed.
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        // Stop listening for the interaction input when this component is disabled.
        interactAction.action.performed -= OnInteract;

        // Disable the input action.
        interactAction.action.Disable();
    }

    private void Update()
    {
        // Continuously update placement while holding an item.
        if (heldItem != null)
        {
            UpdatePlacement();
        }
    }

    /// <summary>
    /// Returns the first enabled, non-trigger collider on the object or its children.
    /// </summary>
    private Collider GetSolidCollider(GameObject target)
    {
        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            if (col.enabled && !col.isTrigger)
            {
                return col;
            }
        }

        return null;
    }

    /// <summary>
    /// Handles the player's interaction input.
    /// </summary>
    /// <param name="context">Information about the triggered input action.</param>
    private void OnInteract(InputAction.CallbackContext context)
    {
        // Pressing the button while holding an item attempts to place it.
        if (heldItem != null)
        {
            TryPlaceItem();
            return;
        }

        // Otherwise, pressing the button attempts to pick something up.
        TryPickUpItem();
    }

    /// <summary>
    /// Attempts to pick up the item the player is looking at.
    /// </summary>
    private void TryPickUpItem()
    {
        // Shoot a ray from the center of the player's camera.
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        // Do not pick anything up if the object is out of interaction range.
        if (!Physics.Raycast(
            ray, 
            out RaycastHit hit, 
            interactionRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        // Look for a PickupItem on the object that was hit.
        PickupItem item = hit.collider.GetComponentInParent<PickupItem>();

        // Ignore objects that are not pickupable or are already being held.
        if (item == null || item.IsHeld)
        {
            return;
        }

        // Cache on pickup
        heldItem = item;
        heldCollider = GetSolidCollider(heldItem.gameObject);
        CreatePlacementPreview();

        // Attach the real item to the player's hold point.
        heldItem.PickUp(holdPoint);
    }

    /// <summary>
    /// Creates a visual copy of the currently held item for placement.
    /// </summary>
    private void CreatePlacementPreview()
    {
        DestroyPlacementPreview();

        placementPreview = Instantiate(heldItem.gameObject);

        // Keep the preview hidden until a valid surface is found.
        placementPreview.SetActive(false);

        // Remove components that could interfere with the preview.
        RemovePreviewComponents(placementPreview);
    }

    /// <summary>
    /// Removes physics and interaction components from the preview object.
    /// </summary>
    /// <param name="preview">The preview object to clean up.</param>
    private void RemovePreviewComponents(GameObject preview)
    {
        Rigidbody[] rigidbodies = preview.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidbody in rigidbodies)
        {
            Destroy(rigidbody);
        }

        Collider[] colliders = preview.GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            Destroy(collider);
        }

        PickupItem[] pickupItems =
            preview.GetComponentsInChildren<PickupItem>();

        foreach (PickupItem pickupItem in pickupItems)
        {
            Destroy(pickupItem);
        }

        MonoBehaviour[] monoBehaviours = preview.GetComponentsInChildren<MonoBehaviour>();

        foreach(MonoBehaviour monoBehaviour in monoBehaviours)
        {
            Destroy(monoBehaviour);
        }
    }

    /// <summary>
    /// Destroys the current placement preview.
    /// </summary>
    private void DestroyPlacementPreview()
    {
        if (placementPreview != null)
        {
            Destroy(placementPreview);
            placementPreview = null;
        }
    }

    /// <summary>
    /// Updates the position and validity of the current placement location.
    /// </summary>
    private void UpdatePlacement()
    {
        // Shoot a ray from the camera to find a placement surface.
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        // Only surfaces on the PlacementSurface layer can be used.
        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                placementRange,
                placementSurfaceLayer))
        {
            // There is no valid placement surface in view.
            canPlace = false;

            // Hide the preview when there is no valid surface.
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }

            return;
        }

        // Face the same horizontal direction as the camera while staying upright.
        Vector3 flatForward = Vector3.ProjectOnPlane(
            playerCamera.transform.forward,
            Vector3.up);

        // Looking straight down: the camera's up vector points the way we face.
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.ProjectOnPlane(
                playerCamera.transform.up,
                Vector3.up);
        }

        placementRotation = Quaternion.LookRotation(
            flatForward.normalized,
            Vector3.up);

        // Calculate the item's position on the surface.
        placementPosition = GetPlacementPosition(hit);

        // Check the surface, placement radius, and available space.
        canPlace = CheckPlacement(hit) &&
                   !CheckForObjectCollision() &&
                   !OverlapsPlayer();

        // Update and show the preview.
        UpdatePreview();
    }

    /// <summary>
    /// Calculates the position where the held item should rest on a surface.
    /// </summary>
    /// <param name="hit">The surface hit by the placement raycast.</param>
    /// <returns>The calculated world position for the held item.</returns>
    private Vector3 GetPlacementPosition(RaycastHit hit)
    {
        return hit.point + Vector3.up * (GetBottomOffset() + placementPadding);
    }

    /// <summary>
    /// Returns the axis-aligned bounds the held collider would have if the item
    /// were rotated to the placement rotation, expressed relative to the item's
    /// pivot. Box colliders are handled exactly (any offset, rotation or scale);
    /// other collider types use their current bounds as an approximation.
    /// </summary>
    private Bounds GetPlacedBoundsRelativeToPivot()
    {
        Vector3 center;
        Vector3 halfX, halfY, halfZ;

        if (heldCollider is BoxCollider box)
        {
            Transform t = box.transform;
            Vector3 half = box.size * 0.5f;

            center = t.TransformPoint(box.center);
            halfX = t.TransformVector(half.x, 0f, 0f);
            halfY = t.TransformVector(0f, half.y, 0f);
            halfZ = t.TransformVector(0f, 0f, half.z);
        }
        else
        {
            Bounds b = heldCollider.bounds;

            center = b.center;
            halfX = new Vector3(b.extents.x, 0f, 0f);
            halfY = new Vector3(0f, b.extents.y, 0f);
            halfZ = new Vector3(0f, 0f, b.extents.z);
        }

        // Rotate from the current (held) pose to the placement pose.
        Quaternion delta = placementRotation *
            Quaternion.Inverse(heldItem.transform.rotation);

        halfX = delta * halfX;
        halfY = delta * halfY;
        halfZ = delta * halfZ;

        // The AABB of a rotated box is the sum of its rotated half-axes.
        Vector3 extents = new Vector3(
            Mathf.Abs(halfX.x) + Mathf.Abs(halfY.x) + Mathf.Abs(halfZ.x),
            Mathf.Abs(halfX.y) + Mathf.Abs(halfY.y) + Mathf.Abs(halfZ.y),
            Mathf.Abs(halfX.z) + Mathf.Abs(halfY.z) + Mathf.Abs(halfZ.z)
        );

        return new Bounds(
            delta * (center - heldItem.transform.position),
            extents * 2f
        );
    }

    /// <summary>
    /// Returns how far the item's pivot sits above the bottom of its collider
    /// once the item is rotated to the placement rotation. This does not depend
    /// on how the item is currently tilted in the player's hands.
    /// </summary>
    private float GetBottomOffset()
    {
        if (heldCollider == null)
        {
            return 0f;
        }

        return -GetPlacedBoundsRelativeToPivot().min.y;
    }

    /// <summary>
    /// Returns the world-space bounds the held collider would occupy if the
    /// item were placed at the current placement position and rotation.
    /// </summary>
    private Bounds GetPlacedBounds()
    {
        Bounds local = GetPlacedBoundsRelativeToPivot();

        return new Bounds(placementPosition + local.center, local.size);
    }

    /// <summary>
    /// Checks whether the item would be placed on or inside the player.
    /// </summary>
    /// <returns>True if the placement location overlaps the player.</returns>
    private bool OverlapsPlayer()
    {
        if (heldCollider == null)
        {
            return false;
        }

        Bounds placed = GetPlacedBounds();
        placed.Expand(playerClearance * 2f);

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            // The held item is parented under the player, so skip its colliders.
            if (!col.enabled || col.isTrigger ||
                col.transform.IsChildOf(heldItem.transform))
            {
                continue;
            }

            if (placed.Intersects(col.bounds))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether the item's placement radius fits on the surface.
    /// </summary>
    /// <param name="hit">The surface hit by the placement raycast.</param>
    /// <returns>True if the item can be placed at the current location.</returns>
    private bool CheckPlacement(RaycastHit hit)
    {
        float radius = heldItem.PlacementRadius;
        Vector3 center = hit.point;
        Vector3 normal = hit.normal;

        // Create directions across the surface for our radius checks.
        Vector3 right = Vector3.Cross(normal, Vector3.forward);

        // Handle surfaces whose normal is nearly parallel to Vector3.forward.
        if (right.sqrMagnitude < 0.01f)
        {
            right = Vector3.Cross(normal, Vector3.right);
        }

        right.Normalize();

        Vector3 forward = Vector3.Cross(right, normal).normalized;

        // Check the center and four points around the placement radius.
        Vector3[] testPoints =
        {
            center,
            center + right * radius,
            center - right * radius,
            center + forward * radius,
            center - forward * radius
        };

        foreach (Vector3 point in testPoints)
        {
            // If any point falls outside the placement surface, placement is invalid.
            if (!IsPointOnSurface(point, normal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a point lies on a valid placement surface.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="surfaceNormal">The normal direction of the surface.</param>
    /// <returns>True if the point is on a valid placement surface.</returns>
    private bool IsPointOnSurface(Vector3 point, Vector3 surfaceNormal)
    {
        // Start slightly above the surface and raycast toward it.
        Vector3 rayStart = point + surfaceNormal * 0.1f;

        // The point is valid if the ray hits a placement surface.
        return Physics.Raycast(
            rayStart,
            -surfaceNormal,
            0.2f,
            placementSurfaceLayer
        );
    }

    /// <summary>
    /// Checks whether the item would overlap another object at its placement
    /// location.
    /// </summary>
    /// <returns>True if the placement location contains another object.</returns>
    private bool CheckForObjectCollision()
    {
        if (heldCollider == null)
        {
            return false;
        }

        Transform itemTransform = heldItem.transform;
        Quaternion inverseItemRotation = Quaternion.Inverse(itemTransform.rotation);

        // Where the collider's bounds center sits relative to the item's pivot,
        // re-expressed at the placement rotation (no scale distortion).
        Vector3 centerOffset = inverseItemRotation *
            (heldCollider.bounds.center - itemTransform.position);
        Vector3 colliderCenter = placementPosition + placementRotation * centerOffset;

        // The collider's own transform pose at the placement location.
        Vector3 colliderPosition = placementPosition + placementRotation *
            (inverseItemRotation *
                (heldCollider.transform.position - itemTransform.position));
        Quaternion colliderRotation = placementRotation * inverseItemRotation *
            heldCollider.transform.rotation;

        // Triggers are ignored at the query level.
        Collider[] nearbyColliders = Physics.OverlapSphere(
            colliderCenter,
            heldCollider.bounds.extents.magnitude,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider other in nearbyColliders)
        {
            // Ignore any collider belonging to the held item itself.
            if (other.transform.IsChildOf(itemTransform))
            {
                continue;
            }

            // Ignore the player and its child colliders.
            if (other.transform.IsChildOf(transform))
            {
                continue;
            }

            // Ignore the surface the item is supposed to sit on.
            if (((1 << other.gameObject.layer) & placementSurfaceLayer) != 0)
            {
                continue;
            }

            if (Physics.ComputePenetration(
                    heldCollider,
                    colliderPosition,
                    colliderRotation,
                    other,
                    other.transform.position,
                    other.transform.rotation,
                    out _,
                    out _))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Updates the placement preview's position and appearance.
    /// </summary>
    private void UpdatePreview()
    {
        if (placementPreview == null)
        {
            return;
        }

        // Show the preview when looking at a placement surface.
        placementPreview.SetActive(true);

        // Move the preview to the calculated placement location.
        placementPreview.transform.SetPositionAndRotation(
            placementPosition,
            placementRotation
        );

        // Show green for valid placement and red for invalid placement.
        SetPreviewMaterial(
            canPlace ? validPreviewMaterial : invalidPreviewMaterial
        );
    }

    /// <summary>
    /// Applies a material to every renderer in the placement preview.
    /// </summary>
    /// <param name="material">The material to apply.</param>
    private void SetPreviewMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        Renderer[] renderers =
            placementPreview.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }

    /// <summary>
    /// Places the held item if the current placement location is valid.
    /// </summary>
    private void TryPlaceItem()
    {
        // Do nothing if the player is not currently aiming at a valid location.
        if (!canPlace)
        {
            return;
        }

        // Tell the item to move to the calculated placement position.
        heldItem.Place(placementPosition, placementRotation);

        // Remove the preview after successfully placing the object.
        DestroyPlacementPreview();

        // The player is no longer holding anything.
        heldItem = null;
        heldCollider = null;
        canPlace = false;
    }
}