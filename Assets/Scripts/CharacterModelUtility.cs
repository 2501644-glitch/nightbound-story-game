using System.Collections.Generic;
using UnityEngine;

public static class CharacterModelUtility
{
    private static RuntimeAnimatorController walkingController;
    private static AnimationClip walkClip;
    private static bool walkingControllerLoaded;

    public static bool PrepareVisual(GameObject visual, string avatarResourcePath, float targetHeight, float targetCenterY)
    {
        if (visual == null) return false;

        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"Character model '{visual.name}' has no renderers.");
            return false;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y <= 0.01f)
        {
            Debug.LogWarning($"Character model '{visual.name}' has invalid height bounds.");
            return false;
        }

        visual.transform.localScale *= targetHeight / bounds.size.y;
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        visual.transform.position += Vector3.up * (targetCenterY - bounds.center.y);

        Animator animator = visual.GetComponentInChildren<Animator>(true);
        if (animator == null) animator = visual.AddComponent<Animator>();
        if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
        {
            Avatar[] avatars = Resources.LoadAll<Avatar>(avatarResourcePath);
            foreach (Avatar avatar in avatars)
            {
                if (avatar != null && avatar.isValid && avatar.isHuman)
                {
                    animator.avatar = avatar;
                    break;
                }
            }
        }

        EnsureAnimationsLoaded();
        if (animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman && walkingController != null)
        {
            animator.applyRootMotion = false;
            animator.runtimeAnimatorController = walkingController;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
        else
        {
            Debug.LogWarning($"Character model '{visual.name}' could not be set up with the walking animation.");
        }

        return true;
    }

    public static Animator FindAnimator(GameObject root)
    {
        return root != null ? root.GetComponentInChildren<Animator>(true) : null;
    }

    public static void ReleaseAnimator(GameObject root)
    {
        if (root == null) return;
        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            AnimatorOverrideController overrides = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrides == null || overrides.runtimeAnimatorController != walkingController) continue;
            animator.runtimeAnimatorController = walkingController;
            Object.Destroy(overrides);
        }
    }

    public static void PlayWalk(Animator animator)
    {
        EnsureAnimationsLoaded();
        PlayClip(animator, walkClip);
    }

    // Enemy models use a separate humanoid animation set matching their melee
    // weapons; keep the hero and cutscene animation choices unchanged.
    public static void PlayEnemyWalk(Animator animator)
    {
        PlayClip(animator, LoadAnimation("Animations/sword and shield walk"));
    }

    public static void PlayEnemyIdle(Animator animator)
    {
        PlayClip(animator, LoadAnimation("Animations/sword and shield idle"));
    }

    public static float PlayEnemyAttack(Animator animator)
    {
        AnimationClip clip = LoadAnimation("Animations/sword and shield attack");
        if (clip == null) clip = LoadAnimation("Animations/sword and shield slash");
        return PlayClip(animator, clip) ? clip.length : 0f;
    }

    public static float PlayEnemyDeath(Animator animator)
    {
        AnimationClip clip = LoadAnimation("Animations/sword and shield death");
        if (clip == null) clip = LoadAnimation("Animations/sword and shield death (2)");
        return PlayClip(animator, clip) ? clip.length : 0f;
    }

    public static void PlayIdle(Animator animator)
    {
        PlayClip(animator, LoadAnimation("Animations/exo_red@Idle"));
    }

    public static float PlayAttack(Animator animator)
    {
        AnimationClip clip = LoadAnimation("Animations/Punch");
        if (clip == null) clip = LoadAnimation("Animations/Kick");
        return PlayClip(animator, clip) ? clip.length : 0f;
    }

    public static float PlayDeath(Animator animator)
    {
        AnimationClip clip = LoadAnimation("Animations/exo_red@Death");
        if (clip == null) clip = LoadAnimation("Animations/sword and shield death");
        if (clip == null) clip = LoadAnimation("Animations/sword and shield death (2)");
        return PlayClip(animator, clip) ? clip.length : 0f;
    }

    private static void EnsureAnimationsLoaded()
    {
        if (walkingControllerLoaded) return;
        walkingControllerLoaded = true;
        walkingController = Resources.Load<RuntimeAnimatorController>("Animations/CharacterWalk");
        if (walkingController == null) return;

        AnimatorOverrideController probe = new AnimatorOverrideController(walkingController);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        probe.GetOverrides(overrides);
        if (overrides.Count > 0) walkClip = overrides[0].Key;
        Object.Destroy(probe);
    }

    private static AnimationClip LoadAnimation(string resourcePath)
    {
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(resourcePath);
        return clips != null && clips.Length > 0 ? clips[0] : null;
    }

    private static bool PlayClip(Animator animator, AnimationClip clip)
    {
        EnsureAnimationsLoaded();
        if (animator == null || clip == null || walkingController == null || walkClip == null) return false;

        AnimatorOverrideController overrides = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (overrides == null || overrides.runtimeAnimatorController != walkingController)
            overrides = new AnimatorOverrideController(walkingController);

        var clips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrides.GetOverrides(clips);
        if (clips.Count == 0) return false;

        for (int i = 0; i < clips.Count; i++)
            clips[i] = new KeyValuePair<AnimationClip, AnimationClip>(clips[i].Key, clip);

        overrides.ApplyOverrides(clips);
        animator.runtimeAnimatorController = overrides;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
        animator.speed = 1f;
        animator.Play("Walk", 0, 0f);
        animator.Update(0f);
        return true;
    }
}
