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
        // Editor placement helper only -- at runtime this fights any script animating a child's
        // position (e.g. BrickController.MoveTo easing bricks down each wave), snapping it back
        // to the nearest cell every frame before the animation ever gets to play.
        if (Application.isPlaying) return;

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