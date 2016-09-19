using UnityEngine;

namespace TheKnightsOfUnity.LineRendererPro.Examples
{
    public class ForceDirectionArea : MonoBehaviour
    {
        public Vector3 direction;

        private void OnTriggerStay(Collider collider)
        {
            var rigidbody = collider.GetComponent<Rigidbody>();

            if (rigidbody != null && !rigidbody.isKinematic)
            {
                rigidbody.AddForce(transform.TransformDirection(direction));
            }
        }
    }
}
