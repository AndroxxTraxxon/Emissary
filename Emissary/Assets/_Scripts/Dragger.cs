using UnityEngine;
using System.Collections;

namespace Emissary
{
    public class Dragger : MonoBehaviour
    {

        RaycastHit hit;
        public Transform player, target;
        Vector3[] path;
        bool successful;
        uint j;
        // Use this for initialization
        void Start()
        {
            j = 0;
        }

        // Update is called once per frame
        void Update()
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (Input.GetMouseButton(0))
                {
                    if (hit.collider.gameObject.GetComponent<Draggable>() != null)
                    {
                        hit.collider.gameObject.GetComponent<Draggable>().Drag(hit.point);
                    }
                }
            }

            PathRequestManager.RequestPath(player.position, target.position, this.UpdatePath, out j);
        }

        public void UpdatePath(Vector3[] path, bool successful)
        {
            this.path = path;
            this.successful = successful;
        }

        public void OnDrawGizmos()
        {
            if (successful)
            {
                Gizmos.DrawCube(player.position, Vector3.one / 2);
                Gizmos.DrawLine(path[0], player.position);
                if (path.Length >= 2)
                {
                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(path[i], Vector3.one / 10);
                        Gizmos.DrawLine(path[i], path[i + 1]);

                    }
                }
                Gizmos.DrawCube(path[path.Length - 1], Vector3.one / 2);

            }
        }
    }
    
}