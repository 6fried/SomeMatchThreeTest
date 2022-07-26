using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using System;

public sealed class Board : MonoBehaviour
{
    public static Board instance { get; private set; }

    public Row[] rows;

    public Tile[,] tiles { get; private set; }

    public int width => tiles.GetLength(dimension:0);

    public int height => tiles.GetLength(dimension:1);

    private readonly List<Tile> selections = new List<Tile>();

    private const float tweenDuration = .25f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        tiles = new Tile[rows.Max(selector: row => row.tiles.Length), rows.Length];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.item = ItemDatabase.items[UnityEngine.Random.Range(0, ItemDatabase.items.Length)];

                tiles[x, y] = tile;
            }
        }
    }

    public async void Select(Tile tile)
    {
        if (!selections.Contains(tile))
        {
            if (selections.Count > 0)
            {
                if (Array.IndexOf(selections[0].neighbours, tile) != -1)
                {
                    selections.Add(tile);
                }
            }
            else
            {
                selections.Add(tile);
            }

        }

        if (selections.Count < 2)
        {
            return;
        }

        await Swap(selections[0], selections[1]);

        if (CanPoP())
        {
            Pop();
        }
        else
        {
            await Swap(selections[0], selections[1]);
        }

        selections.Clear();
    }

    public async Task Swap (Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, tweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.item;

        tile1.item = tile2.item;
        tile2.item = tile1Item;

    }

    private bool CanPoP()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (tiles[x,y].GetConnectedTiles().Skip(1).Count() >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async void Pop()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];

                var connectedTiles = tile.GetConnectedTiles();
                
                if (connectedTiles.Skip(1).Count() < 2)
                {
                    continue;
                }

                var deflateSequence = DOTween.Sequence();

                ScoreCounter.instance.score += tile.item.value * connectedTiles.Count;

                foreach (var  connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, tweenDuration));
                }

                await deflateSequence.Play().AsyncWaitForCompletion();

                var inflateSequence = DOTween.Sequence();

                foreach(var connectedTile in connectedTiles)
                {
                    connectedTile.item = ItemDatabase.items[UnityEngine.Random.Range(0, ItemDatabase.items.Length)];
                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, tweenDuration));
                }

                await inflateSequence.Play().AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }
}
