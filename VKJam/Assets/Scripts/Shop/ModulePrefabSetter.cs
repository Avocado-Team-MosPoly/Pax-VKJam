using UnityEngine;
[RequireComponent(typeof(BoxCollider), typeof(Interactable), typeof(DetecterModule))]
public class ModulePrefabSetter : MonoBehaviour
{
    [ContextMenu("Configure and Remove This Component")]
    public void ConfigureAndRemove()
    {
        DetecterModule temp = GetComponent<DetecterModule>();
        if (temp != null)
        {
            temp.Object = gameObject;
            temp.Data = GetComponent<Interactable>();
        }

        if (!Application.isPlaying)
        {
            DestroyImmediate(this, true);
        }
    }
}
