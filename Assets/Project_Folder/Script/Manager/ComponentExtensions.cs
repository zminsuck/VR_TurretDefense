// ComponentExtensions.cs
using UnityEngine;

public static class ComponentExtensions
{
    // 자신의 컴포넌트 또는 부모의 컴포넌트를 한 번에 찾기
    public static T FindComponent<T>(this Component source) where T : class
    {
        return source.GetComponent<T>() ?? source.GetComponentInParent<T>();
    }
}