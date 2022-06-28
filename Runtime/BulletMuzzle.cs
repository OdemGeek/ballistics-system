using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
                Fire((BulletInfo bh) => { return; });
            }
        }

        public void Fire(Action<BulletInfo> action)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            GameObject GO = Instantiate(m_bullet, transform.position, transform.rotation);
            GO.GetComponent<Bullet>().action = action;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (BallisticsManager.instance.debug)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, 0.1f);
                Gizmos.DrawRay(transform.position, transform.forward);
            }  
        }
#endif
    }

    public struct BulletInfo
    {
        public List<BulletHit> hits;

        public BulletInfo(List<BulletHit> _hits)
        {
            hits = _hits;
        }
    }

    public struct BulletHit
    {
        public float speed;
        public float angle;
        public Collider collider;
        public Vector3[] positions;

        public BulletHit(float _speed, float _angle, Collider _collider, Vector3[] _positions)
        {
            speed = _speed;
            angle = _angle;
            collider = _collider;
            positions = _positions;
        }
    }
}
