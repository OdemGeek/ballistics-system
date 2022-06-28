using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdemIdea.Ballistics
{
    [AddComponentMenu("Ballistics/BulletMuzzle")]
    public class BulletMuzzle : MonoBehaviour
    {
        [SerializeField]
        private bool shot = false;
        [SerializeField]
        private GameObject m_bullet;

        public void OnValidate()
        {
            if (shot)
            {
                shot = false;
                Fire();
            }
        }

        public void Fire()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            GameObject GO = Instantiate(m_bullet, transform.position, transform.rotation);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}
