using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public List<Sprite> pieceSprites;
    public GameObject tilePiece;

    public int gridDimension;
    public float distance;
    GameObject[,] board;

    public Canvas ButtonUI;
    public Canvas ScoreUI;
    public Canvas FinalUI;
    public AudioSource audioManager;

    int difficulty;

    public int movesRemaining = 50;
    public int score = 0;
    public float seconds = 100;

    public static BoardManager Instance
    {
        get;
        private set;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        board = new GameObject[gridDimension, gridDimension];
        ButtonUI.enabled = true;
        ScoreUI.enabled = false;
        FinalUI.enabled = false;

        seconds = 100;
    }

    void Update()
    {
        seconds -= Time.deltaTime;

        if (movesRemaining <= 0)
        {
            FinalUI.enabled = true;
            ScoreUI.enabled = false;
            transform.localScale = new Vector3(0, 0, 0);
        }

        if (seconds <= 0)
        {
            FinalUI.enabled = true;
            ScoreUI.enabled = false;
            transform.localScale = new Vector3(0, 0, 0);
        }
    }

    public void CreateGrid()
    {
        // Create offset to distance each tile
        Vector3 positionOffset = transform.position - new Vector3(gridDimension * distance / 2.0f, gridDimension * distance / 2.0f, 0);
        for (int row = 0; row < gridDimension; row++)
        {
            for (int col = 0; col < gridDimension; col++)
            {
                List<Sprite> possibleSprites = new List<Sprite>(pieceSprites.GetRange(0, difficulty)); // 1

                //Choose what sprite to use for this cell
                Sprite left1 = GetSpriteAt(col - 1, row); //2
                Sprite left2 = GetSpriteAt(col - 2, row);
                if (left2 != null && left1 == left2) // 3
                {
                    possibleSprites.Remove(left1); // 4
                }

                Sprite down1 = GetSpriteAt(col, row - 1); // 5
                Sprite down2 = GetSpriteAt(col, row - 2);
                if (down2 != null && down1 == down2)
                {
                    possibleSprites.Remove(down1);
                }

                // Instantiate our tile
                GameObject newTile = Instantiate(tilePiece);
                // Grab the tile's renderer and randomize its sprite
                SpriteRenderer spriteRenderer = newTile.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = possibleSprites[Random.Range(0, possibleSprites.Count)];
                // Set the board as its parent and set its position with the offset
                Tile tile = newTile.AddComponent<Tile>();
                tile.position = new Vector2Int(col, row);
                newTile.transform.parent = transform;
                newTile.transform.position = new Vector3(col * distance, row * distance, 0) + positionOffset;
                // Add this tile to the board officially
                board[col, row] = newTile;
            }
        }
    }

    Sprite GetSpriteAt(int col, int row)
    {
        // If it's outside of the grid, null
        if (col < 0 || col >= gridDimension ||
            row < 0 || row >= gridDimension)
        {
            return null;
        }

        GameObject tile = board[col, row];
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        return spriteRenderer.sprite;
    }

    SpriteRenderer GetSpriteRendererAt(int col, int row)
    {
        // If it's outside of the grid, null
        if (col < 0 || col >= gridDimension ||
            row < 0 || row >= gridDimension)
        {
            return null;
        }

        GameObject tile = board[col, row];
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        return spriteRenderer;
    }

    public void SwapTiles(Vector2Int _tile1Position, Vector2Int _tile2Position)
    {
        GameObject tile1 = board[_tile1Position.x, _tile1Position.y];
        SpriteRenderer spriteRenderer1 = tile1.GetComponent<SpriteRenderer>();

        GameObject tile2 = board[_tile2Position.x, _tile2Position.y];
        SpriteRenderer spriteRenderer2 = tile2.GetComponent<SpriteRenderer>();

        Sprite temp = spriteRenderer1.sprite;
        spriteRenderer1.sprite = spriteRenderer2.sprite;
        spriteRenderer2.sprite = temp;

        bool changeOccurs = CheckMatch();

        if (!changeOccurs)
        {
            temp = spriteRenderer1.sprite;
            spriteRenderer1.sprite = spriteRenderer2.sprite;
            spriteRenderer2.sprite = temp;
        }
        else
        {
            movesRemaining -= 1;

            do
            {
                FillHoles();
                audioManager.Play();
            }
            while (CheckMatch());
        }
    }

    bool CheckMatch()
    {
        HashSet<SpriteRenderer> matchedTiles = new HashSet<SpriteRenderer>();
        for (int row = 0; row < gridDimension; row++)
        {
            for (int col = 0; col < gridDimension; col++)
            {
                SpriteRenderer current = GetSpriteRendererAt(col, row);

                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(col, row, current.sprite);
                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(current);
                    if (horizontalMatches.Count == 2)
                    {
                        score += 50;
                    }
                    if (horizontalMatches.Count == 3)
                    {
                        score += 100;
                    }
                    if (horizontalMatches.Count == 4)
                    {
                        score += 150;
                    }
                }

                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(col, row, current.sprite);
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(current);
                    if (verticalMatches.Count == 2)
                    {
                        score += 50;
                    }
                    if (verticalMatches.Count == 3)
                    {
                        score += 100;
                    }
                    if (verticalMatches.Count == 4)
                    {
                        score += 150;
                    }
                }
            }
        }

        foreach (SpriteRenderer renderer in matchedTiles)
        {
            renderer.sprite = null;
        }

        return matchedTiles.Count > 0;
    }

    List<SpriteRenderer> FindColumnMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = col + 1; i < gridDimension; i++)
        {
            SpriteRenderer nextColumn = GetSpriteRendererAt(i, row);
            if (nextColumn.sprite != sprite)
            {
                break;
            }
            result.Add(nextColumn);
        }
        return result;
    }

    List<SpriteRenderer> FindRowMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = row + 1; i < gridDimension; i++)
        {
            SpriteRenderer nextRow = GetSpriteRendererAt(col, i);
            if (nextRow.sprite != sprite)
            {
                break;
            }
            result.Add(nextRow);
        }
        return result;
    }

    void FillHoles()
    {
        for (int col = 0; col < gridDimension; col++)
        {
            for (int row = 0; row < gridDimension; row++)
            {
                while (GetSpriteRendererAt(col, row).sprite == null)
                {
                    SpriteRenderer current = GetSpriteRendererAt(col, row);
                    SpriteRenderer next = current;

                    for (int filler = row; filler < gridDimension - 1; filler++)
                    {
                        next = GetSpriteRendererAt(col, filler + 1);
                        current.sprite = next.sprite;
                        current = next;
                    }
                    next.sprite = pieceSprites[Random.Range(0, difficulty)];
                }
            }
        } 
    }

    public void CreateEasyBoard()
    {
        difficulty = 3;
        CreateGrid();
        ButtonUI.enabled = false;
        ScoreUI.enabled = true;
        seconds = 360;
    }

    public void CreateMediumBoard()
    {
        difficulty = 4;
        CreateGrid();
        ButtonUI.enabled = false;
        ScoreUI.enabled = true;
        seconds = 300;
    }

    public void CreateHardBoard()
    {
        difficulty = 5;
        CreateGrid();
        ButtonUI.enabled = false;
        ScoreUI.enabled = true;
        seconds = 240;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}