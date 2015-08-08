﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Emissary
{
    public class PathRequestManager : MonoBehaviour
    {

        FilteredQueue<PathRequest> pathRequestQueue;
        PathRequest currentPathRequest;

        public static PathRequestManager instance;
        AStar pathfinding;
		VectorField field;
        
        
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
            pathRequestQueue = new FilteredQueue<PathRequest>();
            instance = this;
            pathfinding = GetComponent<AStar>();
            field = GetComponent<VectorField>();
            //Debug.Log("PRM Instantiated!");
        }

        public void AddUnitToGrid(Unit unit, Vector3 target)
        {
            target = field.defaultGrid.GetNodeFromWorldPoint(target).worldPosition;
            if(field.gridDict.ContainsKey(target) && field.gridDict[target].GetNodeFromWorldPoint(unit.transform.position).region.oriented)
            {
                Debug.Log("Grid exists already! Adding unit to existing grid!");
                Debug.Log(field.gridDict[target].target);
                unit.AssignToGrid(field.gridDict[target]);

            }
            else
            {
                if (field.gridDict.ContainsKey(target))
                {
                    Debug.Log("Regen Grid");
                    uint ID;
                    //if this is needed elsewhere, then feed ID to wherever the path ID is needed.
                    unit.AssignToGrid(field.gridDict[target]);
                    //field.gridDict[target].assignedUnits.Add(unit);
                    RequestPath(unit.transform.position, target, GenerateGrid, out ID);
                    
                }
                else
                {
                    Debug.Log("NEW GRID INCOMING");
                    field.GenerateDictionaryDefinition(target);
                    //Debug.Log(field.gridDict[target]);
                    uint ID;
                    //field.gridDict[target].assignedUnits.Add(unit);
                    
                    unit.AssignToGrid(field.gridDict[target]);
                    Debug.Log(field.gridDict[target].assignedUnits.Count);
                    RequestPath(unit.transform.position, target, GenerateGrid, out ID);
                    
                }
            }

        }

        public void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, out uint ID)
        {
                PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, out ID);
                instance.pathRequestQueue.Enqueue(newRequest);
                instance.TryProcessNext(); 
        }

        public void RemoveRequestFromQueue(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, uint ID)
        {
            instance.pathRequestQueue.Remove(new PathRequest(pathStart, pathEnd, callback, ID));
        }

        void TryProcessNext()
        {
			if (currentState == ProcessingState.Stopped && pathRequestQueue.Count > 0)
            {
                currentState = ProcessingState.ProcessingPath;
                currentPathRequest = pathRequestQueue.Dequeue();
                pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
                //Debug.Log("Current Path ID: " + CurrentPathID);
            }
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
            //typically what will be called after the path has been found (or not found)
            //
        {
            currentState = ProcessingState.ProcessingGrid;
            currentPathRequest.callback(path, success);
            currentState = ProcessingState.Stopped;
            TryProcessNext();
        }

        public void GenerateGrid(Vector3[] path, bool success)
        {
            if (success)
            {
                Debug.Log("Generating Grid");
                currentState = ProcessingState.ProcessingGrid;
                //hypothetically, the grid will have already been generated by the time that the code reaches this point. re-initializing will overwrite crucial unit data.
                //field.gridDict.Add(path[path.Length - 1], VectorField.instance.defaultGrid);
                StartCoroutine(field.UpdateValues(path));
                Debug.Log("Target: " + path[path.Length-1]);
            }
            currentState = ProcessingState.Stopped;
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