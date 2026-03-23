using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Gère les interactions VR avec la plante (saisie, manipulation, etc.)
/// Permet à l'utilisateur d'interagir physiquement avec la plante en VR
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class VRPlantInteraction : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private PlantBehavior plantBehavior;

    [Header("Feedback visuel")]
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private Color hoverColor = Color.cyan;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("Feedback haptique")]
    [SerializeField] private float hapticAmplitude = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 0.5f;
    [SerializeField] private bool canBeGrabbed = false;

    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isHovered = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        plantBehavior = GetComponent<PlantBehavior>();
        objectRenderer = GetComponentInChildren<Renderer>();

        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.AddListener(OnHoverEntered);
            grabInteractable.hoverExited.AddListener(OnHoverExited);
            grabInteractable.activated.AddListener(OnActivated);
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            grabInteractable.hoverExited.RemoveListener(OnHoverExited);
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        UpdateVisualFeedback(true);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;
        UpdateVisualFeedback(false);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        UpdateVisualFeedback(true, true);

        if (highlightObject != null)
            highlightObject.SetActive(true);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        UpdateVisualFeedback(isHovered, false);

        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (plantBehavior != null)
            plantBehavior.InteractWithPlant();
    }

    private void UpdateVisualFeedback(bool isHighlighted, bool isSelected = false)
    {
        if (objectRenderer != null && originalMaterial != null)
        {
            if (isSelected)
                objectRenderer.material.color = selectedColor;
            else if (isHighlighted)
                objectRenderer.material.color = hoverColor;
            else
                objectRenderer.material.color = originalMaterial.color;
        }
    }
}
