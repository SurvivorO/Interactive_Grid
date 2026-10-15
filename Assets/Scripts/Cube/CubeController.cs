using NUnit.Framework.Internal.Commands;
using System;
using UnityEngine;
public class CubeController : MonoBehaviour
{
    [SerializeField] private Renderer cubeRenderer;
    public Vector2Int Coordinates { get; private set; }
    public GridCell CurrentCell { get; private set; }
    public CubeTypeData Type { get; private set; }

    private void Awake()
    {
        if (!cubeRenderer) cubeRenderer = GetComponentInChildren<Renderer>();
    }
    public void Initialize(GridCell cell)
    {
        CurrentCell = cell;
        Coordinates = cell.Coordinates;
    }

    public void Initialize(GridCell cell, CubeTypeData type)
    {
        Initialize(cell);

        Type = type;
        ApplyAppearance();
    }

    private void ApplyAppearance()
    {
        if(!Type || !cubeRenderer) return;

        if (!Type.Material) cubeRenderer.material = Type.Material;
        else cubeRenderer.material.color = Type.Color;
    }

    public void MoveTo(GridCell cell)
    {
        CurrentCell = cell;
        Coordinates = cell.Coordinates;

        Vector3 position = cell.transform.position;

        Renderer tileRenderer = cell.GetComponentInChildren<Renderer>();

        Renderer cubeRenderer = GetComponentInChildren<Renderer>();


        if(tileRenderer != null && cubeRenderer != null)
        {
            position.y = tileRenderer.bounds.max.y + cubeRenderer.bounds.extents.y;
        }

        transform.position = position;

    }
}
