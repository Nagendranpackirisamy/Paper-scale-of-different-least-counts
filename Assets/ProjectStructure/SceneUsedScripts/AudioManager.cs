using UnityEngine;
using System.Collections.Generic;


namespace paperScaleOfDifferentLeastCounts
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
    
        [System.Serializable]
        public class SFX
        {
            public string name;
            public AudioClip clip;
        }
    
        [Header("SFX Library")]
        public List<SFX> sfxLibrary = new List<SFX>();
    
        private Dictionary<string, AudioClip> sfxDictionary;
    
        private AudioSource sfxSource;
    
        void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
    
            // Create AudioSource
            sfxSource = gameObject.AddComponent<AudioSource>();
    
            // Build dictionary
            sfxDictionary = new Dictionary<string, AudioClip>();
    
            foreach (var sfx in sfxLibrary)
            {
                if (!sfxDictionary.ContainsKey(sfx.name))
                {
                    sfxDictionary.Add(sfx.name, sfx.clip);
                }
            }
        }
    
        public void PlaySFX(string name)
        {
            if (sfxDictionary.TryGetValue(name, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("SFX not found: " + name);
            }
        }
    }
    
}