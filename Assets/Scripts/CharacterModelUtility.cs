using UnityEngine;

public static class CharacterModelUtility
{
    private static RuntimeAnimatorController walkingController;
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
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        if (bounds.size.y <= 0.01f)
        {
            Debug.LogWarning($"Character model '{visual.name}' has invalid height bounds.");
            return false;
        }

        visual.transform.localScale *= targetHeight / bounds.size.y;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
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

        if (!walkingControllerLoaded)
        {
            walkingController = Resources.Load<RuntimeAnimatorController>("Animations/CharacterWalk");
            walkingControllerLoaded = true;
        }

        if (animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman &&
            walkingController != null)
        {
            animator.applyRootMotion = false;
            animator.runtimeAnimatorController = walkingController;
        }
        else
        {
            Debug.LogWarning($"Character model '{visual.name}' could not be set up with the walking animation.");
        }

        return true;
    }
}
