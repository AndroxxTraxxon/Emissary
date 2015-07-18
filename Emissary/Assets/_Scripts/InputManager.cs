using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour {

    public int playerID = 0;
    public Camera cam;
    private List<Unit> selectedUnits;
    private RaycastHit hit;
    private bool mouseDownLastFrame = false;
    private bool boxSelection = false;
    private Rect selectionBox = new Rect();
    private Vector3 initMousePosition;
    private List<Unit> selectableUnits;

    public static InputManager instance;

    void Awake()
    {
        instance = this;
        selectedUnits = new List<Unit>();
        selectableUnits = new List<Unit>();
    }
    void Update(){

        #region Mouse Clicked
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
        //note: hit gets updated every frame here. no need to do it again! 
        //This also means that I can reference it wherever I want at any time.
        {
            //LeftMouseButtonInput
            if (Input.GetMouseButtonDown(0))
            {
                //If the shift button isn't pressed, then not multiple select.
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    ClearUnitList();
                }

                mouseDownLastFrame = true;
                initMousePosition = Input.mousePosition;

            }
            //Right Mouse Button Input
            else if (Input.GetMouseButtonDown(1))
            {
                foreach (Unit go in selectedUnits)
                {
                    go.RequestPathFromManager(hit.point);
                }
            }
        }
        //Basically, if you're holding the left mouse down, and you're dragging the mouse, begin a box selection
        if (Input.GetMouseButton(0) && mouseDownLastFrame)
        {
            if ((Input.mousePosition - initMousePosition).magnitude > .5f)
            {
                showSelectionBox(Input.mousePosition, initMousePosition);
                boxSelection = true;
            }
        } 
        #endregion

        #region Mouse Released
        /*
         * This contains code for when the mouse is released. Actions like unit selection, click-and-drag selection, etc.
         * 
         * */

        if (Input.GetMouseButtonUp(0))
        {
            if (boxSelection)
            {
                //if the multiple selection box has been activated, do a box selection.
                SelectMultipleObjects(Input.mousePosition, initMousePosition);
            }
            else
            {
                //Only adding the GameObject to the units list if it's actually a Unit.
                if (hit.collider.gameObject.GetComponent<Unit>() != null)
                {
                    selectedUnits.Add(hit.collider.gameObject.GetComponent<Unit>());
                    hit.collider.gameObject.GetComponent<Unit>().Select();
                }
            }
            boxSelection = false;
        } 
        #endregion

        //had to use this statement in the case that the mouse moves outside the window. Otherwise, the mouse can exit the window, release, re-enter, and the GetMouseButtonUp() is never called.
        if (boxSelection && !Input.GetMouseButton(0))
        {
            boxSelection = false;
        }


        //Keyboard-Explicit controls begin here.
        //Shift-Key Combo Functions
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {

        }
        //Control-Key combo Functions
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKey(KeyCode.E))
            {
                ClearUnitList();
                foreach (Unit go in selectableUnits)
                {
                    AddUnitToSelection(go);
                }
            }

            if (Input.GetKey(KeyCode.A))
            {
                foreach (Unit go in selectableUnits)
                {
                    if (go.type == Unit.VehicleType.AIR)
                        AddUnitToSelection(go);
                }
            }

            if (Input.GetKey(KeyCode.G))
            {
                foreach (Unit go in selectableUnits)
                {
                    if (go.type == Unit.VehicleType.GROUND)
                        AddUnitToSelection(go);
                }
            }

            if (Input.GetKey(KeyCode.W))
            {
                foreach (Unit go in selectableUnits)
                {
                    if (go.type == Unit.VehicleType.WATER)
                        AddUnitToSelection(go);
                }
            }

        }

        //
        if (Input.GetKeyDown(KeyCode.Escape)){
            ClearUnitList();
        } 
    }

    private void showSelectionBox(Vector3 currentPos, Vector3 originalPos)
    {
        boxSelection = true;
        selectionBox = new Rect((Mathf.Min(originalPos.x, currentPos.x)), Screen.height-Mathf.Min(originalPos.y, currentPos.y),
                              Mathf.Abs(originalPos.x - currentPos.x), -Mathf.Abs(originalPos.y - currentPos.y));
    }

    
    //this is just for debug!
    void OnDrawGizmos()
    {
        Gizmos.DrawRay(hit.point, Vector3.up);
        Gizmos.DrawSphere(hit.point, 0.25f);
    }

    private void SelectMultipleObjects(Vector3 currentPos, Vector3 originalPos)
    {
        foreach (Unit go in selectableUnits) //represents all the movable units
        {
            Vector3 screenCoordinates = Camera.main.WorldToScreenPoint(go.transform.position);//convert the current object position to screen coordinates

            //Find all the objects inside the box
            if ((screenCoordinates.x < originalPos.x && screenCoordinates.x > currentPos.x) && (screenCoordinates.y > originalPos.y && screenCoordinates.y < currentPos.y))
            {
                if (!selectedUnits.Contains(go))
                {
                    AddUnitToSelection(go);
                }
            }
        }
    }

    private void AddUnitToSelection(Unit unit){
        if(selectableUnits.Contains(unit)){
            selectedUnits.Add(unit);
            unit.GetComponent<Unit>().Select();
        }
    }

    private void ClearUnitList()
        {
            foreach (Unit go in selectedUnits)
            {
                go.GetComponent<Unit>().Deselect();
            }
            selectedUnits.Clear();
        }
    public void AddSelectableUnit(Unit unit)
    {
        if (unit.GetComponent<Unit>() != null)
        {
            selectableUnits.Add(unit);
        }
    }

    void OnGUI()
    {
        if (boxSelection)
        {
            GUI.Box(selectionBox, "Unit Selection");
        }
    }
}
