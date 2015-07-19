using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour {

    public int playerID = 0;
    public static InputManager instance;
    public Texture selectionTexture;

    private List<Unit> selectedUnits;
    private List<Unit> selectableUnits;

    private RaycastHit hit;
    private bool mouseDownLastFrame = false;
    private bool boxSelection = false;
    private Rect selectionBox = new Rect();
    private Vector3 initMousePosition;

    private float lastClickTime = 0;
    private const float doubleClickTime = 0.25f;
    private float selectionTransparency = 0.35f;

    void Awake()
    {
        instance = this;
        selectedUnits = new List<Unit>();
        selectableUnits = new List<Unit>();
    }


    void Update(){

        #region Mouse Clicked
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        //note: hit gets updated every frame here. no need to do it again! 
        //This also means that I can reference it wherever I want at any time.

        //LeftMouseButtonInput
        if (Input.GetMouseButtonDown(0))
        {
            
            //If the shift button isn't pressed, then not multiple select.
            if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                ClearUnitList();
            }

            if (Time.time - lastClickTime < doubleClickTime)
            {
                //do double click things
                if (hit.collider.gameObject.GetComponent<Unit>() != null)
                {
                    Unit currentUnit = hit.collider.gameObject.GetComponent<Unit>();
                    foreach (Unit unit in selectableUnits)
                    {
                        if (unit.GetComponent<Renderer>().isVisible && unit.UnitID.Equals(currentUnit.UnitID))
                        {
                            AddUnitToSelection(unit);
                        }
                    } 
                }
            }
            else
            {
                if (hit.collider.gameObject.GetComponent<Unit>()!= null)
                {
                    AddUnitToSelection(hit.collider.gameObject.GetComponent<Unit>()); 
                }
                lastClickTime = Time.time;
            }

        //end left mouse button input
            mouseDownLastFrame = true;
            initMousePosition = Input.mousePosition;
        //Right Mouse Button Input

        }
        else if (Input.GetMouseButtonDown(1))
        {
            foreach (Unit go in selectedUnits)
            {
                go.EnqueuePathLocation(hit.point);
            }
        }

        //Basically, if you're holding the left mouse down, and you're dragging the mouse, begin a box selection
        if (Input.GetMouseButton(0) && mouseDownLastFrame)
        {
            if ((Input.mousePosition - initMousePosition).magnitude > 25)
            {
                showSelectionBox(Input.mousePosition, initMousePosition);
                boxSelection = true;
            }else
            {
                boxSelection = false;
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
            boxSelection = false;
        } 

        #endregion

        //had to use this statement in the case that the mouse moves outside the window. Otherwise, the mouse can exit the window, release, re-enter, and the GetMouseButtonUp() is never called.
        if (boxSelection && !Input.GetMouseButton(0))
        {
            boxSelection = false;
        }


        #region Keyboard Input
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
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    ClearUnitList();
                }
                foreach (Unit go in selectableUnits)
                {
                    if (go.type == Unit.VehicleType.AIR)
                        AddUnitToSelection(go);
                }
            }

            if (Input.GetKey(KeyCode.G))
            {
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    ClearUnitList();
                }
                foreach (Unit go in selectableUnits)
                {
                    if (go.type == Unit.VehicleType.GROUND)
                        AddUnitToSelection(go);
                }
            }

            if (Input.GetKey(KeyCode.W))
            {
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    ClearUnitList();
                }
                foreach (Unit go in selectableUnits)
                {
                    if (go.type == Unit.VehicleType.WATER)
                        AddUnitToSelection(go);
                }
            }

        }

        //
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearUnitList();
        }  
        #endregion
    }

    private void showSelectionBox(Vector3 currentPos, Vector3 originalPos)
    {
        
        selectionBox = new Rect((Mathf.Min(originalPos.x, currentPos.x)), Screen.height-Mathf.Max(originalPos.y, currentPos.y),
                              Mathf.Abs(originalPos.x - currentPos.x), Mathf.Abs(originalPos.y - currentPos.y));
        boxSelection = true;
    }

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
            screenCoordinates.y = Screen.height - screenCoordinates.y;
            //Find all the objects inside the box
            if (selectionBox.Contains(screenCoordinates))
            {
                if (!selectedUnits.Contains(go))
                {
                    AddUnitToSelection(go);
                    go.Select();
                }
            }
        }
    }

    private void AddUnitToSelection(Unit unit){
        if(selectableUnits.Contains(unit) && !selectedUnits.Contains(unit)){
            selectedUnits.Add(unit);
            unit.GetComponent<Unit>().Select();
        }
    }
    private void RemoveLastSelectedUnit()
    {
        selectedUnits.RemoveAt(selectedUnits.Count - 1);
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
            GUI.color = Color.gray;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, selectionTransparency);
            GUI.DrawTexture(selectionBox, selectionTexture);
            GUI.color = Color.white;
        }
    }

    


}
