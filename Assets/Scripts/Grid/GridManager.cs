using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField, Min(1)] private int width = 8;
    [SerializeField, Min(1)] private int  height = 8;
    [SerializeField, Min(0.1f)] private float cellSize = 1f;


    [Header("References")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Camera mainCamera;


    [Header("Spawning")]
    [SerializeField] private CubeController cubePrefab;

    [Header("Cube Types")]
    [SerializeField] private List<CubeTypeData> cubeTypes = new List<CubeTypeData>();
    private int selectedTypeIndex;


    [Header("UI")]
    [SerializeField] private GridUI gridUI;



    private GridCell[,] cells;
    private GridCell selectedCell;
    private GridInputReader inputReader;


    private void Awake()
    {
        inputReader = GetComponent<GridInputReader>();

        if (!inputReader)
            Debug.LogError("GridInputReader component is missing.", this);

        if (!mainCamera)
            mainCamera = Camera.main;
    }


    private void OnEnable()
    {
        if (!inputReader) return;

        inputReader.SelectPressed += HandleSelection;
        inputReader.SpawnPressed += SpawnCube;
        inputReader.MoveRequested += HandleMovement;
        inputReader.ScrollRequested += CycleCubeType;
    }

    private void OnDisable()
    {
        if(!inputReader) return;

        inputReader.SelectPressed -= HandleSelection;
        inputReader.SpawnPressed -= SpawnCube;
        inputReader.MoveRequested -= HandleMovement;
        inputReader.ScrollRequested -= CycleCubeType;
    }

    private void Start()
    {
        GenerateGrid();

        if(cubeTypes.Count > 0 && cubeTypes[0])
        {
            selectedTypeIndex = 0;
            gridUI?.UpdateSelectedType(cubeTypes[selectedTypeIndex]);
        }
    }

    private void CycleCubeType(Vector2 scroll)
    {
        if(cubeTypes.Count == 0) return;

        int direction = scroll.y > 0 ? 1 : -1;
        int nextIndex = selectedTypeIndex;

        for(int i = 0; i < cubeTypes.Count; i++)
        {
            nextIndex = (nextIndex + direction + cubeTypes.Count) % cubeTypes.Count;
            if (cubeTypes[nextIndex] == null) continue;

            selectedTypeIndex = nextIndex;
            gridUI?.UpdateSelectedType(cubeTypes[selectedTypeIndex]);
            
            Debug.Log($"Selected cube type: {cubeTypes[selectedTypeIndex].TypeName}");

            return;
        }

        Debug.LogWarning($"Grid Manager: No valid cube types are assigned.", this);


    }

    private void SpawnCube()
    {   
        if(cubeTypes.Count == 0)
        {
            Debug.LogWarning("Grid Manager: No cube types configured.", this);
            return;
        }

        if (cubeTypes[selectedTypeIndex] == null)
        {
            Debug.LogWarning("Grid Manager: Selected cube type is not assigned.", this);
            return;
        }
        if (selectedCell == null) return;

        if (selectedCell.IsOccupied)
        {
            Debug.Log("Cell is already occupied.");
            return;
        }

        if(cubePrefab == null)
        {
            Debug.LogError("Grid Manager: Cube prefab is missing.", this);
            return;
        }   

        CubeController cube = Instantiate(cubePrefab, selectedCell.transform.position, Quaternion.identity);

        cube.Initialize(selectedCell, cubeTypes[selectedTypeIndex]);

        if (!selectedCell.TryOccupy(cube))
        {
            Destroy(cube.gameObject);
            return;
        }

        Renderer tileRenderer = selectedCell.GetComponentInChildren<Renderer>();
        Renderer cubeRenderer = cube.GetComponentInChildren<Renderer>();

        if(tileRenderer != null && cubeRenderer != null)
        {
            Vector3 position = selectedCell.transform.position;
            position.y = tileRenderer.bounds.max.y + cubeRenderer.bounds.extents.y;
            cube.transform.position = position;
        }
    }

    private void HandleSelection(Vector2 pointerPosition)
    {

        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);

        if(!Physics.Raycast(ray, out RaycastHit hit))
        {
            ClearSelection();
            return;
        }

        GridCell clickedCell = hit.collider.GetComponent<GridCell>();

        if(clickedCell == selectedCell)
        {
            ClearSelection();
            return;
        }
        if (clickedCell == null)
        {   
            ClearSelection();
            return;
        }

        SelectCell(clickedCell);
    }

    private void ClearSelection()
    {
        if (!selectedCell) return;

        selectedCell.SetSelected(false);
        selectedCell = null;
    }

    private void SelectCell(GridCell cell)
    {
        if (selectedCell == cell) return;

        if (selectedCell != null) selectedCell.SetSelected(false);

        selectedCell = cell;
        selectedCell.SetSelected(true);

        Debug.Log($"Selected Cell: {selectedCell.Coordinates}");
    }

    private void GenerateGrid()
    {
        if(!cellPrefab)
        {
            Debug.LogError("GridManager: Cell prefab is not assigned.", this);
            return;
        }

        cells = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for(int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0f, z * cellSize);

                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                GridCell gridCell = cell.GetComponent<GridCell>();

                if (!gridCell)
                {
                    Debug.LogError("GridManager: Cell prefab does not have a GridCell prefab.", this);
                    Destroy(cell);
                    continue;
                }

                gridCell.Initialize(new Vector2Int(x, z));
                cells[x, z] = gridCell;
            }
        }


    }

    private void HandleMovement(Vector2 input)
    {
        Vector2Int direction;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            direction = new Vector2Int(input.x > 0 ? 1 : -1, 0);
        else
            direction = new Vector2Int(0, input.y > 0 ? 1 : -1);

        if (input == Vector2.zero) return;
        MoveAllCubes(direction);
    }


    private void MoveAllCubes(Vector2Int direction)
    {
        List<CubeController> cubesToMove = GetCubesToMove();

        if(cubesToMove.Count == 0) return;

        Dictionary<CubeController, GridCell> destinations = CalculateDestinations(cubesToMove, direction);
        ResolveBlockedMovements(cubesToMove, destinations);
        ExecuteMovement(cubesToMove, destinations);

    }

    private void ExecuteMovement(List<CubeController> cubes, Dictionary<CubeController, GridCell> destinations)
    {
        foreach (CubeController cube in cubes)
        {
            if (destinations[cube] == cube.CurrentCell) continue;

            cube.CurrentCell.ClearOccupant();
        }

        foreach (CubeController cube in cubes)
        {
            GridCell destination = destinations[cube];

            if(destination == cube.CurrentCell) continue;

            destination.TryOccupy(cube);
            cube.MoveTo(destination);
        }
    }

    private void ResolveBlockedMovements(List<CubeController> cubes, Dictionary<CubeController, GridCell> destinations)
    {
        bool changed;

        do
        {
            changed = false;

            foreach (CubeController cube in cubes)
            {
                GridCell destination = destinations[cube];

                if (destination == cube.CurrentCell) continue;

                CubeController occupant = destination.Occupant;

                if (!occupant || destinations[occupant] != destination)
                {
                    continue;
                }

                destinations[cube] = cube.CurrentCell;
                changed = true;
            }
        }
        while (changed);
    }

    private Dictionary<CubeController, GridCell> CalculateDestinations(List<CubeController> cubesToMove, Vector2Int direction)
    {
        Dictionary<CubeController, GridCell> destinations = new Dictionary<CubeController, GridCell>();

        foreach (CubeController cube in cubesToMove)
        {
            Vector2Int target = cube.Coordinates + direction;

            if (IsInsideGrid(target)) destinations[cube] = cells[target.x, target.y];
            else destinations[cube] = cube.CurrentCell;
        }

        return destinations;
    }

    private bool IsInsideGrid(Vector2Int coordinates)
    {
        return coordinates.x >= 0 && coordinates.x < width &&
            coordinates.y >= 0 && coordinates.y < height;
    }

    private List<CubeController> GetCubesToMove()
    {
        List<CubeController> cubes = new List<CubeController>();
        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < height; z++)
            {
                GridCell cell = cells[x, z];

                if (cell.IsOccupied) cubes.Add(cell.Occupant);
            }
        }

        return cubes;
    }
}
