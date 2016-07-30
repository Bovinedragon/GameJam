using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CBoatManager : MonoBehaviour {

    // Class settings
    public GameObject m_boatModel;
    public GameObject m_harpoonModel;
    public CWaterSimulation m_waterSimulation;
    public WhaleManager m_whaleManager;

    public uint m_boatCount = 4;

    public float m_boatIdleSpeed = 3.0f; // Boat speed in IDLE state
    public float m_boatIdleRange = 64.0f; // Boat max search range in IDLE mode
    public float m_boatChaseSpeed = 5.0f; // Boat speed in CHASE state
    public float m_boatTurnSpeed = 0.15f; // Boat max angular speed
    public float m_boatIdleTimeMax = 6.0f; // How long a boat can pursue a single target while in idle
    public float m_occlusionHeight = 0.1f;
    public float m_chaseDetectionRange = 48.0f;
    public float m_chaseDetectionProjection = 0.7f;
    public float m_assaultRange = 16.0f;
    public float m_harpoonTime = 2.0f;

    public float m_vfTurnMagnitude = 1.0f;
    public float m_vfPushMagnitude = 3.0f;

    // Internal definitions
    protected enum EBoatState
    {
        IDLE,
        UNSTUCK,
        CHASE,
        ASSAULT,
    };

    protected class CBoat
    {
        public GameObject m_handle;
        public Vector2 m_moveDirection;
        public Vector2 m_targetPoint;
        public EBoatState m_state;
        public float m_stateTime;
        public GameObject m_targetWhale;
    };

    protected CBoat[] m_boats;
    protected GameObject[] m_harpoons;

    protected bool m_initialized;

    //-------------------------------------------------------------------------
    // Implementation

    void Awake()
    {
        // Only allow one harpoon active per boat at a time
        m_boats = new CBoat[m_boatCount];
        m_harpoons = new GameObject[m_boatCount];

        m_initialized = false;
    }

    // Use this for initialization
    void Start()
    {

    }

    void CreateBoats()
    {
        if (m_waterSimulation == null || !m_waterSimulation.HasObstruction())
            return;

        Vector2 boatPos = new Vector2();
        Vector2 ori = new Vector2();
        float anglMod = Random.Range(0.0f, 1.0f);
        float minDist = m_waterSimulation.m_Scale.x / 8.0f;
        float maxDist = m_waterSimulation.m_Scale.x / 2.0f;

        for (uint i = 0; i < m_boatCount; i++)
        {
            m_boats[i] = new CBoat();

            float angle = 3.1415926f * 2.0f * (i + anglMod) / m_boatCount;
            ori.x = Mathf.Cos(angle);
            ori.y = Mathf.Sin(angle);

            uint j;
            for (j = 0; j < 10; j++)
            {
                float dist = Random.Range(minDist, maxDist);
                boatPos = ori * dist;
                if (m_waterSimulation.GetObstruction(boatPos.x, boatPos.y) > m_occlusionHeight)
                    break;
            }

            // Unlikely, but just don't create the boat for now =P
            if (j >= 10)
            {
                Debug.Log("Could not find place for boat =(");
                continue;
            }

            GameObject newBoat = GameObject.Instantiate(m_boatModel);
            newBoat.transform.SetParent(transform);
            newBoat.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
            newBoat.transform.position = new Vector3(boatPos.x, 0.0f, boatPos.y);

            m_boats[i].m_handle = newBoat;
            m_boats[i].m_state = EBoatState.IDLE;
            m_boats[i].m_targetPoint = new Vector2(Random.Range(boatPos.x - m_boatIdleRange, boatPos.x + m_boatIdleRange), Random.Range(boatPos.y - m_boatIdleRange, boatPos.y + m_boatIdleRange));
            m_boats[i].m_moveDirection = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            m_boats[i].m_moveDirection.Normalize();
            m_boats[i].m_stateTime = m_boatIdleTimeMax;
        }

        m_initialized = true;
    }

    protected void SetBoatIdle(CBoat boat)
    {
        boat.m_targetPoint.x = Mathf.Clamp(Random.Range(boat.m_handle.transform.position.x - m_boatIdleRange, boat.m_handle.transform.position.x + m_boatIdleRange), -m_waterSimulation.m_Scale.x / 2.0f, m_waterSimulation.m_Scale.x / 2.0f);
        boat.m_targetPoint.y = Mathf.Clamp(Random.Range(boat.m_handle.transform.position.z - m_boatIdleRange, boat.m_handle.transform.position.z + m_boatIdleRange), -m_waterSimulation.m_Scale.z / 2.0f, m_waterSimulation.m_Scale.z / 2.0f);
        boat.m_stateTime = m_boatIdleTimeMax;
        boat.m_state = EBoatState.IDLE;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_waterSimulation == null || !m_waterSimulation.HasObstruction())
            return;

        if (!m_initialized)
            CreateBoats();

        // TODO - replace with 2nd-order operation if this becomes too stiff
        float turnAngle = Time.deltaTime * m_boatTurnSpeed;
        float turnX = Mathf.Cos(turnAngle);
        float turnY = Mathf.Sin(turnAngle);

        // Boat movement
        for (uint i = 0; i < m_boatCount; i++)
        {
            CBoat boat = m_boats[i];
            Vector2 boatPos = new Vector2(boat.m_handle.transform.position.x, boat.m_handle.transform.position.z);

            float boatSpeed = 0.0f;

            // State-specific code
            switch (boat.m_state)
            {
                case EBoatState.IDLE:
                case EBoatState.UNSTUCK:
                    boatSpeed = m_boatIdleSpeed;
                    break;

                case EBoatState.CHASE:
                    boatSpeed = m_boatChaseSpeed;
                    boat.m_targetPoint.x = boat.m_targetWhale.transform.position.x;
                    boat.m_targetPoint.y = boat.m_targetWhale.transform.position.z;
                    break;

                case EBoatState.ASSAULT:
                    boatSpeed = 0.0f;
                    break;
            };


            Vector2 vf = m_waterSimulation.SampleVectorFieldWorld(boatPos.x, boatPos.y);

            // Movement code works the same regardless of state
            if (boatSpeed > 0.0f || vf.SqrMagnitude() > 0.0f)
            {
                // Turning
                Vector2 delta = boat.m_targetPoint - boatPos;
                delta.Normalize();
                delta += vf * m_vfTurnMagnitude;

                float perpdp = (delta.x * boat.m_moveDirection.y) - (delta.y * boat.m_moveDirection.x);

                if (perpdp < 0.0f)
                {
                    float tmp = boat.m_moveDirection.x * turnX - boat.m_moveDirection.y * turnY;
                    boat.m_moveDirection.y = boat.m_moveDirection.x * turnY + boat.m_moveDirection.y * turnX;
                    boat.m_moveDirection.x = tmp;
                }
                else
                {
                    float tmp = boat.m_moveDirection.x * turnX + boat.m_moveDirection.y * turnY;
                    boat.m_moveDirection.y = boat.m_moveDirection.y * turnX - boat.m_moveDirection.x * turnY;
                    boat.m_moveDirection.x = tmp;
                }

                // If we completed the turn, just go directly to target
                float newPerp = (delta.x * boat.m_moveDirection.y) - (delta.y * boat.m_moveDirection.x);
                if ((newPerp < 0) != (perpdp < 0))
                {
                    boat.m_moveDirection = delta;
                    boat.m_moveDirection.Normalize();
                }

                boatSpeed += Vector2.Dot(boat.m_moveDirection, vf) * m_vfPushMagnitude;

                // Boat movement
                Vector2 newPos = boatPos + boat.m_moveDirection * boatSpeed * Time.deltaTime;

                // Stuck detection
                if (m_waterSimulation.GetObstruction(newPos.x, newPos.y) < m_occlusionHeight)
                {
                    if (boat.m_state != EBoatState.UNSTUCK)
                        boat.m_targetPoint = boatPos - boat.m_moveDirection * 32.0f;
                    boat.m_state = EBoatState.UNSTUCK;
                }
                else
                    boatPos = newPos;

                Vector3 normal;
                float height = m_waterSimulation.GetWaterHeightNormal(boatPos.x, boatPos.y, out normal);

                // Update position
                boat.m_handle.transform.position = new Vector3(boatPos.x, height, boatPos.y);

                // Update orientation (yeah, it's a bit iffy with axes, broken model =P)
                Vector3 forward = new Vector3(boat.m_moveDirection.x, 0, boat.m_moveDirection.y);
                Vector3 up = normal;
                Vector3 right = Vector3.Cross(up, forward);
                Quaternion q = new Quaternion();
                q.SetLookRotation(up, right);
                boat.m_handle.transform.rotation = q;
            }

            // Post-movement state
            switch (boat.m_state)
            {
                case EBoatState.IDLE:
                case EBoatState.UNSTUCK:
                    boat.m_stateTime -= Time.deltaTime;
                    if ((boatPos - boat.m_targetPoint).SqrMagnitude() < 12.0f * 12.0f || boat.m_stateTime < 0.0f)
                        SetBoatIdle(boat);

                    // Enumerate whales
                    if (m_whaleManager != null)
                    {
                        List<GameObject> whales;
                        m_whaleManager.EnumerateWhalesInRange(boat.m_handle.transform.position, m_chaseDetectionRange, out whales);
                        foreach (GameObject whale in whales)
                        {
                            Vector2 delta = new Vector2();
                            delta.x = whale.transform.position.x - boatPos.x;
                            delta.y = whale.transform.position.z - boatPos.y;
                            delta.Normalize();
                            if( Vector2.Dot(delta, m_boats[i].m_moveDirection) >= m_chaseDetectionProjection )
                            {
                                boat.m_state = EBoatState.CHASE;
                                boat.m_targetWhale = whale;
                            }
                        }
                    }
                    break;

                case EBoatState.CHASE:
                    {
						if (boat.m_targetWhale == null) {
							SetBoatIdle(boat);
							break;
						}

                        Vector2 delta = new Vector2(boat.m_targetWhale.transform.position.x - boatPos.x, boat.m_targetWhale.transform.position.z - boatPos.y);
                        float distSq = delta.SqrMagnitude();
                        if (distSq > m_chaseDetectionRange * m_chaseDetectionRange * 1.1f)
                            SetBoatIdle(boat);
                        else if (distSq < m_assaultRange * m_assaultRange)
                        {
                            boat.m_state = EBoatState.ASSAULT;
                            boat.m_stateTime = m_harpoonTime;
                            m_harpoons[i] = GameObject.Instantiate(m_harpoonModel);
                            m_harpoons[i].transform.SetParent(transform);
                            m_harpoons[i].transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
                            m_harpoons[i].transform.position = new Vector3(boatPos.x, 0.0f, boatPos.y);
                        }
                    }
                    break;

                case EBoatState.ASSAULT:
                    {
						if (boat.m_targetWhale == null) {
							SetBoatIdle(boat);
							Destroy(m_harpoons[i]);
							break;
						}
						
                        Vector2 delta = new Vector2(boat.m_targetWhale.transform.position.x - boatPos.x, boat.m_targetWhale.transform.position.z - boatPos.y);
                        float distSq = delta.SqrMagnitude();
                        if (distSq > m_assaultRange * m_assaultRange * 1.1f)
                        {
                            SetBoatIdle(boat);
                            Destroy(m_harpoons[i]);
                        }
                        else
                        {
                            boat.m_stateTime -= Time.deltaTime;
							if (boat.m_stateTime < 0) 
							{
								boat.m_targetWhale.GetComponent<Whale>().Damage();
								boat.m_stateTime = m_harpoonTime;
							}
                            m_harpoons[i].transform.position = Vector3.Lerp(boat.m_targetWhale.transform.position, boat.m_handle.transform.position, boat.m_stateTime / m_harpoonTime);

                            const float c_arcHeight = 8.0f;
                            float halfTime = m_harpoonTime * 0.5f;
                            float baseSpeed = 2.0f * c_arcHeight / halfTime;
                            float baseAccel = -c_arcHeight / (halfTime * halfTime);

                            float activeTime = m_harpoonTime - boat.m_stateTime;
                            m_harpoons[i].transform.position += new Vector3(0, baseAccel * activeTime * activeTime + baseSpeed * activeTime, 0);

                            float yDir = (2 * baseAccel * activeTime + baseSpeed);
                            m_harpoons[i].transform.right = new Vector3(delta.x, yDir, delta.y);
                        }
                    }
                    break;
            }
        }
	}
}
