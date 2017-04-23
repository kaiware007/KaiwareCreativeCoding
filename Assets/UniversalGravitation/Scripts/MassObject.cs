using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalGravitation
{

    public class MassObject : MonoBehaviour
    {
        public float mass;              // 質量
        public Vector3 velocity;        // 加速度
        public float radius;            // 大きさ（半径）

        [HideInInspector]
        public Vector3 positionCache;
        private Vector3 defaultPosition;
        private Vector3 defaultVelocity;

        void Awake()
        {
            positionCache = transform.position;
            defaultPosition = transform.position;
            defaultVelocity = velocity;
            //Debug.Log("position " + positionCache);
        }

        /// <summary>
        /// パラメータのリセット（初期値に戻す）
        /// </summary>
        public void ResetParam()
        {
            positionCache = defaultPosition;
            velocity = defaultVelocity;
        }

        /// <summary>
        /// 万有引力の計算
        /// </summary>
        /// <param name="list"></param>
        public void CalcUniversalGravitation(List<MassObject> list, float gravityPower)
        {

            Vector3 pos = positionCache;

            for(int i = 0; i < list.Count; i++)
            {
                //Vector3 direction = list[i].transform.position - pos;
                Vector3 direction = list[i].positionCache - pos;
                float distance = direction.magnitude;
                distance *= distance;

                float g = gravityPower * mass * list[i].mass / distance;

                velocity += g * direction.normalized;
            }
                
        }

        /// <summary>
        /// 座標の更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdatePosition(float deltaTime)
        {
            //transform.position += velocity * deltaTime;
            positionCache += velocity * deltaTime;
        }

        public bool IsHit(MassObject obj)
        {
            //Vector3 p = transform.position - obj.transform.position;
            //Vector3 p = positionCache - obj.positionCache;
            float r = radius + obj.radius;

            //return (p.sqrMagnitude <= (r * r));
            return Vector3.Distance(positionCache, obj.positionCache) <= r;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(positionCache, radius);
        }
    }

}