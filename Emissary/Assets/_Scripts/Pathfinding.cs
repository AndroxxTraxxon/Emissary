using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour {

    PathRequestManager requestManager;
    Grid grid;
    double rootTwo = Math.Sqrt(2);
    void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }



    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.getNodeFromWorldPoint(startPos);
        Node targetNode = grid.getNodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {

            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);
            //Debug.Log("openSet Count: " + openSet.Count);
            while (openSet.Count > 0)
            {
                //Debug.Log("Entered main Loop!");
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    //sw.Stop();
                    pathSuccess = true;
                    //UnityEngine.Debug.Log("Path found: " + sw.ElapsedMilliseconds + "ms.");
                    break;
                }

                foreach (Node neighbor in grid.getNeighborNodes(currentNode))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
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
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
        
    }

    public Vector3[] RetracePath(Node startNode, Node endNode)
    {
        //Debug.Log("Starting Retrace!");
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
        //Debug.Log("Finished Retrace!");
    }

    public Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();

        waypoints.Add(path[0].worldPosition);
        /*foreach (Node n in path)
        {
            waypoints.Add(n.worldPosition);
        }/*/
            Vector2 directionOld = Vector2.zero;

            for(int i = 1; i < path.Count; i++){
                Vector2 directionNew = new Vector2(path[i-1].gridX-path[i].gridX, path[i-1].gridY-path[i].gridY);
                if(directionNew!= directionOld){
                    waypoints.Add(path[i].worldPosition);
                    directionOld = directionNew;
                }
            }/**/
            return waypoints.ToArray();
    }

    public int GetDist(Node nodeA, Node nodeB)
        //Returns the general distance between two nodes.
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
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
