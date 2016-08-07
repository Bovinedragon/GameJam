using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour {

	public float m_IntroTime = 0.25f;
    public float m_EatTime = 0.4f;

	private Vector3 m_velocity;
	private float m_AnimTime;
    private bool m_Eaten = false;

    public bool CanEat()
    {
        return !m_Eaten;
    }

	public void Eat () {
        m_AnimTime = 0.0f;
        m_Eaten = true;
	}

	void UpdateAnim () {
        if (m_Eaten)
            transform.localScale = Vector3.one * Mathf.Max((m_EatTime - m_AnimTime) / m_EatTime, 0.0f);
		else if (m_AnimTime < m_IntroTime) {
			transform.localScale = Vector3.one * Mathf.Max(0.02f, m_AnimTime / m_IntroTime);
		} else {
			transform.localScale = Vector3.one;
		}
	}

	// Use this for initialization
	void Start () {
		UpdateAnim();
	}
	
	// Update is called once per frame
	void Update () {
		m_AnimTime += Time.deltaTime;
		UpdateAnim();
	}
}
