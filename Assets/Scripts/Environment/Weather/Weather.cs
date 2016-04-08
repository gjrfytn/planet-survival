using UnityEngine;
using System.Collections;

public class Weather : WeatherBase
    {
        private static readonly Color32 explosionColor = new Color32(255, 255, 255, 255);

        private float CameraMultiplier = 1.0f;
        private Bounds VisibleBounds;
        private float Yoffset;
        private float VisibleWorldWidth;
        private float InitialEmissionRain;
        private float InitialStartSpeed;
        private float InitialStartSize;
        private float InitialStartSpeedMist;
        private float InitialStartSizeMist;
        private float InitialStartSpeedExplosion;
        private float InitialStartSizeExplosion;
        private readonly ParticleSystem.Particle[] particles = new ParticleSystem.Particle[2048];

        public float RainHeightMultiplier = 0.15f;

        public float RainWidthMultiplier = 1.5f;

        public LayerMask CollisionMask;

        [Range(0.0f, 0.5f)]
        public float CollisionLifeTimeRain = 0.02f;

        [Range(0.0f, 0.99f)]
        public float RainMistCollisionMultiplier = 0.75f;

	    public bool IsRain;
	    public bool IsSnow;
	    public bool IsSandstorm;

        private void EmitExplosion(ref Vector3 pos)
        {
            int count = UnityEngine.Random.Range(2, 5);
            while (count != 0)
            {
                float xVelocity = Random.Range(-2.0f, 2.0f) * CameraMultiplier;
                float yVelocity = Random.Range(1.0f, 3.0f) * CameraMultiplier;
                float lifetime = Random.Range(0.1f, 0.2f);
                float size = Random.Range(0.05f, 0.1f) * CameraMultiplier;
                ParticleSystem.EmitParams param = new ParticleSystem.EmitParams();
                param.position = pos;
                param.velocity = new Vector3(xVelocity, yVelocity, 0.0f);
                param.startLifetime = lifetime;
                param.startSize = size;
                param.startColor = explosionColor;
                ExplosionParticleSystem.Emit(param, 1);
                count--;
            }
        }

        private void TransformParticleSystem(ParticleSystem particle, float initialStartSpeed, float initialStartSize)
        {
		if (particle == null)
            {
                return;
            }

			particle.transform.position = new Vector3(Camera.transform.position.x, VisibleBounds.max.y + Yoffset, particle.transform.position.z);
			particle.transform.localScale = new Vector3(VisibleWorldWidth * RainWidthMultiplier, 1.0f, 1.0f);
			particle.startSpeed = initialStartSpeed * CameraMultiplier;
			particle.startSize = initialStartSize * CameraMultiplier;
        }

        private void CheckForCollisionsRainParticles()
        {
            int count = 0;
            bool changes = false;

            if (CollisionMask != 0)
            {
                count = FallParticleSystem.GetParticles(particles);
                RaycastHit2D hit;

                for (int i = 0; i < count; i++)
                {
                    Vector3 pos = particles[i].position + FallParticleSystem.transform.position;
                    hit = Physics2D.Raycast(pos, particles[i].velocity.normalized, particles[i].velocity.magnitude * Time.deltaTime);
                    if (hit.collider != null && ((hit.collider.gameObject.layer << 1) & CollisionMask) == (hit.collider.gameObject.layer << 1))
                    {
                        if (CollisionLifeTimeRain == 0.0f)
                        {
                            particles[i].lifetime = 0.0f;
                        }
                        else
                        {
                            particles[i].lifetime = Mathf.Min(particles[i].lifetime, Random.Range(CollisionLifeTimeRain * 0.5f, CollisionLifeTimeRain * 2.0f));
                            pos += (particles[i].velocity * Time.deltaTime);
                        }
                        changes = true;
                    }
                }
            }

            if (ExplosionParticleSystem != null)
            {
                if (count == 0)
                {
                    count = FallParticleSystem.GetParticles(particles);
                }
                for (int i = 0; i < count; i++)
                {
                    if (particles[i].lifetime < 0.24f)
                    {
                        Vector3 pos = particles[i].position + FallParticleSystem.transform.position;
                        EmitExplosion(ref pos);
                    }
                }
            }
            if (changes)
            {
                FallParticleSystem.SetParticles(particles, count);
            }
        }

        private void CheckForCollisionsMistParticles()
        {
            if (MistParticleSystem == null || CollisionMask == 0)
            {
                return;
            }

            int count = MistParticleSystem.GetParticles(particles);
            bool changes = false;
            RaycastHit2D hit;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = particles[i].position + MistParticleSystem.transform.position;
                hit = Physics2D.Raycast(pos, particles[i].velocity.normalized, particles[i].velocity.magnitude * Time.deltaTime);
                if (hit.collider != null && ((hit.collider.gameObject.layer << 1) & CollisionMask) == (hit.collider.gameObject.layer << 1))
                {
                    particles[i].velocity *= RainMistCollisionMultiplier;
                    changes = true;
                }
            }

            if (changes)
            {
                MistParticleSystem.SetParticles(particles, count);
            }
        }

        protected override void Start()
        {
            base.Start();

            InitialEmissionRain = FallParticleSystem.emission.rate.constantMax;
            InitialStartSpeed = FallParticleSystem.startSpeed;
            InitialStartSize = FallParticleSystem.startSize;

            if (MistParticleSystem != null)
            {
                InitialStartSpeedMist = MistParticleSystem.startSpeed;
                InitialStartSizeMist = MistParticleSystem.startSize;
            }

            if (ExplosionParticleSystem != null)
            {
                InitialStartSpeedExplosion = ExplosionParticleSystem.startSpeed;
                InitialStartSizeExplosion = ExplosionParticleSystem.startSize;
            }
        }

        protected override void Update()
        {
            base.Update();

            CameraMultiplier = (Camera.orthographicSize * 0.25f);
            VisibleBounds.min = Camera.main.ViewportToWorldPoint(Vector3.zero);
            VisibleBounds.max = Camera.main.ViewportToWorldPoint(Vector3.one);
            VisibleWorldWidth = VisibleBounds.size.x;
            Yoffset = (VisibleBounds.max.y - VisibleBounds.min.y) * RainHeightMultiplier;

            TransformParticleSystem(FallParticleSystem, InitialStartSpeed, InitialStartSize);
            TransformParticleSystem(MistParticleSystem, InitialStartSpeedMist, InitialStartSizeMist);
            TransformParticleSystem(ExplosionParticleSystem, InitialStartSpeedExplosion, InitialStartSizeExplosion);

            CheckForCollisionsRainParticles();
            CheckForCollisionsMistParticles();
        }

        protected override float RainFallEmissionRate()
        {
            return InitialEmissionRain * Intensity;
        }
		
    }