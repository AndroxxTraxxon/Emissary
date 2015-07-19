using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Unit : MonoBehaviour
{

    //public Transform target;
    public enum VehicleType { AIR, WATER, GROUND, BUILDING, Constructor};
    public string UnitID;

    float speed = 5;
    Vector3[] path;
    Queue<Vector3> pathQueue = new Queue<Vector3>();
    bool moving = false;
    int targetIndex;
    private bool selected;
    public const float maxHealth = 100f;
    public float health;
    public VehicleType type = VehicleType.AIR;

    void Start()
    {
        InputManager.instance.AddSelectableUnit(this);
    }

    void Update()
    {

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
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
                    moving = false;
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
        if(pathQueue.Count > 0 && !moving)
        {
            RequestPathFromManager(pathQueue.Dequeue());
            moving = true;
        }
    }

    public void RequestPathFromManager(Vector3 target)
    {
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
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
