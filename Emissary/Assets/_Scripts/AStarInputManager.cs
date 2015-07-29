using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Emissary
{
    public class AStarInputManager : MonoBehaviour
    {

        public int playerID = 0;
        public static AStarInputManager instance;
        public Texture selectionTexture;
        Grid grid;

        private List<AStarUnit> selectedAStarUnits;
        private List<AStarUnit> selectableAStarUnits;

        private RaycastHit hit;
        private bool mouseDownLastFrame = false;
        private bool boxSelection = false;
        public static bool shiftPressed;
        private Rect selectionBox = new Rect();
        private Vector3 initMousePosition;

        private float lastClickTime = 0;
        private const float doubleClickTime = 0.25f;
        private float selectionTransparency = 0.35f;


        void Awake()
        {
            instance = this;
            selectedAStarUnits = new List<AStarUnit>();
            selectableAStarUnits = new List<AStarUnit>();
        }


        void Update()
        {

            #region Mouse Clicked
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
            //note: hit gets updated every frame here. no need to do it again! 
            //This also means that I can reference it wherever I want at any time.

            //LeftMouseButtonInput
            shiftPressed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));


            if (Input.GetMouseButton(0) && mouseDownLastFrame)
            {
                if ((Input.mousePosition - initMousePosition).magnitude > 25)
                {
                    showSelectionBox(Input.mousePosition, initMousePosition);
                    boxSelection = true;
                }
                else
                {
                    boxSelection = false;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                initMousePosition = Input.mousePosition;
                //If the shift button isn't pressed, then not multiple select.
                if (!shiftPressed)
                {
                    ClearAStarUnitList();
                }

                if (Time.time - lastClickTime < doubleClickTime)
                {
                    //do double click things
                    if (hit.collider.gameObject.GetComponent<AStarUnit>() != null)
                    {
                        AStarUnit currentAStarUnit = hit.collider.gameObject.GetComponent<AStarUnit>();
                        foreach (AStarUnit AStarUnit in selectableAStarUnits)
                        {
                            if (AStarUnit.GetComponent<Renderer>().isVisible && AStarUnit.UnitID.Equals(currentAStarUnit.UnitID))
                            {
                                AddAStarUnitToSelection(AStarUnit);
                            }
                        }
                    }
                }
                else
                {
                    //if (hit.collider.gameObject.GetComponent<AStarUnit>() != null)
                    {
                        AddAStarUnitToSelection(hit.collider.gameObject.GetComponent<AStarUnit>());
                    }
                    lastClickTime = Time.time;
                }

                //end left mouse button input
                mouseDownLastFrame = true;
                //Right Mouse Button Input

            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (PathRequestManager.instance.gameObject.GetComponent<Grid>().getNodeFromWorldPoint(hit.point).walkable)
                {
                    if (!shiftPressed)
                    {
                        foreach (AStarUnit go in selectedAStarUnits)
                        {

                            go.InterruptPath();
                        }
                    }
                    foreach (AStarUnit go in selectedAStarUnits)
                    {
                        go.EnqueuePathLocation(hit.point);
                    }
                }
            }

            //Basically, if you're holding the left mouse down, and you're dragging the mouse, begin a box selection

            #endregion

            #region Mouse Released
            /*
         * This contains code for when the mouse is released. Actions like AStarUnit selection, click-and-drag selection, etc.
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

                    ClearAStarUnitList();
                    foreach (AStarUnit go in selectableAStarUnits)
                    {
                        AddAStarUnitToSelection(go);
                    }
                }

                if (Input.GetKey(KeyCode.A))
                {
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    {
                        ClearAStarUnitList();
                    }
                    foreach (AStarUnit go in selectableAStarUnits)
                    {
                        if (go.type == AStarUnit.VehicleType.AIR)
                            AddAStarUnitToSelection(go);
                    }
                }

                if (Input.GetKey(KeyCode.G))
                {
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    {
                        ClearAStarUnitList();
                    }
                    foreach (AStarUnit go in selectableAStarUnits)
                    {
                        if (go.type == AStarUnit.VehicleType.GROUND)
                            AddAStarUnitToSelection(go);
                    }
                }

                if (Input.GetKey(KeyCode.W))
                {
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    {
                        ClearAStarUnitList();
                    }
                    foreach (AStarUnit go in selectableAStarUnits)
                    {
                        if (go.type == AStarUnit.VehicleType.WATER)
                            AddAStarUnitToSelection(go);
                    }
                }

            }

            //
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearAStarUnitList();
            }
            #endregion
        }

        private void showSelectionBox(Vector3 currentPos, Vector3 originalPos)
        {

            selectionBox = new Rect((Mathf.Min(originalPos.x, currentPos.x)), Screen.height - Mathf.Max(originalPos.y, currentPos.y),
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
            foreach (AStarUnit go in selectableAStarUnits) //represents all the movable AStarUnits
            {
                Vector3 screenCoordinates = Camera.main.WorldToScreenPoint(go.transform.position);//convert the current object position to screen coordinates
                screenCoordinates.y = Screen.height - screenCoordinates.y;
                //Find all the objects inside the box
                if (selectionBox.Contains(screenCoordinates))
                {
                    if (!selectedAStarUnits.Contains(go))
                    {
                        AddAStarUnitToSelection(go);
                        go.Select();
                    }
                }
            }
        }

        private void AddAStarUnitToSelection(AStarUnit AStarUnit)
        {
            if (selectableAStarUnits.Contains(AStarUnit) && !selectedAStarUnits.Contains(AStarUnit))
            {
                selectedAStarUnits.Add(AStarUnit);
                AStarUnit.GetComponent<AStarUnit>().Select();
            }
        }
        private void RemoveLastSelectedAStarUnit()
        {
            selectedAStarUnits.RemoveAt(selectedAStarUnits.Count - 1);
        }
        private void ClearAStarUnitList()
        {
            foreach (AStarUnit go in selectedAStarUnits)
            {
                go.GetComponent<AStarUnit>().Deselect();
            }
            selectedAStarUnits.Clear();
        }
        public void AddSelectableAStarUnit(AStarUnit AStarUnit)
        {
            if (AStarUnit.GetComponent<AStarUnit>() != null)
            {
                selectableAStarUnits.Add(AStarUnit);
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

}