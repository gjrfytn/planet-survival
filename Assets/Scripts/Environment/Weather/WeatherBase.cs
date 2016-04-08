using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

    public class WeatherBase : MonoBehaviour
    {
        public Camera Camera;

        public AudioClip SoundLight;

        public AudioClip SoundMedium;

        public AudioClip SoundHeavy;

        [Range(0.0f, 1.0f)]
        public float Intensity;

        public ParticleSystem FallParticleSystem;

        public ParticleSystem ExplosionParticleSystem;

        public ParticleSystem MistParticleSystem;

        [Range(0.0f, 1.0f)]
        public float MistThreshold = 0.5f;

        public AudioClip WindSound;

        public float WindSoundVolumeModifier = 0.5f;

        public WindZone WindZone;

        public Vector3 WindSpeedRange = new Vector3(50.0f, 500.0f, 500.0f);

        public Vector2 WindChangeInterval = new Vector2(5.0f, 30.0f);

        public bool EnableWind = true;

        protected LoopingAudioSource audioSourceLight;
        protected LoopingAudioSource audioSourceMedium;
        protected LoopingAudioSource audioSourceHeavy;
        protected LoopingAudioSource audioSourceCurrent;
        protected LoopingAudioSource audioSourceWind;
        protected Material material;
        protected Material explosionMaterial;
        protected Material mistMaterial;

        private float lastIntensityValue = -1.0f;
        private float nextWindTime;

        private void UpdateWind()
        {
            if (EnableWind && WindZone != null && WindSpeedRange.y > 1.0f)
            {
                WindZone.gameObject.SetActive(true);
                WindZone.transform.position = Camera.transform.position;
                if (!Camera.orthographic)
                {
                    WindZone.transform.Translate(0.0f, WindZone.radius, 0.0f);
                }
                if (nextWindTime < Time.time)
                {
                    WindZone.windMain = UnityEngine.Random.Range(WindSpeedRange.x, WindSpeedRange.y);
                    WindZone.windTurbulence = UnityEngine.Random.Range(WindSpeedRange.x, WindSpeedRange.y);
                    if (Camera.orthographic)
                    {
                        int val = UnityEngine.Random.Range(0, 2);
                        WindZone.transform.rotation = Quaternion.Euler(0.0f, (val == 0 ? 90.0f : -90.0f), 0.0f);
                    }
                    else
                    {
                        WindZone.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(-30.0f, 30.0f), UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
                    }
                    nextWindTime = Time.time + UnityEngine.Random.Range(WindChangeInterval.x, WindChangeInterval.y);
                    audioSourceWind.Play((WindZone.windMain / WindSpeedRange.z) * WindSoundVolumeModifier);
                }
            }
            else
            {
                if (WindZone != null)
                {
                    WindZone.gameObject.SetActive(false);
                }
                audioSourceWind.Stop();
            }

            audioSourceWind.Update();
        }

        private void CheckForRainChange()
        {
            if (lastIntensityValue != Intensity)
            {
                lastIntensityValue = Intensity;
                if (Intensity <= 0.01f)
                {
                    if (audioSourceCurrent != null)
                    {
                        audioSourceCurrent.Stop();
                        audioSourceCurrent = null;
                    }
                    if (FallParticleSystem != null)
                    {
                        ParticleSystem.EmissionModule e = FallParticleSystem.emission;
                        e.enabled = false;
                        FallParticleSystem.Stop();
                    }
                    if (MistParticleSystem != null)
                    {
                        ParticleSystem.EmissionModule e = MistParticleSystem.emission;
                        e.enabled = false;
                        MistParticleSystem.Stop();
                    }
                }
                else
                {
                    LoopingAudioSource newSource;
                    if (Intensity >= 0.67f)
                    {
                        newSource = audioSourceHeavy;
                    }
                    else if (Intensity >= 0.33f)
                    {
                        newSource = audioSourceMedium;
                    }
                    else
                    {
                        newSource = audioSourceLight;
                    }
                    if (audioSourceCurrent != newSource)
                    {
                        if (audioSourceCurrent != null)
                        {
                            audioSourceCurrent.Stop();
                        }
                        audioSourceCurrent = newSource;
                        audioSourceCurrent.Play(1.0f);
                    }
                    if (FallParticleSystem != null)
                    {
                        ParticleSystem.EmissionModule e = FallParticleSystem.emission;
                        e.enabled = FallParticleSystem.GetComponent<Renderer>().enabled = true;
                        FallParticleSystem.Play();
                        ParticleSystem.MinMaxCurve rate = e.rate;
                        rate.mode = ParticleSystemCurveMode.Constant;
                        rate.constantMin = rate.constantMax = RainFallEmissionRate();
                        e.rate = rate;
                    }
                    if (MistParticleSystem != null)
                    {
                        ParticleSystem.EmissionModule e = MistParticleSystem.emission;
                        e.enabled = MistParticleSystem.GetComponent<Renderer>().enabled = true;
                        MistParticleSystem.Play();
                        float emissionRate;
                        if (Intensity < MistThreshold)
                        {
                            emissionRate = 0.0f;
                        }
                        else
                        {
                            emissionRate = MistEmissionRate();
                        }
                        ParticleSystem.MinMaxCurve rate = e.rate;
                        rate.mode = ParticleSystemCurveMode.Constant;
                        rate.constantMin = rate.constantMax = emissionRate;
                        e.rate = rate;
                    }
                }
            }
        }

        protected virtual void Start()
        {

#if DEBUG

            if (FallParticleSystem == null)
            {
                Debug.LogError("Fall particle is not detected");
                return;
            }

#endif

            if (Camera == null)
            {
                Camera = Camera.main;
            }

            audioSourceLight = new LoopingAudioSource(this, SoundLight);
            audioSourceMedium = new LoopingAudioSource(this, SoundMedium);
            audioSourceHeavy = new LoopingAudioSource(this, SoundHeavy);
            audioSourceWind = new LoopingAudioSource(this, WindSound);

            if (FallParticleSystem != null)
            {
                ParticleSystem.EmissionModule e = FallParticleSystem.emission;
                e.enabled = false;
                Renderer rainRenderer = FallParticleSystem.GetComponent<Renderer>();
                rainRenderer.enabled = false;
                material = new Material(rainRenderer.material);
                rainRenderer.material = material;
            }
            if (ExplosionParticleSystem != null)
            {
                ParticleSystem.EmissionModule e = ExplosionParticleSystem.emission;
                e.enabled = false;
                Renderer rainRenderer = ExplosionParticleSystem.GetComponent<Renderer>();
                explosionMaterial = new Material(rainRenderer.material);
                rainRenderer.material = explosionMaterial;
            }
            if (MistParticleSystem != null)
            {
                ParticleSystem.EmissionModule e = MistParticleSystem.emission;
                e.enabled = false;
                Renderer rainRenderer = MistParticleSystem.GetComponent<Renderer>();
                rainRenderer.enabled = false;
                mistMaterial = new Material(rainRenderer.material);
                rainRenderer.material = mistMaterial;
            }
        }

        protected virtual void Update()
        {

            if (FallParticleSystem == null)
            {
                Debug.LogError("Fall particle is not detected");
                return;
            }

            CheckForRainChange();
            UpdateWind();
            audioSourceLight.Update();
            audioSourceMedium.Update();
            audioSourceHeavy.Update();
        }

        protected virtual float RainFallEmissionRate()
        {
            return (FallParticleSystem.maxParticles / FallParticleSystem.startLifetime) * Intensity;
        }

        protected virtual float MistEmissionRate()
        {
            return (MistParticleSystem.maxParticles / MistParticleSystem.startLifetime) * Intensity * Intensity;
        }
    }
	
    public class LoopingAudioSource
    {
        public AudioSource AudioSource { get; private set; }
        public float TargetVolume { get; private set; }

        public LoopingAudioSource(MonoBehaviour script, AudioClip clip)
        {
            AudioSource = script.gameObject.AddComponent<AudioSource>();
            AudioSource.loop = true;
            AudioSource.clip = clip;
            AudioSource.Stop();
            TargetVolume = 1.0f;
        }

        public void Play(float targetVolume)
        {
            if (!AudioSource.isPlaying)
            {
                AudioSource.volume = 0.0f;
                AudioSource.Play();
            }
            TargetVolume = targetVolume;
        }

        public void Stop()
        {
            TargetVolume = 0.0f;
        }

        public void Update()
        {
            if (AudioSource.isPlaying && (AudioSource.volume = Mathf.Lerp(AudioSource.volume, TargetVolume, Time.deltaTime)) == 0.0f)
            {
                AudioSource.Stop();
            }
        }
    }
    