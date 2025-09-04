// FXManager.cs
using UnityEngine;

public static class FXManager
{
    // 파티클 이펙트 생성 및 자동 파괴
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
            Object.Destroy(instance, 3f); // 파티클 시스템이 없으면 3초 뒤 파괴
        }
    }

    // 사운드 재생 (AudioSource가 있거나 없을 때 모두 처리)
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