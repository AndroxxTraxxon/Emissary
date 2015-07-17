using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour {

    int playerID = 0;
    public Camera cam;
    List<GameObject> selectedUnits;
    RaycastHit hit;


    void Start()
    {
        selectedUnits = new List<GameObject>();
    }
    void Update(){

        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
        {
            //LeftMouseButtonInput
            if (Input.GetMouseButtonDown(0))
            {
                //If the shift button isn't pressed, then not multiple select.
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    ClearUnitList();
                }
                //Only adding the G.O. if it's actually a Unit.
                if (hit.collider.gameObject.GetComponent<Unit>() != null)
                {
                    selectedUnits.Add(hit.collider.gameObject);
                    hit.collider.gameObject.GetComponent<Unit>().Select();
                }

            }

            //Right Mouse Button Input
            else if (Input.GetMouseButtonDown(1))
            {
                foreach (GameObject go in selectedUnits)
                {
                    go.GetComponent<Unit>().RequestPathFromManager(hit.point);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)){
            ClearUnitList();
        } 
    }

    private void ClearUnitList()
    {
        foreach (GameObject go in selectedUnits)
        {
            go.GetComponent<Unit>().Deselect();
        }
        selectedUnits.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(hit.point, Vector3.up);
        Gizmos.DrawSphere(hit.point, 0.25f);
    }
}
