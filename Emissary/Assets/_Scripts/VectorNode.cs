using UnityEngine;
using System.Collections;
using System;

namespace Emissary
{
    public class VectorNode : IHeapItem<VectorNode>
    {

        public bool walkable;
        public Vector3 worldPosition;
        public Vector2 regionCoords;
        public VectorNode parent;
        public int movementPenalty;
        public Vector3 flowDirection;
        public GridRegion region;

        public int gCost;
        public int hCost;
        int heapIndex;

        public VectorNode(bool _walkable, Vector3 _worldPos, int _regionX, int _regionY, int _movementPenalty, GridRegion region)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            regionCoords = new Vector2(_regionX, _regionY);
            movementPenalty = _movementPenalty;
            this.region = region;
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
            return "NODE{WK: " + walkable + ", WP: " + worldPosition + ", GP: "+GridPosition+"}";
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

        public Vector2 GridPosition
        {
            get
            {
                return region.GridCoords * 16 + regionCoords;
            }
        }

    }

}