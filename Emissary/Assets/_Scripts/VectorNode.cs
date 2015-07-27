using UnityEngine;
using System.Collections;
using System;

namespace Emissary
{
    public class VectorNode : IHeapItem<VectorNode>
    {

        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        public VectorNode parent;
        public int movementPenalty;
        public Vector3 flowDirection;


        public int gCost;
        public int hCost;
        int heapIndex;

        public VectorNode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _movementPenalty)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
            movementPenalty = _movementPenalty;
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

        public int CompareTo(VectorNode other)
        {
            int compare = gCost.CompareTo(other.gCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(other.hCost);
            }

            return -compare;
        }
    }

}