using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class CircleDrawer : MonoBehaviour
{
    public Shader circleShader;

    [Range(0.0f, 5.0f)]
    public float radius = 0.5f;
    public Vector2[] positions = {new(0, 0)};
    public Color color = new(1, 1, 1, 1);
    public Color bgColor = new(49, 77, 121, 0);
    [Range(0.0f, 0.5f)]
    public float edgeWidthPercentage = 0.2f;
    public float sdfThresh = 0.1f;

    private void Start()
    {
        CreateCircle();
    }

    private void Update()
    {
        UpdateCircle();
    }

    private void CreateCircle()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && circleShader != null)
        {
            Texture2D posTex = new(positions.Length, 1, TextureFormat.RGBAFloat, false);
            // Convert positions to texture
            for (int i = 0; i < positions.Length; i++)
            {
                posTex.SetPixel(i, 0, new Vector4(positions[i].x, positions[i].y, 0, 0));
            }
            posTex.Apply();
            meshRenderer.material = new Material(circleShader);
            meshRenderer.material.SetFloat("_Radius", radius);
            meshRenderer.material.SetInt("_NumPositions", positions.Length);
            meshRenderer.material.SetTexture("_PositionsTex", posTex);
            // meshRenderer.material.SetVector("_Center", transform.position);
            meshRenderer.material.SetVector("_Color", new Vector4(color.r, color.g, color.b, color.a));
            meshRenderer.material.SetVector("_BgColor", new Vector4(bgColor.r, bgColor.g, bgColor.b, bgColor.a));
            meshRenderer.material.SetFloat("_EdgeWidth", edgeWidthPercentage);
            meshRenderer.material.SetFloat("_Thresh", sdfThresh);
        }

        if (TryGetComponent<MeshFilter>(out var meshFilter))
        {
            Mesh circleMesh = new();

            Vector3[] vertices = new Vector3[] {
                new(-5f, -5f, 0),
                new(-5f, 5f, 0),
                new(5f, -5f, 0),
                new(5f, 5f, 0)
            };

            int[] triangles = new int[] {
                0, 1, 2,
                2, 1, 3
            };

            circleMesh.vertices = vertices;
            circleMesh.triangles = triangles;
            circleMesh.uv = new Vector2[] {
                new(0, 0),
                new(0, 1),
                new(1, 0),
                new(1, 1)
            };

            meshFilter.mesh = circleMesh;
        }
    }

    private void UpdateCircle()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && circleShader != null)
        {
            Texture2D posTex = new(positions.Length + 1, 1, TextureFormat.RGFloat, false);
            // Convert positions to texture
            for (int i = 0; i < positions.Length; i++)
            {
                posTex.SetPixel(i, 0, new Vector4(positions[i].x * 0.5f + 0.5f, positions[i].y * 0.5f + 0.5f, 0, 0));
            }
            posTex.SetPixel(positions.Length, 0, new Vector4(0.5f, 0.5f, 0, 0));
            posTex.Apply();
            
            meshRenderer.material.SetFloat("_Radius", radius);
            meshRenderer.material.SetInt("_NumPositions", positions.Length);
            meshRenderer.material.SetTexture("_PositionsTex", posTex);
            meshRenderer.material.SetVector("_Center", transform.position);
            meshRenderer.material.SetVector("_Color", new Vector4(color.r, color.g, color.b, color.a));
            meshRenderer.material.SetVector("_BgColor", new Vector4(bgColor.r, bgColor.g, bgColor.b, bgColor.a));
            meshRenderer.material.SetFloat("_EdgeWidth", edgeWidthPercentage);
            meshRenderer.material.SetFloat("_Thresh", sdfThresh);
        }
    }
}