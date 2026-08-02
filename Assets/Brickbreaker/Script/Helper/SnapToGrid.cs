using UnityEngine;

[ExecuteAlways]
public class SnapToGrid : MonoBehaviour
{
    private Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        if (grid == null) grid = GetComponent<Grid>();
        if (grid == null) return;

        foreach (Transform child in transform)
        {
            Vector3Int cell = grid.WorldToCell(child.position);
            Vector3 cellOrigin = grid.CellToWorld(cell); // bottom-left corner of the cell
            Vector3 center = cellOrigin + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0f);

            child.position = center;
        }
    }
}