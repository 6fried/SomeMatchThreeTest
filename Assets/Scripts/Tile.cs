using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;

    public Image icon;

    public Button button;

    public Tile left => x > 0 ? Board.instance.tiles[x - 1, y]: null;
    public Tile right => x < Board.instance.width - 1 ? Board.instance.tiles[x + 1, y]: null;
    public Tile top => y > 0 ? Board.instance.tiles[x, y - 1]: null;
    public Tile bottom => y < Board.instance.height - 1 ? Board.instance.tiles[x, y + 1]: null;


    public Tile[] neighbours => new[]
    {
        left,
        top,
        right,
        bottom,
    };

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> { this };

        if (exclude == null)
        {
            exclude = new List<Tile> { this };
        }
        else
        {
            exclude.Add(this);
        }

        foreach(var neighbour in neighbours)
        {
            if(neighbour == null || exclude.Contains(neighbour) || neighbour.item != item)
            {
                continue;
            }

            result.AddRange(neighbour.GetConnectedTiles(exclude));

        }

        return result;
    }

    public Item item
    {
        get => _item;

        set
        {
            if (_item == value) return;
            
            _item = value;

            icon.sprite = _item.icon;
        }
    }

    private void Start()
    {
        button.onClick.AddListener(() => Board.instance.Select(this));
    }
}
