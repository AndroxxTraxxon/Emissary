using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PathRequestManager : MonoBehaviour {

    FilteredQueue<PathRequest> pathRequestQueue = new FilteredQueue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, out float ID)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, out ID);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }
    
    public static void RemoveRequestFromQueue(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, float ID)
    {
        instance.pathRequestQueue.Remove(new PathRequest(pathStart, pathEnd, callback, ID));
    }
    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;
        float RequestID;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, float ID)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            RequestID = ID;
        }

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, out float ID)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            RequestID = Time.time;
            ID = RequestID;
        }
        public override string ToString()
        {
            return " " + pathStart.ToString() + " " + pathEnd.ToString();
        }
    }
}
