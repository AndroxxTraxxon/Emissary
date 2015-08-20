using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Emissary
{
    public class Unit : Selectable
    {

        //public Transform target;
        public enum VehicleType { AIR, WATER, GROUND, BUILDING, ENGY };
        public enum UnitState { Stopped, RequestingPath, Moving, Building, Attacking };
        public string UnitID;
        //public PlayerManager PM;
        public float speed = 5;
        public UnitState currentState = UnitState.Stopped;

        protected Vector3[] path;
        protected Queue<Vector3> pathQueue = new Queue<Vector3>();

        protected int targetIndex;
        //protected bool selected;
        public const float maxHealth = 100f;
        public float health;
        public VehicleType type = VehicleType.GROUND;
        protected bool cancelCurrentPath = false;
        protected uint CurrentRequestID;
        protected VectorGrid currentGrid;
        public Transform target;

        void Start()
        {
            InputManager.instance.AddSelectableUnit(this);
            //Debug.Log("ADDING THIS TO GRID!");
            PathRequestManager.instance.AddUnitToGrid(this, target.position);
        }

        void Update()
        {

        }

        internal void OnPathFound(Vector3[] newPath, bool pathSuccessful)
        {

        }

        internal override void Deselect()
        {
            if (selected)
            {
                selected = false;
                gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            }
        }
        internal override void Select()
        {
            if (!selected)
            {
                selected = true;
                gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
            }
        }

        public void AssignToGrid(VectorGrid grid)
        {
            if (currentGrid != null)
            {
                currentGrid.assignedUnits.Remove(this);
                StopCoroutine(FollowGrid());
            }
            currentGrid = grid;
            currentGrid.assignedUnits.Add(this);
        }
        public void InitiateMovement()
        {
            StopCoroutine(FollowGrid());
            StartCoroutine(FollowGrid());
        }

        public IEnumerator FollowGrid()
        {
            VectorNode node;

            //int targetCost = currentGrid.GetNodeFromWorldPoint(currentGrid.target).movementPenalty;
            currentGrid.TryGetNearestWalkableNode(transform.position, out node);
            while (currentGrid != null && node.gCost == 0)
            {
                transform.Translate(node.flowDirection * speed * Time.deltaTime);
                yield return null;
                currentGrid.TryGetNearestOrientedWalkableNode(transform.position, out node);
            }

            currentGrid.assignedUnits.Remove(this);
            currentGrid = null;
            Debug.Log("Target Acquired!");
            yield return null;
        }

        public void TryProcessNextPath()
        {

        }

        public void InterruptPath()
        {
            
        }



        public void RequestPathFromManager(Vector3 target)
        {

        }

        public void OnDrawGizmos()
        {
            if (path != null)
            {
                for (int i = targetIndex; i < path.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i], Vector3.one / 2);

                    if (i == targetIndex)
                    {
                        Gizmos.DrawLine(transform.position, path[i]);
                    }
                    else
                    {
                        Gizmos.DrawLine(path[i - 1], path[i]);
                    }
                }
            }
        }

        public void EnqueuePathLocation(Vector3 target)
        {
            PathRequestManager.instance.AddUnitToGrid(this, target);
        }

        public void FactoryAddPathLocation(Vector3 target)
        {
            
        }


    }

}