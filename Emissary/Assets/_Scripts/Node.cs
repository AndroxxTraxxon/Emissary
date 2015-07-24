using UnityEngine;
using System.Collections;
using System;

namespace Emissary
{
    public class Node : IHeapItem<Node>
    {

        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        public Node parent;
        public int movementPenalty;


        public int gCost;
        public int hCost;
        int heapIndex;

        public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _movementPenalty)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
            movementPenalty = _movementPenalty;
        }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }

            set
            {
                heapIndex = value;
            }
        }

        public override string ToString()
        {
            return "NODE{WK: " + walkable + ", WP: " + worldPosition + ", X: " + gridX + "Y: " + gridY + "}";
        }

        public int CompareTo(Node other)
        {
            int compare = fCost.CompareTo(other.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(other.hCost);
            }

            return -compare;
        }
    }
    
}