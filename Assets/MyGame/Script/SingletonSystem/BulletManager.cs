using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Pool;


namespace MyGame.Script.SingletonSystem
{
    //TODO オブジェクトプールがオンライン上で同期できていない
    //TODO オブジェクトプールからゲットした弾の奪い合いが起きている。
    public class BulletManager : MonoBehaviourPunCallbacks , IActivatable
    {
        [SerializeField] private List<GameObject> _bulletList = new();
        [SerializeField] private Transform _bulletParentTransform;
        private static int _bulletID = 0;
        public static BulletManager Instance;
        private readonly Dictionary<int , ObjectPool<GameObject>> _objectPools = new ();
        private readonly Dictionary<int, GameObject> _bulletIDReference = new();

        #region MasterOnly

        public void CallReleaseBullet(BulletType bulletType ,int bulletID)
        {
            photonView.RPC(nameof(ReleaseBullet), RpcTarget.AllViaServer, bulletType, bulletID);
        }
        public void CallMadeBullet(BulletType bulletType , Vector3 position , Quaternion rotation )
        {
            _bulletID += 1;
            Debug.Log("CallMadeBullet" + _bulletID);
            photonView.RPC(nameof(MadeBullet), RpcTarget.AllViaServer , bulletType,position, rotation , _bulletID);
        }

        #endregion

        #region synchronize
        
        [PunRPC]
        public void ReleaseBullet(BulletType bulletType ,int bulletID)
        {
            GameObject bullet = _bulletIDReference[bulletID];
            bullet.GetComponent<BulletController>().Release();
            //_objectPools[(int)bulletType].Release(bullet);
        }
        
        [PunRPC]
        public void MadeBullet(BulletType bulletType , Vector3 position , Quaternion rotation , int bulletID )
        {
            GameObject obj = Instantiate(_bulletList[(int)bulletType], _bulletParentTransform);
            obj.GetComponent<BulletController>().Initialize(position, rotation , bulletID);
            _bulletIDReference.Add(bulletID , obj);
            return;
            
            
            // int bulletIndex = (int)bulletType;
            // GameObject obj;
            // if (_objectPools.TryGetValue(bulletIndex, out var value))
            // {
            //     value.Get(out obj);
            // }
            // else
            // {
            //     _objectPools.Add(bulletIndex, InitializeObjectPool(bulletIndex));
            //     _objectPools[bulletIndex].Get(out obj);
            // }
            // //弾の初期化
            // obj.GetComponent<BulletController>().Initialize(position, rotation , bulletID);
            // _bulletIDReference.Add(bulletID , obj);
            
        }

        #endregion



        private ObjectPool<GameObject> InitializeObjectPool(int bulletIndex)
        {
           
            var obstaclePool = new ObjectPool<GameObject>(
                () => Instantiate(_bulletList[bulletIndex], _bulletParentTransform), // プールが空のときに新しいインスタンスを生成する処理
                target => { target.SetActive(true); }, // プールから取り出されたときの処理 
                target => { target.SetActive(false); },
                target => { Destroy(target); } ,
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
        public override void OnEnable()  
        {
            base.OnEnable();
            MyServiceLocator.IRegister<IActivatable>(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            MyServiceLocator.IUnRegister<IActivatable>(this);
        }
        

        public void Active()
        {
            _bulletIDReference.Clear();
            _bulletID = 0;
        }

        public void DeActive()
        {
            foreach (Transform bullet in _bulletParentTransform)
            {
                Debug.Log("弾を戻しました");
                bullet.GetComponent<BulletController>().Release();
                //_objectPools[(int)bullet.GetComponent<BulletController>().BulletType].Release(bullet.gameObject);
                    
            }

        }
    }
}