using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTablesGenerator : MonoBehaviour
{
    [SerializeField] private GameObject tablePrefab;

    //Par défaut les tables vont se placer au point d'ancrage des tiles, c.a.d. dans le coin inférieur gauche. Pour compenser cela on applique un offset de 0.5 en x et y
    [SerializeField] private Vector2 offset = new(0.5f, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePosition))
                {
                    Vector3 tileWorldPosition = tilemap.CellToWorld(tilePosition);
                    Instantiate(tablePrefab, new Vector2(tileWorldPosition.x, tileWorldPosition.y) + offset, Quaternion.identity);
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
