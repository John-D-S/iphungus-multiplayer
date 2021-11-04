using UnityEngine;

namespace AltarChase
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;

        // Update is called once per frame
        void Update()
        {
            transform.position += transform.right * (Time.deltaTime * speed * Input.GetAxis("Horizontal"));
            transform.position += transform.forward * (Time.deltaTime * speed * Input.GetAxis("Vertical"));
        }
    }
}