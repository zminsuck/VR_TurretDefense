// FXManager.cs
using UnityEngine;

public static class FXManager
{
    // ��ƼŬ ����Ʈ ���� �� �ڵ� �ı�
    public static void PlayEffect(GameObject vfxPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!vfxPrefab) return;

        var instance = Object.Instantiate(vfxPrefab, position, rotation, parent);
        if (instance.TryGetComponent<ParticleSystem>(out var ps))
        {
            Object.Destroy(instance, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Object.Destroy(instance, 3f); // ��ƼŬ �ý����� ������ 3�� �� �ı�
        }
    }

    // ���� ��� (AudioSource�� �ְų� ���� �� ��� ó��)
    public static void PlaySound(AudioClip clip, Vector3 position, AudioSource source = null, float volume = 1f)
    {
        if (!clip) return;

        if (source)
        {
            source.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}