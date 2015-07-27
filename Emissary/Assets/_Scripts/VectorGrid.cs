using UnityEngine;
using System.Collections.Generic;

namespace Emissary
{
    public class VectorGrid : MonoBehaviour
    {
        public bool displayGridGizmos;
        //public Transform player;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float VectorNodeRadius;
        public TerrainType[] walkableRegions;
        LayerMask walkableMask;
        public VectorNode[,] grid;
        Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();


        float VectorNodeDiameter;
        public int gridSizeX, gridSizeY;

        void Awake()
        {
            VectorNodeDiameter = VectorNodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / VectorNodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / VectorNodeDiameter);

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

            //Draw the grid of VectorNodes. Very purty.

            if (grid != null && displayGridGizmos)
            {
                foreach (VectorNode n in grid)
                {
                    //Baseline: Walkable --> white; Not Walkable --> red
                    Gizmos.color = new Color(1- n.gCost / 255f,1 -  n.gCost / 255f, 1);
                    //Player Position --> cyan
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (VectorNodeDiameter * .195f));
                    Gizmos.DrawRay(n.worldPosition, n.flowDirection * VectorNodeDiameter);
                }

            }
        }

        public VectorNode getVectorNodeFromWorldPoint(Vector3 worldPosition)
        //returns a VectorNode given a global position.
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            return this.grid[x, y];
        }

        public List<VectorNode> getNeighborVectorNodes(VectorNode VectorNode)
        //returns a list of the (up to) eight neighboring VectorNodes, adjacent and diagonal VectorNodes.
        {
            List<VectorNode> VectorNodes = new List<VectorNode>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = VectorNode.gridX + x;
                    int checkY = VectorNode.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        VectorNodes.Add(grid[checkX, checkY]);
                    }
                }
            }

            return VectorNodes;
        }

        void CreateGrid()
        //initializes the grid and determines if each VectorNode is walkable, then positions the VectorNodes in worldspace.
        {
            grid = new VectorNode[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * VectorNodeDiameter + VectorNodeRadius) + Vector3.forward * (y * VectorNodeDiameter + VectorNodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, VectorNodeRadius, unwalkableMask));
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

                    grid[x, y] = new VectorNode(walkable, worldPoint, x, y, movementPenalty);
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