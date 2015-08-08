using UnityEngine;
using System;
using System.Collections.Generic;

namespace Emissary
{
    public class GridRegion
    {
		public System.Random rand = new System.Random();
        public bool displayGridGizmos = true;
        // Use this for initialization
        public const int STANDARD_SIZE = 16;
        VectorGrid parentGrid;
        int width, height;
        Vector2 gridCoords;
        VectorNode[,] nodes;// = new VectorNode[width, height];
        Vector3 offset;
        float nodeScale;
        float nodeRadius;
        Vector3 RegionBottomLeft;
        public bool oriented
        {
            get;
            set;
        }
        public List<Unit> assignedUnits;

        public GridRegion(VectorGrid parentGrid)
        {
            gridCoords = Vector2.zero;
            width = STANDARD_SIZE;
            height = STANDARD_SIZE;
            nodes = new VectorNode[width, height];
            nodeScale = 1;
            nodeRadius = 1f / 2f;
            this.offset = new Vector3();
            this.parentGrid = parentGrid;
            RegionBottomLeft = offset - Vector3.right * width * nodeRadius - Vector3.forward * height * nodeRadius;
            InitializeNodes();
        }

        public GridRegion(int x, int y, float nodeScale, Vector3 offset, VectorGrid parentGrid)
        {
            gridCoords = new Vector2(x,y);
            width = STANDARD_SIZE;
            height = STANDARD_SIZE;
            nodes = new VectorNode[width, height];
            this.nodeScale = nodeScale;
            nodeRadius = nodeScale / 2f;
            this.offset = offset;
            this.parentGrid = parentGrid;
            RegionBottomLeft = offset - Vector3.right * width * nodeRadius - Vector3.forward * height * nodeRadius;
            InitializeNodes();
        }

        public GridRegion(int x, int y, int width, int height, float nodeScale, Vector3 offset, VectorGrid parentGrid)
        {
            gridCoords.x = x;
            gridCoords.y = y;
            this.width = width;
            this.height = height;
            nodes = new VectorNode[width, height];
            this.nodeScale = nodeScale;
            nodeRadius = nodeScale / 2f;
            this.offset = offset;
            //Debug.Log(offset);
            this.parentGrid = parentGrid;
            //Debug.Log(nodeRadius);
            RegionBottomLeft = this.offset - Vector3.right * width * nodeRadius - Vector3.forward * height * nodeRadius;
            InitializeNodes();
        }

        private void InitializeNodes()
        {
            //Debug.Log("Initializing Nodes: width " + width + ", height " + height);
            //Debug.Log(RegionBottomLeft);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector3 worldPoint = RegionBottomLeft + Vector3.right * (i * nodeScale + nodeRadius) + Vector3.forward * (j * nodeScale + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeScale / 2, parentGrid.unwalkableMask));
                    int movementPenalty = 0;
                    if (walkable)
                    {
                        Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100, parentGrid.walkableMask))
                        {
                            parentGrid.walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        }
                    }
                    nodes[i, j] = new VectorNode(walkable, worldPoint, i, j, movementPenalty, this);
                    //Debug.Log("New Node: " + nodes[i, j].ToString());
                }
            }
        }

        public VectorNode getNode(int x, int y){
            return nodes[x,y];
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.white;//new Color(GridRegion.rand.Range(0f,1f),GridRegion.rand.Range(0f,1f),GridRegion.rand.Range(0f,1f));
            Gizmos.DrawWireCube(offset, Vector3.right * width * nodeScale + Vector3.forward * height * nodeScale);
            if (nodes != null && oriented)
            {
                foreach (VectorNode n in nodes)
                {
                    //Baseline: Walkable --> white; Not Walkable --> red
                    Gizmos.color = new Color(1 - n.gCost / 255f, 1 - n.gCost / 255f, 1);
                    if (!n.walkable) Gizmos.color = Color.red;
                    else if (n.OnEdge) Gizmos.color = Color.green;
                    //Player Position --> cyan
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeScale * .195f));
                    Gizmos.DrawRay(n.worldPosition, n.flowDirection * nodeScale);
                }

            } 

        }

        internal VectorGrid GetParentGrid()
        {
            return parentGrid;
        }

        public override string ToString()
        {
            return "Region: " + width + ", " + height;
        }

        internal Vector2 GridCoords
        {
            get
            {
                return gridCoords;
            }
        }
    }

}