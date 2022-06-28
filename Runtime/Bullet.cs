using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdemIdea.Ballistics
{
    [AddComponentMenu("Ballistics/Bullet")]
    public class Bullet : MonoBehaviour
    {
        public System.Action<BulletInfo> action;
        [SerializeField]
        [Tooltip("m/s")]
        private float m_speed = 900f;
        [SerializeField]
        [Tooltip("gr")]
        private float m_mass = 4f;
        [SerializeField]
        [Tooltip("gr/cm")]
        private float m_density = 8f;
        [SerializeField]
        [Tooltip("mm")]
        private float m_radius = 4.5f;
        [SerializeField]
        [Tooltip("0 - angle same as been, 1 - angle changes to wall normal")]
        [Range(0f, 1f)]
        private float m_angleLerpPenetration = 0.07f;
        [SerializeField]
        [Tooltip("Randomize angle")]
        private float m_anglePenetration = 1.5f;
        [SerializeField]
        [Tooltip("Randomize angle")]
        private float m_angleRicochet = 3f;
        [SerializeField]
        [Range(0f, 0.999f)]
        private float m_drag = 0.077f;

        private float m_currentSpeed; //m/s
        private int m_countColor = 0;
        private float m_penetratedDistance = 0f;

        private const float minSpeed = 0.5f;
        private static readonly Color[] m_colors = new Color[] { Color.blue, Color.green, Color.magenta, Color.red, Color.cyan, Color.yellow };

        private List<BulletHit> m_hits = new List<BulletHit>();

        private void Start()
        {
            m_currentSpeed = m_speed;
        }

        private void FixedUpdate()
        {
            m_currentSpeed *= 1 - m_drag;
            if (m_currentSpeed < minSpeed)
            {
                action.Invoke(new BulletInfo() { hits = m_hits }); //return action on all hits
                Destroy(gameObject);
                return;
            }
            Vector3 endPos = transform.position + transform.forward * m_currentSpeed;
            Cast(endPos);
        }

        private void Cast(Vector3 endPos)
        {
            if (Physics.Linecast(transform.position, endPos, out RaycastHit hit, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                BallisticsMaterial hitMaterial;
                //get material from the object we hit
                if (hit.collider.gameObject.TryGetComponent(out MaterialHolder holder))
                {
                    hitMaterial = BallisticsManager.GetMaterial(holder.materialName);
                }
                else //or if it doesn't have material take default
                {
                    hitMaterial = BallisticsManager.GetMaterial(0);
                }

                float angle = 180f - Vector3.Angle(transform.forward, hit.normal);
                float penetrationCoefficient = Mathf.Max(m_density - hitMaterial.density, Mathf.Epsilon) * 1000; //the more penetration coeficient is, the easier to penetrate that material
                float P = Mathf.Clamp(m_currentSpeed * (m_mass / 1000) / penetrationCoefficient * Mathf.Sin(angle * Mathf.PI / 180) * 700, 0, 100); // P = V * m / (D1 - D) / sin(angle) | P is chance of ricochet, V is speed, m is mass, D1 is density of bullet, D is density of object

                //check ricochet
                if (Random.Range(0f, 100f) < P && angle > 10)
                {
                    //if ricochet

                    float distRemained = m_currentSpeed * (1 - m_drag * 5) - Vector3.Distance(transform.position, hit.point); //remaining distance

                    //calculate new bullet direction with some random
                    Vector3 newDir = Vector3.Reflect(transform.forward, hit.normal);
                    transform.forward = newDir;
                    transform.Rotate(RandomVector3(-m_angleRicochet, m_angleRicochet));
#if UNITY_EDITOR
                    if (BallisticsManager.instance.debug)
                        Debug.DrawLine(transform.position, hit.point, Color.cyan, 9); //draw line from bullet to wall
#endif
                    transform.position = hit.point;

                    Vector3[] hits = new Vector3[1];
                    hits[0] = hit.point;

                    m_hits.Add(new BulletHit() { collider = hit.collider, angle = angle, speed = m_currentSpeed, positions = hits }); //add hit to list

                    Cast(transform.position + transform.forward * distRemained);
                }
                else
                {
                    //if not
                    //check outer raycast

                    float castDist = Vector3.Distance(transform.position, hit.point);

                    float distRemained = m_currentSpeed /** (1 - m_drag * 5)*/ - castDist; //remaining distance

                    Vector3 newDir = Vector3.Lerp(transform.forward, -hit.normal, m_angleLerpPenetration);
                    transform.forward = newDir;
                    transform.Rotate(RandomVector3(-m_anglePenetration, m_anglePenetration));
#if UNITY_EDITOR
                    if (BallisticsManager.instance.debug)
                        Debug.DrawLine(transform.position, hit.point, RandomColor(), 9); //draw line from bullet to wall
#endif
                    transform.position = hit.point - hit.normal * 0.001f;
                    //check inner raycast
                    //find next surface
                    Vector3 startPoint = transform.position + transform.forward * distRemained;
                    if (Physics.Linecast(transform.position, transform.position + transform.forward * distRemained, out RaycastHit hit2, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    {
                        startPoint = hit2.point + hit2.normal * 0.001f;
                    }
                    int hitCount = 2;
                    //cast ray backwards to find end of first surface
                    if (Physics.Linecast(startPoint, transform.position, out RaycastHit hit3, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    {
                        m_penetratedDistance = Vector3.Distance(hit3.point, transform.position);
                        m_currentSpeed = Mathf.Max(m_currentSpeed - (m_penetratedDistance / penetrationCoefficient * 1000000), 0);

                        transform.position = hit3.point + hit3.normal * 0.001f;

                        distRemained = distRemained - m_penetratedDistance; //get distance after that object
                    }
                    else
                    {
                        hitCount = 1;
#if UNITY_EDITOR
                        if (BallisticsManager.instance.debug)
                            Debug.LogWarning($"Surface with one side found: {hit.collider.gameObject.name}", hit.collider.gameObject);
#endif
                    }
                    Vector3[] hits = new Vector3[hitCount];
                    hits[0] = hit.point;
                    if (hitCount == 2) hits[1] = hit3.point;

                    m_hits.Add(new BulletHit() { collider = hit.collider, angle = angle, speed = m_currentSpeed, positions = hits }); //add hit to list

                    Cast(transform.position + transform.forward * Mathf.Min(distRemained, m_currentSpeed));
                }
                
            }
            else
            {
                transform.position = endPos;
            }
        }

        private Color RandomColor()
        {
            return m_colors[m_countColor++ % m_colors.Length];
        }

        private Vector3 RandomVector3(float min, float max)
        {
            return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            transform.localScale = Vector3.one * m_radius / 1000;
        }
#endif
    }
}
