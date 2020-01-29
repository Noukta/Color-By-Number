using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePooler : MonoBehaviour {

    public Tile tilePrefab;

    public int poolAmount = 20;

    private void Start()
    {
        for(int i = 0; i < poolAmount; i++)
        {
            Tile tile = Instantiate(tilePrefab);
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(transform);
        }
    }

    public Tile GetPooledTile()
    {
        if (transform.childCount > 0)
            return transform.GetChild(0).GetComponent<Tile>();

        Tile tile = Instantiate(tilePrefab);
        return tile;
    }

    public void FreeTile(Tile tile)
    {
        tile.gameObject.SetActive(false);
        tile.transform.SetParent(transform);
    }
}
