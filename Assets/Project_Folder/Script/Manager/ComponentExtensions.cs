// ComponentExtensions.cs
using UnityEngine;

public static class ComponentExtensions
{
    // �ڽ��� ������Ʈ �Ǵ� �θ��� ������Ʈ�� �� ���� ã��
    public static T FindComponent<T>(this Component source) where T : class
    {
        return source.GetComponent<T>() ?? source.GetComponentInParent<T>();
    }
}