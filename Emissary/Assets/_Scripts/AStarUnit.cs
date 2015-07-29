using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Emissary
{
    public class AStarUnit : MonoBehaviour
    {

        //public Transform target;
        public enum VehicleType { AIR, WATER, GROUND, BUILDING, ENGY };
        public enum UnitState { Stopped, RequestingPath, Moving, Building, Attacking };
        public string UnitID;
        public PlayerManager PM;
        public float speed = 5;
        public UnitState currentState = UnitState.Stopped;

        protected Vector3[] path;
        protected Queue<Vector3> pathQueue = new Queue<Vector3>();

        protected int targetIndex;
        protected bool selected;
        public const float maxHealth = 100f;
        public float health;
        public VehicleType type = VehicleType.GROUND;
        protected bool cancelCurrentPath = false;
        protected uint CurrentRequestID;
        protected Vector3 CurrentTarget;

        void Start()
        {
            AStarInputManager.instance.AddSelectableAStarUnit(this);
        }

        void Update()
        {
            
        }

        public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
        {
            if (pathSuccessful && !cancelCurrentPath)
            {
                path = newPath;
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
            else
            {
                currentState = UnitState.Stopped;
            }
            cancelCurrentPath = false;
        }

        internal void Deselect()
        {
            if (selected)
            {
                selected = false;
                gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            }
        }
        internal void Select()
        {
            if (!selected)
            {
                selected = true;
                gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
            }
        }
        IEnumerator FollowPath()
        {
            currentState = UnitState.Moving;
            Vector3 currentWaypoint = path[0];
            targetIndex = 0;

            while (true)
            {
                if (transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        currentState = UnitState.Stopped;
                        TryProcessNextPath();
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                yield return null;

            }
        }

        public void TryProcessNextPath()
        {
            if (pathQueue.Count > 0 && (currentState == UnitState.Stopped) && this.type != VehicleType.BUILDING)
            {
                RequestPathFromManager(pathQueue.Dequeue());
            }
        }

        public void InterruptPath()
        {
            if (this.type != VehicleType.BUILDING)
            {
                if (currentState == UnitState.RequestingPath)
                {
                    if (PathRequestManager.instance.CurrentPathID == CurrentRequestID)
                    {
                        cancelCurrentPath = true;
                    }
                    else
                    {
                        PathRequestManager.RemoveRequestFromQueue(transform.position, CurrentTarget, OnPathFound, CurrentRequestID);
                        cancelCurrentPath = false;
                    }
                }
                pathQueue.Clear();
                StopCoroutine("FollowPath");
                currentState = UnitState.Stopped;
            }
            else
            {
                pathQueue.Clear();
            }
        }

        public void RequestPathFromManager(Vector3 target)
        {
            CurrentTarget = target;
            currentState = UnitState.RequestingPath;
            PathRequestManager.RequestPath(transform.position, target, OnPathFound, out CurrentRequestID);
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
            pathQueue.Enqueue(target);
            TryProcessNextPath();

        }

        public void FactoryAddPathLocation(Vector3 target)
        {
            pathQueue.Enqueue(target);
        }

        
    }
    
}