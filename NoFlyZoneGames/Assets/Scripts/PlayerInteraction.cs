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

    [Header("Preview")]
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;

    // The item currently being held by the player.
    private PickupItem heldItem;

    // The visual copy of the held item used for the placement preview.
    private GameObject placementPreview;

    // The position and rotation where the item can currently be placed.
    private Vector3 placementPosition;
    private Quaternion placementRotation;

    // Determines whether the current placement position is valid.
    private bool canPlace;

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
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange))
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

        // Store the item before creating the preview.
        heldItem = item;

        // Create the preview while the real item is still in its original state.
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

        // Keep the item's horizontal rotation while keeping it upright.
        Vector3 euler = heldItem.transform.eulerAngles;
        placementRotation = Quaternion.Euler(0f, euler.y, 0f);

        // Calculate the item's position on the surface.
        placementPosition = GetPlacementPosition(hit);

        // Check the surface, placement radius, and available space.
        canPlace = CheckPlacement(hit) && !CheckForObjectCollision();

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
        BoxCollider boxCollider = heldItem.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            // Find the bottom of the box in the item's local space.
            float localBottom = boxCollider.center.y -
                boxCollider.size.y * 0.5f;

            // Account for the item's world scale.
            float bottomOffset = -localBottom *
                heldItem.transform.lossyScale.y;

            // Place the item's bottom directly on the surface.
            return hit.point + Vector3.up *
                (bottomOffset + placementPadding);
        }

        Collider itemCollider = heldItem.GetComponent<Collider>();

        if (itemCollider == null)
        {
            return hit.point + Vector3.up * placementPadding;
        }

        // Fall back to the existing calculation for non-box colliders.
        float fallbackOffset =
            heldItem.transform.position.y -
            itemCollider.bounds.min.y;

        return hit.point + Vector3.up *
            (fallbackOffset + placementPadding);
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
        Collider itemCollider = heldItem.GetComponent<Collider>();

        if (itemCollider == null)
        {
            return false;
        }

        Vector3 localCenter = heldItem.transform.InverseTransformPoint(
            itemCollider.bounds.center
        );

        Vector3 colliderCenter = placementPosition +
            placementRotation * localCenter;

        Collider[] nearbyColliders = Physics.OverlapSphere(
            colliderCenter,
            itemCollider.bounds.extents.magnitude
        );

        foreach (Collider other in nearbyColliders)
        {
            // Ignore the held item's own collider.
            if (other == itemCollider)
            {
                continue;
            }

            // Ignore the player and its child colliders.
            if (other.transform == transform || other.transform.IsChildOf(transform))
            {
                continue;
            }

            // Ignore the player.
            if (other.transform.IsChildOf(transform))
            {
                continue;
            }

            // Ignore the surface the item is supposed to sit on.
            if (((1 << other.gameObject.layer) & placementSurfaceLayer) != 0)
            {
                continue;
            }

            // Check the actual collider shapes for an overlap.
            if (Physics.ComputePenetration(
                    itemCollider,
                    colliderCenter,
                    placementRotation,
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
        canPlace = false;
    }
}