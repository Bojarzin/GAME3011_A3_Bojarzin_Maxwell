using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    static Tile selectedTile;
    SpriteRenderer spriteRenderer;
    public Vector2Int position;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        spriteRenderer.color = Color.gray;
    }
    public void DeSelect()
    {
        spriteRenderer.color = Color.white;
    }

    private void OnMouseDown()
    {
        if (selectedTile != null)
        {
            if (selectedTile == this)
            {
                return;
            }
            selectedTile.DeSelect();

            if (Vector2Int.Distance(selectedTile.position, position) == 1)
            {
                BoardManager.Instance.SwapTiles(position, selectedTile.position);
                selectedTile = null;
            }
            else
            {
                selectedTile = this;
                Select();
            }
        }
        else
        {
            selectedTile = this;
            Select();
        }
    }
}
