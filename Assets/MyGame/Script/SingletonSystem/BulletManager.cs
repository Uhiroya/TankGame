using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Pool;

public enum BulletType
{
    Normal = 0,
    Speed = 1,
}
namespace MyGame.Script.SingletonSystem
{
    public class BulletManager : MonoBehaviourPunCallbacks , IActivatable
    {
        [SerializeField] private List<GameObject> _bulletList = new();
        public static BulletManager Instance;
        private Dictionary<int , ObjectPool<GameObject>> _objectPools = new ();
        private Dictionary<int, GameObject> _bulletIDReference = new();
        
        [PunRPC]
        public void ReleaseBullet(BulletType bulletType ,int bulletID)
        {
            _objectPools[(int)bulletType].Release(_bulletIDReference[bulletID]);
        }
        
        
        public void CallMadeBullet(BulletType bulletType , Vector3 position , Quaternion rotation )
        {
            photonView.RPC(nameof(MadeBullet), RpcTarget.All ,bulletType,position,rotation);
        }

        [PunRPC]
        public void MadeBullet(BulletType bulletType , Vector3 position , Quaternion rotation )
        {
            int bulletIndex = (int)bulletType;
            GameObject obj;
            if (_objectPools.TryGetValue(bulletIndex, out var value))
            {
                value.Get(out obj);
            }
            else
            {
                _objectPools.Add(bulletIndex, InitializeObjectPool(bulletIndex));
                _objectPools[bulletIndex].Get(out obj);
            }

            var bulletController = obj.GetComponent<BulletController>();
            _bulletIDReference.Add(bulletController.Initialize(position , rotation) , obj);
        }
        private ObjectPool<GameObject> InitializeObjectPool(int bulletIndex)
        {
            var obstaclePool = new ObjectPool<GameObject>(
                () => Instantiate(_bulletList[bulletIndex], transform), // プールが空のときに新しいインスタンスを生成する処理
                target => { target.SetActive(true); }, // プールから取り出されたときの処理 
                target => { target.SetActive(false); },
                target =>
                {
                    PhotonNetwork.Destroy(target);
                } ,
                false
            );
            //一つ目を作っておかないと初回生成が重い
            obstaclePool.Get(out var obj);
            obstaclePool.Release(obj);
            return obstaclePool;
        }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        
        

        public void Active()
        {
            
        }

        public void DeActive()
        {
            
        }
    }
}