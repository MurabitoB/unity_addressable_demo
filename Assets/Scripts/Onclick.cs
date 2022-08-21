using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class LoadPrefabFromFile
{
    public void Load(string filename)
    {
        Debug.Log(filename);
        var loadedObject = Resources.Load(filename);
        GameObject InstanceObj = GameObject.Instantiate(loadedObject, Vector3.zero, Quaternion.identity) as GameObject;
    }
}

public class Onclick : MonoBehaviour
{
    //LoadPrefabFromFile Loader = new LoadPrefabFromFile();
    public AssetReference objectToLoad;
    private GameObject instantiatedObject;
    private AsyncOperationHandle<GameObject> objectOperation;

    public void Click()
    {

        for (int i = 0; i < 100; i++)
        {
            objectOperation = Addressables.LoadAssetAsync<GameObject>(objectToLoad);
            objectOperation.Completed += ObjectLoadDone;
            pause();
            Debug.Log(i);
        }
    }
    
    public IEnumerator pause()
    {
        yield return new WaitForSeconds(1f);
    }

    public void RoutineWrap()
    {
        StartCoroutine(pause());
    }

    private void ObjectLoadDone(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            //prefab_count++;
            GameObject loadedObject = obj.Result;
            Debug.Log("Successfully loaded object");
            instantiatedObject = Instantiate(loadedObject);
            Debug.Log("Successfully instantiated object");
        }
    }
}