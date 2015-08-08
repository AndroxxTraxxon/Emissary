using UnityEngine;
using System.Collections.Generic;

namespace Emissary
{
    public class Grid : MonoBehaviour
    {

        public bool displayGridGizmos;
        //public Transform player;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float nodeRadius;
        public TerrainType[] walkableRegions;
        LayerMask walkableMask;
        Node[,] grid;
        Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        public Transform player;


        float nodeDiameter;
        int gridSizeX, gridSizeY;

        void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            foreach (TerrainType region in walkableRegions)
            {
                walkableMask.value |= region.terrainMask.value;
                walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
            }

            CreateGrid();
        }

        public int MaxSize
        {
            get
            {
                return gridSizeX * gridSizeY;
            }
        }
        void OnDrawGizmos()
        //draws everything on the screen in the scene view for you. Makes it purty.
        {
            //Draw the bounding box of the grid view.
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            //Draw the grid of nodes. Very purty.

            if (grid != null && displayGridGizmos)
            {
                Node playerNode = null;
                if (player != null)
                    playerNode = getNodeFromWorldPoint(player.position);
                foreach (Node n in grid)
                {
                    //Baseline: Walkable --> white; Not Walkable --> red
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    //Player Position --> cyan
                    if (playerNode == n) Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter * .95f));
                }

            }
        }

        public Node getNodeFromWorldPoint(Vector3 worldPosition)
        //returns a node given a global position.
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            return this.grid[x, y];
        }

        public List<Node> getNeighborNodes(Node node)
        //returns a list of the (up to) eight neighboring nodes, adjacent and diagonal nodes.
        {
            List<Node> nodes = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        nodes.Add(grid[checkX, checkY]);
                    }
                }
            }

            return nodes;
        }

        void CreateGrid()
        //initializes the grid and determines if each node is walkable, then positions the nodes in worldspace.
        {
            grid = new Node[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    int movementPenalty = 0;

                    //raycast code
                    if (walkable)
                    {
                        Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100, walkableMask))
                        {
                            walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        }
                    }

                    grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                }

            }
        }
        [System.Serializable]
        public class TerrainType
        {
            public LayerMask terrainMask;
            public int terrainPenalty;
        }

    }
    
}