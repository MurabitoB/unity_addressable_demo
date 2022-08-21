using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Enums;
using UnityEngine;

public interface IResourceManager
{
    IEnumerator LoadAsync<T>(string path, ResourceLoadOrder order = ResourceLoadOrder.Low) where T : UnityEngine.Object;
}
