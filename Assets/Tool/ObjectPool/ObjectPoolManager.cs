using System.Collections.Generic;
using Tool.Single;
using Tool.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tool.ObjectPool
{
    public class PoolInfo
    {
        public Stack<GameObject> PoolStack;
        public int MaxSize;
        public int InitSize;
        public int NowCount => PoolStack.Count;
        public GameObject HandleGo;
    }

    /// <summary>
    /// 对象池只负责生成对象和对象生成在哪个父类，不负责其他
    /// </summary>
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private Dictionary<string, PoolInfo> _poolsDic;
        protected override void OnInit() => _poolsDic = new Dictionary<string, PoolInfo>();

        public GameObject CreateObject(GameObject gameObject, int initSize = 10, int maxSize = 50)
        {
            //如果有池子信息，则从池子中获取元素
            string poolName = gameObject.name.Split('(')[0];
            if (_poolsDic.TryGetValue(poolName, out var poolInfo))
            {
                //池子有元素则返回
                if (poolInfo.PoolStack.Count >= 1)
                {
                    var poolObject = poolInfo.PoolStack.Pop();
                    poolObject.SetActive(true);
                    return poolObject;
                }
                
                //没有则实例化
                var createItem = Object.Instantiate(gameObject,poolInfo.HandleGo.transform);
                createItem.transform.position = Vector3.zero;
                createItem.SetActive(true);
                return createItem;
            }

            //如果没有池子则创建一个池子，然后初始化池子
            _poolsDic.Add(poolName, new PoolInfo()
            {
                PoolStack = new Stack<GameObject>(),
                MaxSize = maxSize,
                InitSize = initSize,
                HandleGo = new GameObject(poolName)
            });

            //初始化池子元素
            PoolInfo newPoolInfo = _poolsDic[poolName];
            for (int i = 0; i < initSize; i++)
            {
                var createItem = Object.Instantiate(gameObject,newPoolInfo.HandleGo.transform);
                createItem.transform.position = Vector3.zero;
                createItem.SetActive(false);
                newPoolInfo.PoolStack.Push(createItem);
            }

            //获取一个元素，执行激活方法，然后返回
            var returnItem = newPoolInfo.PoolStack.Pop();
            returnItem.SetActive(true);
            return returnItem;
        }

        public void ReturnObjectToPool(GameObject gameObject)
        {
            string poolName = gameObject.name.Split('(')[0];

            //判断有无对应池子信息
            if (!_poolsDic.ContainsKey(poolName))
            {
                Object.Destroy(gameObject);
                return;
            }

            //获取对应池子
            PoolInfo getPoolInfo = _poolsDic[poolName];
            //如果当前物品已在池子中，则不再存入
            if (getPoolInfo.PoolStack.Contains(gameObject)) return;
            //对象失火
            gameObject.SetActive(false);
            //判断池子是否已满
            if (getPoolInfo.NowCount < getPoolInfo.MaxSize) getPoolInfo.PoolStack.Push(gameObject);
            else Object.Destroy(gameObject);
        }
    }
}