using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{

    static private SoundManager s_instance;

    static public SoundManager Get()
    {
        return s_instance;
    }

	void Awake()
    {
        s_instance = this;
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
