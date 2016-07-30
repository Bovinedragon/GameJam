using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CBoatManager : MonoBehaviour {

    // Class settings
    public GameObject m_boatModel;
    public GameObject m_harpoonModel;
    public CWaterSimulation m_waterSimulation;

    public uint m_boatCount = 4;

    public float m_boatIdleSpeed = 3.0f; // Boat speed in IDLE state
    public float m_boatIdleRange = 64.0f; // Boat max search range in IDLE mode
    public float m_boatTurnSpeed = 0.15f; // Boat max angular speed

    // Internal definitions
    protected enum EBoatState
    {
        IDLE,
        CHASE,
        ASSAULT,
    };

    protected class CBoat
    {
        public GameObject m_handle;
        public Vector2 m_moveDirection;
        public Vector2 m_targetPoint;
        public EBoatState m_state;
    };

    protected class CHarpoon
    {
        public GameObject m_handle;
    };

    protected CBoat[] m_boats;
    protected CHarpoon[] m_harpoons;

    //-------------------------------------------------------------------------
    // Implementation

    void Awake ()
    {
        // Only allow one harpoon active per boat at a time
        m_boats = new CBoat[m_boatCount];
        m_harpoons = new CHarpoon[m_boatCount];
    }

	// Use this for initialization
	void Start ()
    {
        if (m_waterSimulation == null)
            return;

        Vector2 boatPos = new Vector2();
        Vector2 ori = new Vector2();
        float anglMod = Random.Range(0.0f, 1.0f);
        float minDist = m_waterSimulation.m_Scale.x / 8.0f;
        float maxDist = m_waterSimulation.m_Scale.x / 2.0f;

        for (uint i = 0; i < m_boatCount; i++)
        {
            m_boats[i] = new CBoat();
            m_harpoons[i] = new CHarpoon();

            float angle = 3.1415926f * 2.0f * (i + anglMod) / m_boatCount;
            ori.x = Mathf.Cos(angle);
            ori.y = Mathf.Sin(angle);

            uint j;
            for (j = 0; j < 10; j++)
            {
                float dist = Random.Range(minDist, maxDist);
                boatPos = ori * dist;
                if (m_waterSimulation.GetObstruction(boatPos.x, boatPos.y) > 0.9f)
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
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_waterSimulation == null)
            return;

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
            bool stuck = false;

            // State-specific code
            switch (boat.m_state)
            {
                case EBoatState.IDLE:
                    boatSpeed = m_boatIdleSpeed;
                    break;
            };

            // Movement code works the same regardless of state
            if (boatSpeed > 0.0f)
            {
                // Turning
                Vector2 delta = boat.m_targetPoint - boatPos;
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

                // Boat movement
                Vector2 newPos = boatPos + boat.m_moveDirection * boatSpeed * Time.deltaTime;

                // Stuck detection
                if (m_waterSimulation.GetObstruction(newPos.x,newPos.y) < 0.9f)
                    stuck = true;
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
                    if (stuck || 
                        ((boatPos - boat.m_targetPoint).SqrMagnitude() < 12.0f * 12.0f))
                    {
                        m_boats[i].m_targetPoint.x = Mathf.Clamp(Random.Range(boatPos.x - m_boatIdleRange, boatPos.x + m_boatIdleRange), -m_waterSimulation.m_Scale.x / 2.0f, m_waterSimulation.m_Scale.x / 2.0f);
                        m_boats[i].m_targetPoint.y = Mathf.Clamp(Random.Range(boatPos.y - m_boatIdleRange, boatPos.y + m_boatIdleRange), -m_waterSimulation.m_Scale.z / 2.0f, m_waterSimulation.m_Scale.z / 2.0f);
                    }
                    break;
            }
        }
	}
}
