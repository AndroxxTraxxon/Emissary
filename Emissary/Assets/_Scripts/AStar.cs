using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Emissary
{
    public class AStar : MonoBehaviour
    {

        PathRequestManager requestManager;
        VectorGrid grid;
        double rootTwo = Math.Sqrt(2);
        void Awake()
        {
            requestManager = GetComponent<PathRequestManager>();
            grid = VectorField.instance.defaultGrid;
        }



        IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;

            VectorNode startNode;
            VectorNode targetNode;
            bool startFound = grid.TryGetNearestWalkableNode(startPos, out startNode);
            bool endFound = grid.TryGetNearestWalkableNode(targetPos, out targetNode);

            if (startFound && endFound)
            {

                Heap<VectorNode> openSet = new Heap<VectorNode>(grid.MaxSize);
                HashSet<VectorNode> closedSet = new HashSet<VectorNode>();
                openSet.Add(startNode);
                //Debug.Log("openSet Count: " + openSet.Count);
                while (openSet.Count > 0)
                {
                    //Debug.Log("Entered main Loop!");
                    VectorNode currentNode = openSet.RemoveFirst();
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        //sw.Stop();
                        pathSuccess = true;
                        //UnityEngine.Debug.Log("Path found: " + sw.ElapsedMilliseconds + "ms.");
                        break;
                    }

                    foreach (VectorNode neighbor in grid.GetNeighborNodes(currentNode))
                    {
                        if (!neighbor.walkable)
                        {
                            continue;
                        }

                        int newMovementCostToNeighbor = currentNode.gCost + GetDist(currentNode, neighbor) + neighbor.movementPenalty;
                        if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                        {
                            neighbor.gCost = newMovementCostToNeighbor;
                            neighbor.hCost = GetDist(neighbor, targetNode);
                            neighbor.parent = currentNode;

                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);

                            }
                            else openSet.UpdateItem(neighbor);
                        }

                    }
                }
            }

            yield return null;
            if (pathSuccess && startFound && endFound)
            {
                waypoints = RetracePath(startNode, targetNode);
            }
            if (waypoints.Length <= 0)
            {
                pathSuccess = false;
            }
            requestManager.FinishedProcessingPath(targetPos, waypoints, pathSuccess);

        }

        public Vector3[] RetracePath(VectorNode startNode, VectorNode endNode)
        {
            //Debug.Log("Starting Retrace!");
            List<VectorNode> path = new List<VectorNode>();
            VectorNode currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            if (path.Count <= 0)
            {
                return new Vector3[0];
            }
            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
            //Debug.Log("Finished Retrace!");
        }

        public Vector3[] SimplifyPath(List<VectorNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();

            waypoints.Add(path[0].worldPosition);
            foreach (VectorNode n in path)
            {
                waypoints.Add(n.worldPosition);
            }

            //this section that is commented out is 
            /*
            Vector2 directionOld = new Vector2(path[1].GridPosition.x - path[0].GridPosition.x, path[1].GridPosition.y - path[0].GridPosition.y);

            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2 directionNew = new Vector2(path[i + 1].GridPosition.x - path[i].GridPosition.x, path[i + 1].GridPosition.y - path[i].GridPosition.y);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i].worldPosition);
                    directionOld = directionNew;
                }
            }/**/
            return waypoints.ToArray();
        }

        public int GetDist(VectorNode nodeA, VectorNode nodeB)
        //Returns the general distance between two nodes.
        {
            int distX = Mathf.RoundToInt(Mathf.Abs(nodeA.GridPosition.x - nodeB.GridPosition.x));
            int distY = Mathf.RoundToInt(Mathf.Abs(nodeA.GridPosition.y - nodeB.GridPosition.y));
            if (distX > distY)
            {
                return (int)(10 * rootTwo * distY + 10 * (distX - distY));
            }
            return (int)(10 * rootTwo * distX + 10 * (distY - distX));
        }


        public void StartFindPath(Vector3 startPos, Vector3 targetPos)
        {
            StartCoroutine(FindPath(startPos, targetPos));
        }
    }
    
}