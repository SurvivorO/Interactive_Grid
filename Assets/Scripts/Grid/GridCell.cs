using System;
using System.Reflection.Metadata.Ecma335;
using UnityEngine;

public class GridCell : MonoBehaviour
{

    [SerializeField] private Renderer cellRenderer;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.yellow;

    public Vector2Int Coordinates { get; private set; }
    public CubeController Occupant { get; private set; }
    public bool IsOccupied => Occupant != null;


    public bool TryOccupy(CubeController cube)
    {
        if(cube == null || IsOccupied)
            return false;

        Occupant = cube;

        return true;
    }

    public void ClearOccupant()
    {
        Occupant = null;
    }

    private void Awake(){
        if (cellRenderer == null)
            cellRenderer = GetComponent<Renderer>();

        SetSelected(false);
    }

    public void Initialize(Vector2Int coordinates)
    {
        Coordinates = coordinates;
        name = $"Cell_{coordinates.x}_{coordinates.y}";
    }

    public void SetSelected(bool selected)
    {
        if (cellRenderer == null) return;

        cellRenderer.material.color = selected ? selectedColor : normalColor;
    }
}
