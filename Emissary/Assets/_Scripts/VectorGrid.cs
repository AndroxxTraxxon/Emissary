using UnityEngine;
using System.Collections.Generic;
using System;

namespace Emissary
{
    public class VectorGrid
    {
        //public Transform player;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float VectorNodeRadius;
        public TerrainType[] walkableRegions;
        internal LayerMask walkableMask;
        internal GridRegion[,] regions;
        public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        Vector3 RegionStandardSize;
        Vector3 position;
        public Vector3 target;
        float VectorNodeDiameter;
        public int gridSizeX, gridSizeY;
        List<GridRegion> regionList;
        public List<Unit> assignedUnits;


        public VectorGrid(Vector3 position, Vector2 gridSize, float nodeRadius, TerrainType[] walkableAreas, Dictionary<int,int> walkRegDict, LayerMask unwalkableMask){

            //taking in the input.
            this.position = position;
            gridWorldSize = gridSize;
            VectorNodeRadius = nodeRadius;
            VectorNodeDiameter = VectorNodeRadius * 2;
            walkableRegions = walkableAreas;
            walkableRegionsDictionary = walkRegDict;
            this.unwalkableMask = unwalkableMask;

            RegionStandardSize = new Vector3(GridRegion.STANDARD_SIZE * VectorNodeDiameter, 0, GridRegion.STANDARD_SIZE * VectorNodeDiameter);
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / VectorNodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / VectorNodeDiameter);
            regionList = new List<GridRegion>();
            assignedUnits = new List<Unit>();

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

        

        public bool TryGetNearestWalkableNode(Vector3 worldPosition, out VectorNode node)
        {
            node = GetNodeFromWorldPoint(worldPosition);
            if (node.walkable)
            {
                return true;
            }
            else
            {
                Queue<VectorNode> nodes = new Queue<VectorNode>();
                foreach(VectorNode vn in GetAdjacentNodes(node))
                {
                    nodes.Enqueue(vn);
                }
                int radius = 2;//this value will change how far the unit looks for neighboring nodes. 
                int count = 0;
                while(nodes.Count > 0 && count++ < (Mathf.Pow(2 * radius + 1, 2)-1))
                {
                    node = nodes.Dequeue();
                    if (node.walkable)
                    {
                        return true;
                    }

                    foreach (VectorNode vn in GetAdjacentNodes(node))
                    {
                        nodes.Enqueue(vn);
                    }
                }
            }
            Debug.Log("This node can't find a friend around here!");
            return false;
        }

        public bool TryGetNearestOrientedWalkableNode(Vector3 worldPosition, out VectorNode node)
        {
            node = GetNodeFromWorldPoint(worldPosition);
            if (node.walkable && node.region.oriented)
            {
                return true;
            }
            else
            {
                Queue<VectorNode> nodes = new Queue<VectorNode>();
                foreach (VectorNode vn in GetAdjacentNodes(node))
                {
                    nodes.Enqueue(vn);
                }
                int radius = 2;//this value will change how far the unit looks for neighboring nodes. 
                int count = 0;
                while (nodes.Count > 0 && count++ < (Mathf.Pow(2 * radius + 1, 2) - 1))
                {
                    node = nodes.Dequeue();
                    if (node.walkable && node.region.oriented)
                    {
                        return true;
                    }

                    foreach (VectorNode vn in GetAdjacentNodes(node))
                    {
                        nodes.Enqueue(vn);
                    }
                }
            }
            Debug.Log("This node can't find a friend around here!");
            return false;
        }

        public VectorNode GetNodeFromWorldPoint(Vector3 worldPosition)
        //returns a VectorNode given a global position.
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            //Debug.Log("Grid Position: " + x + ", " + y);
            return GetNode(x, y);
        }

        public VectorNode GetNode(int x, int y)
            //returns node at grid location x,y 
        {
            int regionX = x / GridRegion.STANDARD_SIZE;
            int regionY = y / GridRegion.STANDARD_SIZE;
            //Debug.Log("Region: " + regionX + ", " + regionY);
            //Debug.Log(regions[regionX, regionY].ToString());
            x = x % GridRegion.STANDARD_SIZE;
            y = y % GridRegion.STANDARD_SIZE;
            //Debug.Log("Region Position: " + x + ", " + y);
           // Debug.Log(regions[regionX, regionY].getNode(x, y));
            return this.regions[regionX, regionY].getNode(x, y);
        }

        public VectorNode GetNode(float x, float y)
        {
            return GetNode((int)x, (int)y);
        }

        public VectorNode GetNode(Vector2 Position)
        {
            return GetNode(Position.x, Position.y);
        }
        public List<VectorNode> GetNeighborNodes(VectorNode node)
        //returns a list of the (up to) eight neighboring VectorNodes, adjacent and diagonal VectorNodes.
        {
            List<VectorNode> VectorNodes = new List<VectorNode>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = (int)node.GridPosition.x + x;
                    int checkY = (int)node.GridPosition.y + y;
                    //Debug.Log("X: " + checkX + ", Y: " + checkY);
                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        VectorNodes.Add(GetNode(checkX, checkY));
                    }
                }
            }

            return VectorNodes;
        }

        public List<VectorNode> GetAdjacentNodes(VectorNode node)
            //returns the list of (up to) four adjacent nodes immediately up, down, left, and right of the input node
        {
            List<VectorNode> nodes = new List<VectorNode>();
            Vector2 nodePos = node.GridPosition;
            if (nodePos.y < gridSizeY - 1)nodes.Add(GetNode((int)(Vector2.up.x + nodePos.x), (int)(Vector2.up.y + nodePos.y)));
            if (nodePos.y > 0) nodes.Add(GetNode((int)(Vector2.down.x + nodePos.x), (int)(Vector2.down.y + nodePos.y)));
            if (nodePos.x < gridSizeY - 1) nodes.Add(GetNode((int)(Vector2.right.x + nodePos.x), (int)(Vector2.right.y + nodePos.y)));
            if (nodePos.x > 0) nodes.Add(GetNode((int)(Vector2.left.x + nodePos.x), (int)(Vector2.left.y + nodePos.y)));
            return nodes;
        }

        void CreateGrid()
        //initializes the grid and determines if each VectorNode is walkable, then positions the VectorNodes in worldspace.
        {
            int rcx = (int)Mathf.Ceil(gridSizeX * 1.0f / GridRegion.STANDARD_SIZE - 0.00005f);
            int rcy = (int)Mathf.Ceil(gridSizeY * 1.0f / GridRegion.STANDARD_SIZE - 0.00005f);

            int lastWidth = GridRegion.STANDARD_SIZE;
            
            int lastHeight = GridRegion.STANDARD_SIZE;

            lastWidth = (gridSizeX % GridRegion.STANDARD_SIZE == 0) ? lastWidth :(gridSizeX % GridRegion.STANDARD_SIZE);
            lastHeight = (gridSizeY % GridRegion.STANDARD_SIZE == 0) ? lastHeight : (gridSizeY % GridRegion.STANDARD_SIZE);
            
            //Debug.Log(rcx + ", " + rcy);
            //Debug.Log(lastHeight);
            //Debug.Log(lastWidth);

            regions = new GridRegion[rcx, rcy];
            Vector3 worldBottomLeft = position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
            
            for (int x = 0; x < rcx; x++)
            {
                for (int y = 0; y < rcy; y++)
                {
                    int width = GridRegion.STANDARD_SIZE;
                    int height = GridRegion.STANDARD_SIZE;
                    if (x >= rcx - 1)
                    {
                        width = lastWidth;
                    }

                    if (y >= rcy - 1)
                    {
                        height = lastHeight;
                    }
                    //Debug.Log("Radius:" + VectorNodeRadius);
                    //Debug.Log("X: " + x + ", Y: " + y);
                    //Debug.Log("Width: " + width + ", Height: " + height);
                    //Debug.Log(worldBottomLeft);
                    float centerX = worldBottomLeft.x + width * VectorNodeRadius + x * RegionStandardSize.x;
                    float centerZ = worldBottomLeft.z + height * VectorNodeRadius + y * RegionStandardSize.z;

                    //Debug.Log(centerX + ", " + centerZ);
                    regions[x, y] = new GridRegion(x,y, width, height, VectorNodeDiameter, new Vector3(centerX, 0, centerZ), this);
                    regionList.Add(regions[x, y]);
                    
                    /*
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
                     */
                }

            }
        }

        public GridRegion GetRegion(int x, int y)
            //returns the GridRegion at x,y
        {
            return regions[x, y];
        }

        public GridRegion GetRegionFromGridLocation(int gridX, int gridY)
            //returns the region that contains the node with location x,y
        {
            return GetNode(gridX, gridY).region;
        }

        public List<GridRegion> listRegions()
            //returns a list of all regions in the grid
        {
            return regionList;
        }

        public List<GridRegion> listRegionsAlongPath(Vector3[] path)
            //returns a list of all regions which contain nodes along the given path
        {
            List<GridRegion> list = new List<GridRegion>();
            foreach (Vector3 loc in path)
            {
                VectorNode node = GetNodeFromWorldPoint(loc);
                if (!list.Contains(node.region))
                {
                    list.Add(node.region);
                }
            }

            return list;
        }

        public override string ToString()
        {
            return "V-Grid: Destination " + target;
        }

        public void DrawGizmos()
        //draws everything on the screen in the scene view for you. Makes it purty.
        {
            //Draw the bounding box of the grid view.
            Gizmos.DrawWireCube(position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            foreach (GridRegion region in regions)
            {
                region.DrawGizmos();
            }
            if(target != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(target, Vector3.one * (VectorNodeRadius));
            }
        }

        internal void ForceDrawGizmos()
        {
            Gizmos.DrawWireCube(position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            foreach (GridRegion region in regions)
            {
                region.ForceDrawGizmos();
            }
            if (target != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(target, Vector3.one * (VectorNodeRadius));
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