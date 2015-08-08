using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
     * 
     * Resources: 
     * Vector Field algorithm
     * http://gamedevelopment.tutsplus.com/tutorials/understanding-goal-based-vector-field-pathfinding--gamedev-9007
     * 
     * How PA does this stuffs:
     * https://www.youtube.com/watch?v=5Qyl7h7D1Q8&feature=youtu.be&t=24m24s
     * 
*/

namespace Emissary
{
    public class VectorField : MonoBehaviour 
    {
        static double rootTwo = Mathf.Sqrt(2f);
        public VectorGrid defaultGrid;
        public Vector2 worldSize;
        private Vector3 lastTarget;
        public float nodeRadius;
        public LayerMask unwalkableMask;
        public TerrainType[] walkableRegions;
        public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        public Dictionary<Vector3, VectorGrid> gridDict;
        public List<Vector3> ActiveTargets;
        public static int distanceFactor = 10;
        public bool displayGridGizmos;
        public static VectorField instance;

        // Use this for initialization
        void Awake () {
            instance = this;
            gridDict = new Dictionary<Vector3, VectorGrid>();
            ActiveTargets = new List<Vector3>();
            defaultGrid = new VectorGrid(transform.position, worldSize, nodeRadius, walkableRegions, walkableRegionsDictionary, unwalkableMask);

        }

        // Update is called once per frame
        void Update () {
	        
	    }

        public void GenerateDictionaryDefinition(Vector3 location)
        {
            ActiveTargets.Add(location);
            gridDict.Add(location, new VectorGrid(transform.position, worldSize, nodeRadius, walkableRegions, walkableRegionsDictionary, unwalkableMask));
            Debug.Log("Grid Initialization success: " + gridDict.ContainsKey(location));
            gridDict[location].target = location;
            Debug.Log(gridDict[location].target);
            //Debug.Log(gridDict[location]);
        }

        public IEnumerator UpdateValues(Vector3 target)
        {
            lastTarget = target;
            gridDict[target].target = target;
            //VectorGrid grid = gridDict[target];
            int floorX = Mathf.FloorToInt(target.x) + 1;
            int ceilX = Mathf.CeilToInt(target.x) + 1;
            int floorZ = Mathf.FloorToInt(target.z) + 1;
            int ceilZ = Mathf.CeilToInt(target.z) + 1;
            if (floorX == ceilX)
            {
                //ceilX++;
            }
            if (floorZ == ceilZ)
            {
                //ceilZ++;
            }

            Queue<VectorNode> openSet = new Queue<VectorNode>();
            HashSet<VectorNode> closedSet = new HashSet<VectorNode>();
            foreach(GridRegion region in gridDict[target].regions)
            {
                region.oriented = true;
            }
            VectorNode targetNode = gridDict[target].GetNodeFromWorldPoint(new Vector3(floorX, 0, floorZ));
            //Debug.Log(targetNode);
            targetNode.gCost = 0;
            openSet.Enqueue(targetNode);
            targetNode = gridDict[target].GetNodeFromWorldPoint(new Vector3(floorX, 0, ceilZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            targetNode = gridDict[target].GetNodeFromWorldPoint(new Vector3(ceilX, 0, floorZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            targetNode = gridDict[target].GetNodeFromWorldPoint(new Vector3(ceilX, 0, ceilZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            yield return null;
            while (openSet.Count > 0)
            {
                VectorNode currentNode = openSet.Dequeue();
                closedSet.Add(currentNode);
                foreach (VectorNode node in gridDict[target].GetAdjacentNodes(currentNode))
                {

                    //Debug.Log(node);
                    if (node.walkable)
                    {
                        int cost = currentNode.gCost + distanceFactor;
                        if (!closedSet.Contains(node) || node.gCost > cost)
                        {
                            node.parent = currentNode;
                            node.gCost = cost + node.movementPenalty;
                            //Debug.Log(node.gCost);
                            if (!openSet.Contains(node) && !closedSet.Contains(node))
                            {
                                openSet.Enqueue(node);
                            }
                        }
                    }
                }
            }
            yield return null;
            OrientGrid(gridDict[target], closedSet);
            yield return null;
            foreach(Unit unit in gridDict[target].assignedUnits)
            {
                unit.InitiateMovement();
            }
        }

        public IEnumerator UpdateValues(Vector3[] path)
        {
            Vector3 location = path[path.Length - 1];
            lastTarget = location;
            gridDict[location].target = location;
            List<GridRegion> regions = gridDict[location].listRegionsAlongPath(path);
            int floorX = Mathf.FloorToInt(location.x) + 1;
            int ceilX = Mathf.CeilToInt(location.x) + 1;
            int floorZ = Mathf.FloorToInt(location.z) + 1;
            int ceilZ = Mathf.CeilToInt(location.z) + 1;
            if (floorX == ceilX)
            {
                //ceilX++;
            }
            if (floorZ == ceilZ)
            {
                //ceilZ++;
            }

            Queue<VectorNode> openSet = new Queue<VectorNode>();
            HashSet<VectorNode> closedSet = new HashSet<VectorNode>();
            VectorNode targetNode = gridDict[location].GetNodeFromWorldPoint(new Vector3(floorX, location.y, floorZ));
            //Debug.Log(targetNode);
            targetNode.gCost = 0;
            openSet.Enqueue(targetNode);
            targetNode = gridDict[location].GetNodeFromWorldPoint(new Vector3(floorX, location.y, ceilZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            targetNode = gridDict[location].GetNodeFromWorldPoint(new Vector3(ceilX, location.y, floorZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            targetNode = gridDict[location].GetNodeFromWorldPoint(new Vector3(ceilX, location.y, ceilZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            foreach (GridRegion region in gridDict[location].listRegionsAlongPath(path))
            {
                region.oriented = true;
            }
            yield return null;
            while (openSet.Count > 0)
            {
                VectorNode currentNode = openSet.Dequeue();
                closedSet.Add(currentNode);

                foreach(VectorNode node in gridDict[location].GetNeighborNodes(currentNode))
                {
                    if(!node.walkable || !(regions.Contains(node.region)||node.region.oriented))
                    {
                        currentNode.OnEdge = true;
                        currentNode.gCost += distanceFactor;
                        break;
                    }
                }

                foreach (VectorNode node in gridDict[location].GetAdjacentNodes(currentNode))
                {
                    //Debug.Log(node);
                    if (node.walkable && (regions.Contains(node.region) || node.region.oriented))
                    {
                        int cost = currentNode.gCost + distanceFactor;
                        if (!closedSet.Contains(node) || node.gCost > cost)
                        {
                            node.parent = currentNode;
                            node.gCost = cost + node.movementPenalty;
                            //Debug.Log(node.gCost);
                            if (!openSet.Contains(node) && !closedSet.Contains(node))
                            {
                                openSet.Enqueue(node);
                            }
                        }
                    }
                }
            }
            yield return null;
            OrientGrid(gridDict[location], closedSet);
            yield return null;
            //Debug.Log(gridDict[location]);
            Debug.Log(gridDict[location].assignedUnits.Count + " Unit" + ((gridDict[location].assignedUnits.Count==1)?"":"s") + " assigned to this grid.");

            foreach (Unit unit in gridDict[location].assignedUnits)
            {
                unit.InitiateMovement();
            }

        }

        private static void OrientGrid(VectorGrid grid, HashSet<VectorNode> closedSet)
        {
            //TODO: Region-based processing
            for (int x = 0; x < grid.gridSizeX; x++)
            {
                for (int y = 0; y < grid.gridSizeY; y++)
                {
                    if (closedSet.Contains(grid.GetVectorNodeFromGrid(x, y)))
                    {
                        int up, down, left, right;
                        int upY, downY, leftX, rightX;
                        leftX = (x >= 1 && grid.GetVectorNodeFromGrid(x - 1, y).region.oriented && grid.GetVectorNodeFromGrid(x - 1, y).walkable) ? x - 1 : x;
                        rightX = (x < grid.gridSizeX - 1 && grid.GetVectorNodeFromGrid(x + 1, y).region.oriented && grid.GetVectorNodeFromGrid(x + 1, y).walkable) ? x + 1 : x;
                        upY = (y >= 1 && grid.GetVectorNodeFromGrid(x, y - 1).region.oriented && grid.GetVectorNodeFromGrid(x, y - 1).walkable) ? y - 1 : y;
                        downY = (y < grid.gridSizeY - 1 && grid.GetVectorNodeFromGrid(x, y + 1).region.oriented && grid.GetVectorNodeFromGrid(x, y + 1).walkable) ? y + 1 : y;

                        up = grid.GetVectorNodeFromGrid(x, upY).gCost;
                        down = grid.GetVectorNodeFromGrid(x, downY).gCost;
                        left = grid.GetVectorNodeFromGrid(leftX, y).gCost;
                        right = grid.GetVectorNodeFromGrid(rightX, y).gCost;
                        float factor = 1f / Mathf.Sqrt(4 * (Mathf.Pow(left - right, 2) + Mathf.Pow(down - up, 2)));
                        Vector3 direction = new Vector3(left - right, 0f, up - down) * factor;
                        grid.GetVectorNodeFromGrid(x, y).flowDirection = direction;

                    }
                }
            }
        }

        public static int GetDist(VectorNode nodeA, VectorNode nodeB)
        //Returns the general distance between two nodes.
        {
            int distX = (int)Mathf.Abs(nodeA.GridPosition.x - nodeB.GridPosition.x);
            int distY = (int)Mathf.Abs(nodeA.GridPosition.y - nodeB.GridPosition.y);
            if (distX > distY)
            {
                return (int)(distanceFactor * rootTwo * distY + 10 * (distX - distY));
            }
            return (int)(distanceFactor * rootTwo * distX + 10 * (distY - distX));
        }

        void OnDrawGizmos()
        {
            if (displayGridGizmos)
            {
                gridDict[lastTarget].DrawGizmos();
            }

        }
    } 
    
}
