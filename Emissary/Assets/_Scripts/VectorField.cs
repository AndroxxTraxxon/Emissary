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
     * 
     * 
     * within selected group of units
     * find average selection location
     * A* from average to target
     * get sectors that A* path goes through
     * generate vectorFields for path{
     * Node currentNode
     *      make path list of VectorFields{
     *          start at target, moving out
     *          make 4 start nodes, with cost values 0, 0,0,0;
     *          add them to open set
     *          while open set is not empty{
     *              currentNode = openSet.Dequeue(0);
     *              for each node in neighbor nodes{
     *                  calc f cost from this node{
     *                      int cost = CurrentNode.fCost + grid distance to neighbor;
     *                      if(!closedSet.Contains(neighborNode) || neighborNode.fCost > cost){
     *                          neighborNode.parentNode = currentNode;
     *                          neighborNode.gCost = cost;
     *                          if(!openSetSet.Contains(neighborNode)){
     *                              openSet.Add(neighborNode);
     *                          }
     *                      }
     *                  }
     *              }
     *              closedSet.Add(currentNode);
     *              
     *          }
     *          foreach(VectorNode node in Section){
     *              if(!closedSet)
     *          }
     *      }
     * }
     * send units along vector field.
*/

namespace Emissary
{
    public class VectorField : MonoBehaviour 
    {

        public VectorGrid grid;
        double rootTwo = 1.5; // Math.Sqrt(2);


        // Use this for initialization
	    void Start () {
            OrientGrid(new Vector3(0, 0, 0));
	    }
	
	    // Update is called once per frame
	    void Update () {
	        
	    }

        public void OrientGrid(Vector3 location)
        {
            int floorX = Mathf.FloorToInt(location.x)+1;
            int ceilX = Mathf.CeilToInt(location.x)+1;
            int floorZ = Mathf.FloorToInt(location.z)+1;
            int ceilZ = Mathf.CeilToInt(location.z)+1;
            if (floorX == ceilX)
            {
                ceilX++;
            }
            if (floorZ == ceilZ)
            {
                ceilZ++;
            }

            Queue<VectorNode> openSet = new Queue<VectorNode>();
            HashSet<VectorNode> closedSet = new HashSet<VectorNode>();
            VectorNode targetNode = grid.getVectorNodeFromWorldPoint(new Vector3(floorX, location.y, floorZ));
            targetNode.gCost = 0;
            openSet.Enqueue(targetNode);
            targetNode = grid.getVectorNodeFromWorldPoint(new Vector3(floorX, location.y, ceilZ));
            if(openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            targetNode = grid.getVectorNodeFromWorldPoint(new Vector3(ceilX, location.y, floorZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            targetNode = grid.getVectorNodeFromWorldPoint(new Vector3(ceilX, location.y, ceilZ));
            if (openSet.Contains(targetNode))
                openSet.Enqueue(targetNode);
            while (openSet.Count > 0)
            {
                VectorNode currentNode = openSet.Dequeue();
                closedSet.Add(currentNode);
                foreach (VectorNode node in grid.getNeighborVectorNodes(currentNode))
                {
                    if (node.walkable)
	                    {
		                    int cost = currentNode.gCost + GetDist(currentNode, node);
                            if(!closedSet.Contains(node) || node.gCost > cost){
                                node.parent = currentNode;
                                node.gCost = cost;
                                if(!openSet.Contains(node)){
                                    openSet.Enqueue(node);
                                }
                            } 
	                    }
                }
            }
            for(int x = 0; x < grid.gridSizeX; x++){
                for(int y = 0; y < grid.gridSizeY; y++){
                    if(closedSet.Contains(grid.grid[x,y])){
                        int up, down, left, right;
                        int upY, downY, leftX, rightX;
                        leftX = (x >= 1 && grid.grid[x-1, y].walkable)? x-1: x;
                        rightX = (x < grid.gridSizeX-1 && grid.grid[x+1, y].walkable)? x+1: x;
                        upY = (y >= 1 && grid.grid[x, y-1].walkable)? y-1: y;
                        downY = (y < grid.gridSizeY-1 && grid.grid[x, y+1].walkable)? y+1: y;

                        up = grid.grid[x, upY].gCost;
                        down = grid.grid[x, downY].gCost;
                        left = grid.grid[leftX, y].gCost;
                        right = grid.grid[rightX, y].gCost;
                        float factor = 1f/Mathf.Sqrt(4 * (Mathf.Pow(left - right, 2) + Mathf.Pow(down - up, 2)));
                        Vector3 direction = new Vector3(left-right, 0f, up-down) * factor;
                        grid.grid[x,y].flowDirection = direction;
                        
                    }
                }
            }

        }

        public int GetDist(VectorNode nodeA, VectorNode nodeB)
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
    } 
}
