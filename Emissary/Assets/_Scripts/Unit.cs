using UnityEngine;
using System.Collections;
using System;

public class Unit : MonoBehaviour
{

    //public Transform target;
    float speed = 5;
    Vector3[] path;
    int targetIndex;
    public bool selected;
    public float maxHealth = 100f;
    public float health;

    void Start()
    {
        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    void Update()
    {

    }

    public void RequestPathFromManager(Vector3 target)
    {
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
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
        selected = false;
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
    internal void Select()
    {
        selected = true;
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
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
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;

        }
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

}
