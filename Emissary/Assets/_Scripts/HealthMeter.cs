using UnityEngine;
using System.Collections;

namespace Emissary
{
    public class HealthMeter : MonoBehaviour
    {

        public int maxHealth;
        int currentHealth;
        Color color;

        public float percentHealth
        {
            get
            {
                return 100.0f * maxHealth / currentHealth;
            }
        }
        public void UpdateHealth(int health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
        }

        public void UpdateColor()
        {
            if (percentHealth < 30)
            {
                color = Color.red;
            }
            else if (percentHealth < 60)
            {
                color = Color.yellow;
            }
            else if (percentHealth < 90)
            {
                color = Color.green;
            }
            else
            {
                color = Color.blue;
            }
        }

        void Start()
        {
            currentHealth = maxHealth;
            UpdateColor();
        }

        public void Damage(int dmg)
        {
            UpdateHealth(currentHealth - dmg);
        }
    }
    
}