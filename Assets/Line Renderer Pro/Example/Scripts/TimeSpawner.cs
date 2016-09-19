using UnityEngine;

namespace TheKnightsOfUnity.LineRendererPro.Examples
{
    public class TimeSpawner : MonoBehaviour
    {
        public Transform prefab;

        public float delayBetweenSpawn = 1.0f;

        private float _lastSpawn = -Mathf.Infinity;

        private void Update()
        {
            if (Time.time - delayBetweenSpawn > _lastSpawn)
            {
                _lastSpawn = Time.time;

                Instantiate(prefab, transform.position, transform.rotation);
            }
        }
    }
}
