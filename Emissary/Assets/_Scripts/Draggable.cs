using UnityEngine;
using System.Collections;

namespace Emissary
{
    public class Draggable : MonoBehaviour
    {

        // Update is called once per frame

        public void Drag(Vector3 pt)
        {
            transform.position = new Vector3(pt.x, 0.5f, pt.z);
        }
    } 
}
