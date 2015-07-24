using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Emissary
{
    public class PlayerManager : MonoBehaviour
    {

        private int id;
        public int ID
        {
            get
            {
                return id;
            }
        }
        float mass;
        float energy;
        float massRate = 20;
        float energyRate = 500;
        List<GameObject> units;
        // Use this for initialization
        void Start()
        {
            Initialize(0);
        }

        // Update is called once per frame
        void Update()
        {
            AddResources(Time.deltaTime * massRate, Time.deltaTime * energyRate);
        }

        public float RequestResources(float massRequest, float energyRequest)
        {
            float factor = 1;
            if (massRequest < 0 || energyRequest < 0)
            {
                Debug.LogError("Resource Request out of range! \nmassRequest:" + massRequest + " || energyRequest:" + energyRequest);
            }

            if (massRequest > mass || energyRequest > energy)
            {
                factor = Mathf.Min(mass / massRequest, energy / energyRequest);
            }

            mass -= factor * massRequest;
            energy -= energyRequest;

            return factor;
        }

        public void AddResources(float mass, float energy)
        {
            this.mass += mass;
            this.energy += energy;
        }

        public void Initialize(int playerID)
        {
            id = playerID;
        }
        void OnGUI()
        {
            GUIContent content = new GUIContent("Mass: " + mass + "\n"
                                              + "Energy: " + energy);
            GUI.Box(new Rect(0, 0, 200, 200), content);
        }
    }

}