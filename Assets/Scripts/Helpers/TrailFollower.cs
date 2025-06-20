using UnityEngine;

public class TrailFollower : MonoBehaviour
{
    public Transform targetTransform;
    public Color trailColor = Color.white;
    public float trailTime = 1.0f;
    public float trailWidth = 0.1f;

    private TrailRenderer trailRenderer;

    void Start()
    {
        // Add a TrailRenderer component to the game object
        trailRenderer = gameObject.AddComponent<TrailRenderer>();

        // Set the TrailRenderer's properties
        trailRenderer.time = trailTime;
        trailRenderer.widthMultiplier = trailWidth;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.material.color = trailColor;
    }

    void Update()
    {
        // Update the TrailRenderer's position to follow the target transform
        trailRenderer.transform.position = targetTransform.position;
    }
}