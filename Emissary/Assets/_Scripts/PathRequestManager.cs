using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Emissary
{
    public class PathRequestManager : MonoBehaviour
    {

        FilteredQueue<PathRequest> pathRequestQueue = new FilteredQueue<PathRequest>();
        PathRequest currentPathRequest;

        public static PathRequestManager instance;
        AStar pathfinding;
		VectorField field;
        //List<VectorGrid>

		public enum ProcessingState {ProcessingPath, ProcessingGrid, Stopped};

		ProcessingState currentState = ProcessingState.Stopped;

        public float CurrentPathID
        {
            get
            {
                return currentPathRequest.RequestID;
            }
        }
        void Awake()
        {
            instance = this;
            pathfinding = GetComponent<AStar>();
        }

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, out uint ID)
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, out ID);
            instance.pathRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();
        }

        public static void RemoveRequestFromQueue(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, uint ID)
        {
            instance.pathRequestQueue.Remove(new PathRequest(pathStart, pathEnd, callback, ID));
        }
        void TryProcessNext()
        {
			if (currentState == ProcessingState.Stopped && pathRequestQueue.Count > 0)
            {
                currentPathRequest = pathRequestQueue.Dequeue();
                currentState = ProcessingState.ProcessingPath;
                pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
                //Debug.Log("Current Path ID: " + CurrentPathID);
            }
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            currentPathRequest.callback(path, success);
            currentState = ProcessingState.ProcessingGrid;
            TryProcessNext();
        }

        struct PathRequest
        {
            public Vector3 pathStart;
            public Vector3 pathEnd;
            public Action<Vector3[], bool> callback;
            public uint RequestID;
            static uint currentID = 0;

            public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, uint ID)
            {
                pathStart = _start;
                pathEnd = _end;
                callback = _callback;
                RequestID = ID;
            }

            public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, out uint ID)
            {
                pathStart = _start;
                pathEnd = _end;
                callback = _callback;
                RequestID = currentID++;
                ID = RequestID;
            }
            public override string ToString()
            {
                return " " + pathStart.ToString() + " " + pathEnd.ToString();
            }
        }
    } 
}
