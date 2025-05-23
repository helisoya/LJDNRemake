using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public static SONG activeSong = null;

    public List<SONG> allSongs = new List<SONG>();

    public float songTransitionSpeed = 2f;

    public bool songSmoothTransitions = true;

    public AudioSource voice;

    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup voiceGroup;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayVoice(AudioClip effect, float volume = 1f, float pitch = 1f)
    {
        if (!Locals.current.Equals("fra")) return;

        if (voice.isPlaying)
        {
            voice.Stop();
        }
        voice.clip = effect;
        voice.volume = volume;
        voice.pitch = pitch;
        voice.outputAudioMixerGroup = voiceGroup;
        voice.Play();
    }


    public void PlaySFX(AudioClip effect, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = CreateNewSource(string.Format("SFX [{0}]", effect.name));
        source.clip = effect;
        source.volume = volume;
        source.pitch = pitch;
        source.outputAudioMixerGroup = sfxGroup;
        source.Play();

        Destroy(source.gameObject, effect.length);
    }

    public void PlaySong(string songName, float maxVolume = 1f, float pitch = 1f, float startingVolume = 0f, bool playOnStart = true, bool loop = true)
    {
        AudioClip song = Resources.Load<AudioClip>("Audio/Music/" + songName);
        if (activeSong != null && activeSong.clipName.Equals(songName)) return;

        print("Loading song : " + songName + "Previous : " + activeSong?.clipName + " - " + activeSong?.clipName.Equals(songName));

        if (song != null)
        {
            activeSong = allSongs.Find(s => s.clip == song);

            if (activeSong == null)
            {
                activeSong = new SONG(songName, song, maxVolume, pitch, startingVolume, playOnStart, loop, bgmGroup);
            }
            else
            {
                activeSong.Stop();
                activeSong.Play();
            }
        }
        else
        {
            activeSong = null;
        }

        StopAllCoroutines();
        StartCoroutine(VolumeLeveling());
    }

    IEnumerator VolumeLeveling()
    {
        while (TransitionSongs())
        {
            yield return new WaitForEndOfFrame();
        }
    }

    bool TransitionSongs()
    {
        bool anyValueChanged = false;

        float speed = songTransitionSpeed * Time.deltaTime;
        for (int i = allSongs.Count - 1; i >= 0; i--)
        {
            SONG song = allSongs[i];
            if (song == activeSong)
            {
                if (song.volume < song.maxVolume)
                {
                    song.volume = songSmoothTransitions ? Mathf.Lerp(song.volume, song.maxVolume, speed) : Mathf.MoveTowards(song.volume, song.maxVolume, speed);
                    anyValueChanged = true;
                }
            }
            else
            {
                if (song.volume > 0)
                {
                    song.volume = songSmoothTransitions ? Mathf.Lerp(song.volume, 0f, speed) : Mathf.MoveTowards(song.volume, 0f, speed);
                    anyValueChanged = true;
                }
                else
                {
                    allSongs.RemoveAt(i);
                    song.DestroySong();
                    continue;
                }
            }
        }
        return anyValueChanged;
    }

    public AudioSource CreateNewSource(string _name)
    {
        AudioSource newSource = new GameObject(_name).AddComponent<AudioSource>();
        newSource.transform.SetParent(transform);
        return newSource;
    }

    [System.Serializable]
    public class SONG
    {
        public AudioSource source;
        public AudioClip clip { get { return source.clip; } set { source.clip = value; } }
        public string clipName;
        public float maxVolume = 1f;

        public SONG(string clipName, AudioClip clip, float _maxVolume, float pitch, float startingVolume, bool playOnStart, bool loop, AudioMixerGroup bgmGroup)
        {
            source = instance.CreateNewSource(string.Format("SONG [{0}]", clip.name));
            source.clip = clip;
            source.volume = startingVolume;
            maxVolume = _maxVolume;
            source.pitch = pitch;
            source.loop = loop;
            source.outputAudioMixerGroup = bgmGroup;
            this.clipName = clipName;

            instance.allSongs.Add(this);

            if (playOnStart)
            {
                source.Play();
            }
        }

        public float volume { get { return source.volume; } set { source.volume = value; } }
        public float pitch { get { return source.pitch; } set { source.pitch = value; } }

        public void Play()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }

        public void Pause()
        {
            source.Pause();
        }

        public void UnPause()
        {
            source.UnPause();
        }

        public void DestroySong()
        {
            instance.allSongs.Remove(this);
            DestroyImmediate(source.gameObject);
        }
    }
}
