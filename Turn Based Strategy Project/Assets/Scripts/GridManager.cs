using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GridManager : MonoBehaviour {

    
    public GameObject Hex;
    public GameObject Line;
   
    public static int gridWidthInHexes = 10;
    public static int gridHeightInHexes = 10;

    
    public Tile selectedTile = null;
    public TileBehaviour originTileTB = null;
    public TileBehaviour destTileTB = null;

    public static GridManager instance = null;
    public Dictionary<Point, TileBehaviour> board;

    List<GameObject> path;

    //void Awake()
    //{
    //    instance = this;
    //    setSizes();
    //    createGrid();
    //    generateAndShowPath();
    //}

    //Hexagon tile width and height in game world
    private float hexWidth;
    private float hexHeight;

    //Method to initialise Hexagon width and height
    void setSizes()
    {
        //renderer component attached to the Hex prefab is used to get the current width and height
        hexWidth = Hex.GetComponent<Renderer>().bounds.size.x;
        hexHeight = Hex.GetComponent<Renderer>().bounds.size.y;
    }

    //Method to calculate the position of the first hexagon tile
    //The center of the hex grid is (0,0,0)
    Vector3 calcInitPos()
    {
        Vector3 initPos;
        //the initial position will be in the left upper corner
        initPos = new Vector3(-hexWidth * gridWidthInHexes / 2f + hexWidth / 2, gridHeightInHexes / 2f * hexHeight - hexHeight / 2, 0);

        return initPos;
    }

    //method used to convert hex grid coordinates to game world coordinates
    public Vector3 calcWorldCoord(Vector2 gridPos)
    {
        //Position of the first hex tile
        Vector3 initPos = calcInitPos();
        //Every second row is offset by half of the tile width
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;

        float x = initPos.x + offset + gridPos.x * hexWidth;
        //Every new line is offset in z direction by 3/4 of the hexagon height
        float y = initPos.y - gridPos.y * hexHeight * 0.75f;
        return new Vector3(x, y, 0);
    }

    //Finally the method which initialises and positions all the tiles
    void createGrid()
    {
        Vector2 gridSize = new Vector2(gridWidthInHexes, gridHeightInHexes);

        //Game object which is the parent of all the hex tiles
        GameObject hexGridGO = new GameObject("HexGrid2");

        board = new Dictionary<Point, TileBehaviour>();

        for (float y = 0; y < gridSize.y; y++)
        {
            for (float x = 0; x < gridSize.x; x++)
            {
                //GameObject assigned to Hex public variable is cloned
                GameObject hex = (GameObject)Instantiate(Hex);
                //Current position in grid
                Vector2 gridPos = new Vector2(x, y);
                hex.transform.position = calcWorldCoord(gridPos);
                hex.transform.parent = hexGridGO.transform;

                var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");
                tb.tile = new Tile((int)x - (int)(y / 2), (int)y);
                board.Add(tb.tile.Location, tb);
            }
        }
        bool equalLineLengths = (gridSize.x + 0.5) * hexWidth <= gridSize.x;
        foreach (TileBehaviour tb in board.Values)
            tb.tile.FindNeighbours(board, gridSize, equalLineLengths);
    }
   
    double calcDistance(Tile tile)
    {
        Tile destTile = destTileTB.tile;

        float deltaX = Mathf.Abs(destTile.X - tile.X);
        float deltaY = Mathf.Abs(destTile.Y - tile.Y);
        int z1 = -(tile.X + tile.Y);
        int z2 = -(destTile.X + destTile.Y);
        float deltaZ = Mathf.Abs(z2 - z1);

        return Mathf.Max(deltaX, deltaY, deltaZ);
    }
    private void DrawPath(IEnumerable<Tile> path)
    {
        if (this.path == null)
            this.path = new List<GameObject>();
        //Destroy game objects which used to indicate the path
        this.path.ForEach(Destroy);
        this.path.Clear();

        //Lines game object is used to hold all the "Line" game objects indicating the path
        GameObject lines = GameObject.Find("Lines");
        if (lines == null)
            lines = new GameObject("Lines");
        foreach (Tile tile in path)
        {
            var line = (GameObject)Instantiate(Line);
            //calcWorldCoord method uses squiggly axis coordinates so we add y / 2 to convert x coordinate from straight axis coordinate system
            Vector2 gridPos = new Vector2(tile.X + tile.Y / 2, tile.Y);
            line.transform.position = calcWorldCoord(gridPos);
            this.path.Add(line);
            line.transform.parent = lines.transform;
        }
    }
    public void generateAndShowPath()
    {
        //Don't do anything if origin or destination is not defined yet
        if (originTileTB == null || destTileTB == null)
        {
            DrawPath(new List<Tile>());
            return;
        }
        //We assume that the distance between any two adjacent tiles is 1
        
        Func<Tile, Tile, double> distance = (node1, node2) => 1;

        var path = PathFinder.FindPath(originTileTB.tile, destTileTB.tile);
        DrawPath(path);
    }

    //The grid should be generated on game start
    void Start()
    {
        instance = this;
        setSizes();
        createGrid();
        generateAndShowPath();

    }
}
