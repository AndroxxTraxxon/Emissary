using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Unit : MonoBehaviour
{

    //public Transform target;
    public enum VehicleType { AIR, WATER, GROUND, BUILDING, ENGY};
    public enum UnitState { Stopped, RequestingPath, Moving, Building, Attacking}
    public string UnitID;

    float speed = 5;
    Vector3[] path;
    Queue<Vector3> pathQueue = new Queue<Vector3>();
    public UnitState currentState = UnitState.Stopped;
    int targetIndex;
    private bool selected;
    public const float maxHealth = 100f;
    public float health;
    public VehicleType type = VehicleType.AIR;
    private bool cancelCurrentPath = false;
    uint CurrentRequestID;
    Vector3 CurrentTarget;

    void Start()
    {
        InputManager.instance.AddSelectableUnit(this);
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

    private void TryProcessNextPath()
    {
        if(pathQueue.Count > 0 && (currentState != UnitState.Moving))
        {
            RequestPathFromManager(pathQueue.Dequeue());
        }
    }

    public void InterruptPath()
    {
        if(currentState == UnitState.RequestingPath)
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
                Gizmos.DrawCube(path[i], Vector3.one/2);

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

}
