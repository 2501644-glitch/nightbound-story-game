using UnityEngine;

public class ProceduralAudio : MonoBehaviour
{
    public static ProceduralAudio Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource loopSource;
    private AudioSource voiceSource;

    private AudioClip sizzleClip;
    private AudioClip biteClip;
    private AudioClip punchClip;
    private AudioClip slowMoClip;
    private AudioClip shadowStepClip;
    private AudioClip serumInjectClip;
    private AudioClip heartbeatClip;
    private AudioClip bossSlamClip;
    private AudioClip gunshotClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;

        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;

        sizzleClip = CreateSizzleClip();
        biteClip = CreateBiteClip();
        punchClip = CreatePunchClip();
        slowMoClip = CreateSlowMoClip();
        shadowStepClip = CreateShadowStepClip();
        serumInjectClip = CreateSerumInjectClip();
        heartbeatClip = CreateHeartbeatClip();
        bossSlamClip = CreateBossSlamClip();
        gunshotClip = CreateGunshotClip();
    }

    public void PlayBurnSizzle(bool play)
    {
        if (play)
        {
            if (!loopSource.isPlaying || loopSource.clip != sizzleClip)
            {
                loopSource.clip = sizzleClip;
                loopSource.volume = 0.35f;
                loopSource.Play();
            }
        }
        else
        {
            if (loopSource.isPlaying && loopSource.clip == sizzleClip)
            {
                loopSource.Stop();
            }
        }
    }

    public void PlayBite() => sfxSource.PlayOneShot(biteClip, 0.95f);
    public void PlayPunch() => sfxSource.PlayOneShot(punchClip, 0.85f);
    public void PlayShadowStep() => sfxSource.PlayOneShot(shadowStepClip, 0.8f);
    public void PlaySerumInject() => sfxSource.PlayOneShot(serumInjectClip, 0.9f);
    public void PlayBossSlam() => sfxSource.PlayOneShot(bossSlamClip, 1.0f);
    public void PlayGunshot() => sfxSource.PlayOneShot(gunshotClip, 0.75f);

    public void PlayVoiceBlip(float pitch = 1.0f)
    {
        voiceSource.pitch = pitch;
        voiceSource.PlayOneShot(punchClip, 0.15f);
    }

    public void PlayHumanHeartbeat(bool play)
    {
        if (play)
        {
            if (!loopSource.isPlaying || loopSource.clip != heartbeatClip)
            {
                loopSource.clip = heartbeatClip;
                loopSource.volume = 0.6f;
                loopSource.Play();
            }
        }
        else
        {
            if (loopSource.isPlaying && loopSource.clip == heartbeatClip)
            {
                loopSource.Stop();
            }
        }
    }

    public void SetPitch(float pitch)
    {
        sfxSource.pitch = pitch;
        loopSource.pitch = pitch;
    }

    private AudioClip CreateSizzleClip()
    {
        int rate = 44100;
        int samples = rate / 2;
        AudioClip clip = AudioClip.Create("SizzleLoop", samples, 1, rate, false);
        float[] data = new float[samples];
        System.Random rnd = new System.Random(42);
        for (int i = 0; i < samples; i++)
            data[i] = ((float)rnd.NextDouble() * 2f - 1f) * 0.15f;
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateBiteClip()
    {
        int rate = 44100;
        int samples = (int)(rate * 0.45f);
        AudioClip clip = AudioClip.Create("BiteCrunch", samples, 1, rate, false);
        float[] data = new float[samples];
        System.Random rnd = new System.Random(101);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float env = Mathf.Exp(-t * 8f);
            float noise = ((float)rnd.NextDouble() * 2f - 1f) * 0.4f;
            float lowThump = Mathf.Sin(2f * Mathf.PI * 90f * ((float)i / rate)) * 0.6f;
            data[i] = (noise + lowThump) * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreatePunchClip()
    {
        int rate = 44100;
        int samples = (int)(rate * 0.22f);
        AudioClip clip = AudioClip.Create("PunchImpact", samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(160f, 40f, t);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * ((float)i / rate));
            float env = Mathf.Exp(-t * 12f);
            data[i] = sine * env * 0.7f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSlowMoClip()
    {
        int rate = 44100;
        int samples = rate;
        AudioClip clip = AudioClip.Create("SlowMoDrone", samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
            data[i] = Mathf.Sin(2f * Mathf.PI * 65f * ((float)i / rate)) * 0.25f;
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateShadowStepClip()
    {
        int rate = 44100;
        int samples = (int)(rate * 0.35f);
        AudioClip clip = AudioClip.Create("ShadowStepWhoosh", samples, 1, rate, false);
        float[] data = new float[samples];
        System.Random rnd = new System.Random(77);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = ((float)rnd.NextDouble() * 2f - 1f);
            float lowSweep = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(300f, 60f, t) * ((float)i / rate));
            float env = Mathf.Sin(Mathf.PI * t);
            data[i] = (noise * 0.3f + lowSweep * 0.7f) * env * 0.8f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSerumInjectClip()
    {
        int rate = 44100;
        int samples = (int)(rate * 0.8f);
        AudioClip clip = AudioClip.Create("SerumInject", samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float hiss = Mathf.Sin(2f * Mathf.PI * 1800f * ((float)i / rate)) * 0.25f;
            float bassRise = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(50f, 180f, t) * ((float)i / rate)) * 0.7f;
            float env = Mathf.Exp(-t * 3f);
            data[i] = (hiss + bassRise) * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateHeartbeatClip()
    {
        int rate = 44100;
        int samples = rate; // 1s loop
        AudioClip clip = AudioClip.Create("HeartbeatLoop", samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / rate;
            float thump1 = (t < 0.2f) ? Mathf.Sin(2f * Mathf.PI * 60f * t) * Mathf.Exp(-t * 20f) : 0f;
            float t2 = t - 0.28f;
            float thump2 = (t2 > 0f && t2 < 0.2f) ? Mathf.Sin(2f * Mathf.PI * 55f * t2) * Mathf.Exp(-t2 * 22f) * 0.7f : 0f;
            data[i] = (thump1 + thump2) * 0.85f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateBossSlamClip()
    {
        int rate = 44100;
        int samples = (int)(rate * 0.6f);
        AudioClip clip = AudioClip.Create("BossSlam", samples, 1, rate, false);
        float[] data = new float[samples];
        System.Random rnd = new System.Random(99);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = ((float)rnd.NextDouble() * 2f - 1f) * 0.5f;
            float lowBoom = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(80f, 25f, t) * ((float)i / rate));
            float env = Mathf.Exp(-t * 6f);
            data[i] = (noise + lowBoom) * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateGunshotClip()
    {
        int rate = 44100;
        int samples = (int)(rate * 0.3f);
        AudioClip clip = AudioClip.Create("Gunshot", samples, 1, rate, false);
        float[] data = new float[samples];
        System.Random rnd = new System.Random(13);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = ((float)rnd.NextDouble() * 2f - 1f);
            float env = Mathf.Exp(-t * 25f);
            data[i] = noise * env * 0.9f;
        }
        clip.SetData(data, 0);
        return clip;
    }
}
