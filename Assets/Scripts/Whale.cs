using UnityEngine;
using System.Collections;

public class Whale : MonoBehaviour {

	public int m_StartHealth = 15;
	public int m_MaxHealth = 30;

	public AudioClip m_WhaleHappySound;
	public AudioClip m_WhaleDeadSound;
	public float m_WhaleVolume = 0.5f;
    public float m_EffectTimeMax = 0.4f;

	public Vector3 m_SpawnLocation;
	private int m_currentHealth;

	protected enum EWhaleState {
		INTRO,
		IDLE,
		OUTRO,
		DEATH
	};

    protected enum EEffectType
    {
        EAT,
        DAMAGE,
    };

	private EWhaleState m_state;
    private EEffectType m_effectType;
	private float m_stateTime = 0.0f;
    private float m_effectTime = 0.0f;

    protected void SetEffect (EEffectType type)
    {
        m_effectType = type;
        m_effectTime = m_EffectTimeMax;
    }

	public void Damage () {
		if (m_currentHealth > 0) {
			m_currentHealth--;

			if (m_currentHealth == 0) {
				m_state = EWhaleState.DEATH;
				m_stateTime = 0;
				SoundManager.Get().PlayOneShotSound(m_WhaleDeadSound, m_WhaleVolume, transform.position);
			}
            else
            {
                SetEffect(EEffectType.DAMAGE);
            }
		}
	}

	public void Heal () {
		if (m_currentHealth < m_MaxHealth) {
			m_currentHealth++;
		
			if (m_currentHealth == m_MaxHealth) {
				m_state = EWhaleState.OUTRO;
				m_stateTime = 0;
				SoundManager.Get().PlayOneShotSound(m_WhaleHappySound, m_WhaleVolume, transform.position);
			}
		}
	}

	private float m_IntroTime = 2.5f;
	private float m_OutroTime = 1.5f;
	private float m_DeathTime = 0.5f;

	private float m_AnimXDist = 8.0f;
	//private float m_AnimZDist = 5.0f;
	//private float m_AnimLookUpFactor = 0.8f;

	void UpdateIntroAnimState (float alpha) {
		float curvedAlpha = Mathf.Sin(alpha * Mathf.PI / 2.0f);
		GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, curvedAlpha);

		Vector3 pos = transform.position;
		pos.x = m_SpawnLocation.x - m_AnimXDist * (1.0f - curvedAlpha);
		//pos.y = m_SpawnLocation.y - m_AnimZDist * (1.0f - curvedAlpha);
		transform.position = pos;

		//Vector3 lookAt = pos + new Vector3(1.0f, m_AnimLookUpFactor * (1.0f - curvedAlpha), 0.0f);
		//transform.LookAt(lookAt);
	}

	void UpdateIdleAnimState () {
		transform.position = m_SpawnLocation;
	}
		
	void UpdateOutroAnimState (float alpha) {
		//float curvedAlpha = 1.0f - Mathf.Cos(alpha * Mathf.PI / 2.0f);
		//float curvedAlpha = alpha;
		float curvedAlpha = Mathf.Sin(alpha * Mathf.PI / 2.0f);
		GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - curvedAlpha);

		Vector3 pos = transform.position;
		pos.x = m_SpawnLocation.x + m_AnimXDist * curvedAlpha;
		//pos.y = m_SpawnLocation.y + m_AnimZDist * curvedAlpha;
		transform.position = pos;

		//Quaternion rot = transform.rotation;
		//rot.R
		//pos.y = m_SpawnLocation.y + m_AnimZDist * curvedAlpha;
		//transform.rotation = rot;

		//Vector3 lookAt = pos + new Vector3(1.0f, -m_AnimLookUpFactor * curvedAlpha * 0.8f, 0.0f);
		//transform.LookAt(lookAt);
	}

	void UpdateDeathAnimState (float alpha) {
		//float curvedAlpha = 1.0f - Mathf.Cos(alpha * Mathf.PI / 2.0f);
		float curvedAlpha = alpha;
		//float curvedAlpha = Mathf.Sin(alpha * Mathf.PI / 2.0f);
		GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - curvedAlpha);

		//Vector3 pos = transform.position;
		//pos.x = m_SpawnLocation.x + m_AnimXDist * curvedAlpha;
		//pos.y = m_SpawnLocation.y + m_AnimZDist * curvedAlpha;
		//transform.position = pos;

		//Vector3 lookAt = pos + new Vector3(1.0f, -m_AnimLookUpFactor * curvedAlpha * 0.8f, 0.0f);
		//transform.LookAt(lookAt);
	}

	void UpdateState () {
		switch (m_state) {
			case EWhaleState.INTRO:				
				if (m_stateTime < m_IntroTime) {
					UpdateIntroAnimState(m_stateTime / m_IntroTime);
				} else {
					m_state = EWhaleState.IDLE;
					m_stateTime = 0.0f;
					transform.position = m_SpawnLocation;			
				}
				break;
			case EWhaleState.OUTRO:
				if (m_stateTime < m_OutroTime) {
					UpdateOutroAnimState(m_stateTime / m_OutroTime);
				} else {
					WhaleManager manager = GetComponentInParent<WhaleManager>();
					manager.WhaleFullyFed(this.gameObject);
				}
				break;
			case EWhaleState.DEATH:
				if (m_stateTime < m_DeathTime) {
				UpdateDeathAnimState(m_stateTime / m_DeathTime);
				} else {
					WhaleManager manager = GetComponentInParent<WhaleManager>();
					manager.WhaleKilled(this.gameObject);
				}
				break;
		}

        if (m_effectTime > 0.0f)
        {
            Color clr = GetComponent<Renderer>().material.color;
            Color newClr;
            if (m_effectType == EEffectType.DAMAGE)
                newClr = new Color(2.0f, 0.3f, 0.3f, clr.a);
            else if (m_effectType == EEffectType.EAT)
                newClr = new Color(0.2f, 1.0f, 0.0f, clr.a);
            else
                newClr = new Color(1.0f, 1.0f, 1.0f, clr.a);

            m_effectTime -= Time.deltaTime;
            GetComponent<Renderer>().material.color = Color.Lerp(new Color(1.0f, 1.0f, 1.0f, clr.a), newClr, Mathf.Max(m_effectTime / m_EffectTimeMax, 0.0f));
        }
  }

	// Use this for initialization
	void Start () {
		m_currentHealth = m_StartHealth;

		m_state = EWhaleState.INTRO;
		m_stateTime = 0;
        m_effectTime = 0;
		UpdateState();
	}
	
	// Update is called once per frame
	void Update () {
		m_stateTime += Time.deltaTime;
		UpdateState();
	}

	void OnTriggerEnter (Collider other) {
		Fish fish = other.gameObject.GetComponent<Fish>();
		if (fish != null && fish.CanEat() && m_state == EWhaleState.IDLE) {
			fish.Eat();
			Heal();
            // SetEffect(EEffectType.EAT);
		}
	}
		
}
