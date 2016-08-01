using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public AudioClip m_WaterDragSound;
    public float m_WaterDragVolume = 1f;
    public float m_WaterDragFadeInTime = 1f;
    public float m_WaterDragFadeOutTime = 1f;

    private AudioSource m_WaterDragAudioSource;
    private GameObject m_WaterDragGameObject;
    private Coroutine m_WaterDragFadeCoroutine;

    static private SoundManager s_instance;

    static public SoundManager Get()
    {
        return s_instance;
    }

	void Awake()
    {
        s_instance = this;
	}

    void Start()
    {
        m_WaterDragGameObject = new GameObject(m_WaterDragSound.name);
        m_WaterDragAudioSource = m_WaterDragGameObject.AddComponent<AudioSource>();
        m_WaterDragAudioSource.volume = 0f;
        m_WaterDragAudioSource.loop = true;
        m_WaterDragAudioSource.clip = m_WaterDragSound;
        m_WaterDragGameObject.transform.localPosition = Vector3.zero;
    }

    public void PlayOneShotSound(AudioClip clip, float volume, Vector3 position, Transform parent=null)
    {
        StartCoroutine(CreateOneShotSound(clip, volume, position, parent));
    }
 
    public void PlayOneShotRandomSound(List<AudioClip> clips, float volume, Vector3 position, Transform parent=null)
    {
        AudioClip clip = clips[Random.Range(0, clips.Count)];
        StartCoroutine(CreateOneShotSound(clip, volume, position, parent));
    }

    public void StartWaterDrag(Vector3 position)
    {
        m_WaterDragGameObject.transform.position = position;
        if (m_WaterDragFadeCoroutine != null)
            StopCoroutine(m_WaterDragFadeCoroutine);
        m_WaterDragFadeCoroutine = StartCoroutine(FadeSound(m_WaterDragFadeInTime, m_WaterDragAudioSource.volume, m_WaterDragVolume));
        m_WaterDragAudioSource.Play();
    }

    public void UpdateWaterDrag(Vector3 position)
    {
        m_WaterDragGameObject.transform.position = position;
    }

    public void StopWaterDrag()
    {
        if (m_WaterDragFadeCoroutine != null)
            StopCoroutine(m_WaterDragFadeCoroutine);
        m_WaterDragFadeCoroutine = StartCoroutine(FadeSound(m_WaterDragFadeOutTime, m_WaterDragAudioSource.volume, 0f)); 
    }

    private IEnumerator FadeSound(float time, float startVolume, float endVolume)
    {
        float startTime = Time.time;
        float endTime = Time.time + time;
        while (Time.timeSinceLevelLoad < endTime)
        {
            float vol = (Time.time - startTime) / (endTime - startTime);
            m_WaterDragAudioSource.volume =  Mathf.Lerp(startVolume, endVolume, vol);
            yield return null;
        }
        m_WaterDragAudioSource.volume = endVolume;
    }

    private IEnumerator CreateOneShotSound(AudioClip clip, float volume, Vector3 position, Transform parent)
    {
        GameObject go = new GameObject(clip.name);
        AudioSource source = go.AddComponent<AudioSource>();
        source.volume = volume;
        source.clip = clip;
        if (parent)
            go.transform.parent = parent;
        go.transform.localPosition = position;
        source.Play();

        while (source.isPlaying)
            yield return null;

        yield return null;
        Destroy(go);
    }
}
