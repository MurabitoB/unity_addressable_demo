using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Enums;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

// public class LoadPrefabFromFile
// {
//     public void Load(string filename)
//     {
//         Debug.Log(filename);
//         var loadedObject = Resources.Load(filename);
//         GameObject InstanceObj = GameObject.Instantiate(loadedObject, Vector3.zero, Quaternion.identity) as GameObject;
//     }
// }

public class ResourceLoader : MonoBehaviour
{
    //LoadPrefabFromFile Loader = new LoadPrefabFromFile();
    
    /// <summary>
    /// 素材 Prefab
    /// </summary>
    public AssetReference objectToLoad;
    
    private GameObject instantiatedObject;
    private AsyncOperationHandle<GameObject> objectOperation;
    
    public int MaxAllowProcessingQueueCount = 300;

    public int DemoLoadingCount = 500;

    /// <summary>
    /// 正在讀取的資料
    /// </summary>
    public List<Guid> processingQueue;
    
    /// <summary>
    /// 排隊中的資料
    /// </summary>
    public Dictionary<ResourceLoadOrder, List<AssetReference>> priorityQueue;

    /// <summary>
    /// 讀取中的陣列是否已滿
    /// </summary>
    private bool isProcessingQueueFull;

    /// <summary>
    /// 是否有等待讀取的資料
    /// </summary>
    private bool isOrderQueueEmpty;


    private void LoadObjects(ResourceLoadOrder order, AssetReference asset)
    {
        priorityQueue[order].Add(asset);
    }
    
    public void LoadLowPriorityObjects(AssetReference asset) => LoadObjects(ResourceLoadOrder.Low, asset);

    public void LoadMidPriorityObjects(AssetReference asset) => LoadObjects(ResourceLoadOrder.Medium, asset);

    public void LoadHighPriorityObjects(AssetReference asset) => LoadObjects(ResourceLoadOrder.High, asset);

    private void Awake()
    {
        priorityQueue = new Dictionary<ResourceLoadOrder, List<AssetReference>>();
        priorityQueue[ResourceLoadOrder.Low] = new List<AssetReference>();
        priorityQueue[ResourceLoadOrder.Medium] = new List<AssetReference>();
        priorityQueue[ResourceLoadOrder.High] = new List<AssetReference>();

        processingQueue = new List<Guid>();
    }

    private void Start()
    {
        StartCoroutine(LoadAssetCoroutine());
    }

    private void Update()
    {
        isProcessingQueueFull = processingQueue.Count >= MaxAllowProcessingQueueCount;
        isOrderQueueEmpty = priorityQueue.Any(x => x.Value.Count > 0);
    }

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

    /// <summary>
    /// 演示只有高優先度的資料被讀取
    /// </summary>
    public void DemoOnlyHighOrderAssetsLoad()
    {
        for (int i = 0; i < DemoLoadingCount; i++)
        {
            LoadHighPriorityObjects(objectToLoad);
        }
    }


    public IEnumerator LoadAssetCoroutine()
    {
        while (true)
        {
            // 等待到需要讀取資料的情境，才去讀取資料
            Debug.Log($"isProcessingQueueFull: {isProcessingQueueFull}");
            Debug.Log($"isOrderQueueEmpty: {isOrderQueueEmpty}");

            yield return new WaitUntil(() => !isProcessingQueueFull && !isOrderQueueEmpty);
            
            // 實作讀取資料
            var loadableCount = MaxAllowProcessingQueueCount - processingQueue.Count;
            // 根據排序決定讀取順序 從 priority Queue pop 出資料 
            var queue = PopPriorityQueue(loadableCount);
            // 將進入處理中的資料放入 processing Queue
            AddProcessingQueue(queue);
        }
    }

    private List<AssetReference> PopPriorityQueue(int count)
    {
        var result = new List<AssetReference>();
        // 選擇高優先度的 Queue，選擇最大數量為 count
        // 如果選完之後，還沒達到 count 的值，就繼續往下選
        result.AddRange(priorityQueue[ResourceLoadOrder.High].Take(count));
        result.AddRange(priorityQueue[ResourceLoadOrder.Medium].Take(count - result.Count));
        result.AddRange(priorityQueue[ResourceLoadOrder.Low].Take(count - result.Count));

        // 清除暫存陣列
        foreach (var deleted in result)
        {
            priorityQueue[ResourceLoadOrder.High].Remove(deleted);
            priorityQueue[ResourceLoadOrder.Medium].Remove(deleted);
            priorityQueue[ResourceLoadOrder.Low].Remove(deleted);
        }
        
        return result;
    }

    private void AddProcessingQueue(List<AssetReference> queue)
    {
        var selected = queue.Select(x => new
        {
            Id = Guid.NewGuid(),
            Asset = x
        });
        
        processingQueue.AddRange(selected.Select(x => x.Id));

        foreach (var asset in selected)
        {
            objectOperation = Addressables.LoadAssetAsync<GameObject>(asset.Asset);
            objectOperation.Completed += (obj =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject loadedObject = obj.Result;
                    Debug.Log("Successfully loaded object");
                    instantiatedObject = Instantiate(loadedObject);
                    Debug.Log("Successfully instantiated object");
                }
                processingQueue.Remove(asset.Id);
            });
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