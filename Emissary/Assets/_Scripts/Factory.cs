using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Emissary
{
    public class Factory : Unit
    {
        //public PlayerManager PM;
        FilteredQueue<BuildItem> BuildQueue;
        VehicleType productionType = VehicleType.GROUND;

        public void Start()
        {
            InputManager.instance.AddSelectableUnit(this);
            BuildQueue = new FilteredQueue<BuildItem>();
            type = VehicleType.BUILDING;
        }

        public void EnqueueBuildItem(Unit unit, int massCost, int energyCost, int massRate, int energyRate)
        {
            BuildQueue.Enqueue(new BuildItem(unit, massCost, energyCost, massRate, energyRate));
            TryBuildNextItem();
            unit.PM = this.PM;
            foreach (Vector3 loc in pathQueue)
            {
                unit.FactoryAddPathLocation(loc);
            }
            unit.type = productionType;

        }

        private void TryBuildNextItem()
        {
            if (BuildQueue.Count > 0 && (currentState != UnitState.Building))
            {
                StartCoroutine(Build(BuildQueue.Dequeue()));
            }
        }

        IEnumerator Build(BuildItem item)
        {
            float currentMass = 0;
            float currentEnergy = 0;

            while (currentMass < item.massCost || currentEnergy < item.energyCost)
            {
                PM.RequestResources(item.massRate * Time.deltaTime, item.energyRate * Time.deltaTime);
                yield return null;
            }
            GameObject unit;
            unit = Instantiate(item.item.gameObject);
            unit.transform.position = transform.position;
            unit.GetComponent<Unit>().TryProcessNextPath();
            yield return null;

            currentState = UnitState.Stopped;
            TryBuildNextItem();
        }



        struct BuildItem
        {

            public Unit item;
            public int massCost;
            public int energyCost;
            public int massRate;
            public int energyRate;

            public BuildItem(Unit item, int massTotal, int energyTotal, int massRate, int energyRate)
            {
                this.item = item;
                massCost = massTotal;
                energyCost = energyTotal;
                this.massRate = massRate;
                this.energyRate = energyRate;
            }

            public float percentComplete(int currentMass, int currentEnergy)
            {
                return 50f * (1.0f * currentMass / massCost + 1.0f * currentEnergy / energyCost);
            }
        }
    }

}
