using UnityEngine;
using UnityEngine.UIElements;

public class Renderer
{

    public Renderer(Vector2[] positions)
    {
        this.positions = positions; 
    }
    [SerializeField] private Vector2[] positions;

    
}