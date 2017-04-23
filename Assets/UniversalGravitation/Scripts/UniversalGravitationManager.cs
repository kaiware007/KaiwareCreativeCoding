using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace UniversalGravitation
{
    public class UniversalGravitationManager : MonoBehaviour
    {
        #region public
        public float gravityPower = 6.67e-11f;    // 重力係数

        public List<MassObject> planetList;
        public Rocket rocket;
        public MassObject goalPlanet;

        public float deltaTime = 0.05f;
        public int iterationCount = 120;
        public bool isPlay = false;

        public Vector3 boxSize;
        public Vector3 boxCenter;

        public float goalDistanceMin = 0;   // ゴールまでの距離（最近点）

        public int positionHistoryUpdateIntervalFrame = 5;

        public LineRenderer line;
        #endregion

        #region private
        //private List<Vector3> positionHistory = new List<Vector3>();
        private Vector3[] positionHistory = null;
        private int positionHistoryCount = 0;

        private Thread thread = null;
        private int simulationCount = 0;

        [HideInInspector]
        public bool isEnd = false;
        private Vector3 simulationPosition;
        
        //bool isThreadPlay = false;
        #endregion

        private void Awake()
        {
            simulationPosition = transform.position;
        }

        // Use this for initialization
        //void Start()
        //{
        //    Initialize();
        //    Restart();
        //}

        public void Initialize()
        {
            positionHistory = new Vector3[iterationCount / positionHistoryUpdateIntervalFrame];
        }

        public void Restart()
        {
            AbortThread();
            thread = new Thread(UpdateSimulation);

            positionHistoryCount = 0;
            rocket.ResetParam();
            rocket.UpdateParam();
            for(int i = 0; i < planetList.Count; i++)
            {
                planetList[i].ResetParam();
            }

            simulationCount = 0;
            isPlay = true;
            isEnd = false;
            goalDistanceMin = float.MaxValue;

            //isThreadPlay = false;

            //Debug.Log("Restart");

            //Debug.Log("thread.State1 " + thread.ThreadState);

            thread.Start();

            //Debug.Log("thread.State2 " + thread.ThreadState);
        }

        void UpdateSimulation()
        {
            Vector3 rangeMax = simulationPosition + boxCenter + boxSize / 2f;
            Vector3 rangeMin = simulationPosition + boxCenter - boxSize / 2f;

            //isThreadPlay = true;

            //simulationCount = 1;

            //Vector3 diff;

            //isPlay = true;
            while (isPlay)
            //while (true)
            {
                int i;
                //int loop = 0;
                for (i = 0; i < iterationCount; i++)
                {
                    rocket.CalcUniversalGravitation(planetList, gravityPower);
                    rocket.UpdatePosition(deltaTime);

                    // ゴールから最も近い距離を保持
                    float dist = Vector3.Distance(rocket.positionCache, goalPlanet.positionCache);
                    goalDistanceMin = Mathf.Min(dist, goalDistanceMin);

                    for (int j = 0; j < planetList.Count; j++)
                    {
                        // 惑星にぶつかったら終了
                        if (rocket.IsHit(planetList[j]))
                        {
                            isPlay = false;
                            break;
                        }
                    }

                    // シミュレーション範囲からはみ出しても終了
                    if ((rocket.positionCache.x < rangeMin.x) ||
                        (rocket.positionCache.y < rangeMin.y) ||
                        (rocket.positionCache.z < rangeMin.z) ||
                        (rocket.positionCache.x > rangeMax.x) ||
                        (rocket.positionCache.y > rangeMax.y) ||
                        (rocket.positionCache.z > rangeMax.z))
                    {
                        isPlay = false;
                    }

                    // 座標履歴追加
                    //positionHistory.Add(rocket.positionCache);
                    if (((i % positionHistoryUpdateIntervalFrame) == 0 )&&(positionHistory.Length > positionHistoryCount))
                    {
                        positionHistory[positionHistoryCount] = rocket.positionCache;
                        positionHistoryCount++;
                    }

                    //loop++;

                    if (!isPlay)
                    {
                        break;
                    }

                }

                //simulationCount = 2;
                simulationCount = i;
                //Debug.Log("iterationCount " + i);
                isPlay = false;
                //Thread.Sleep(0);
            }

            //simulationCount = 3;
            //isThreadPlay = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!thread.IsAlive && !isEnd && !isPlay)
            //if(!isPlay && !isEnd)
            {
                //Debug.Log("thread.State3 " + thread.ThreadState);
                // 結果表示
                //Debug.Log("simulationCount " + simulationCount + " goalDistanceMin " + goalDistanceMin);
                line.positionCount = positionHistoryCount;
                line.SetPositions(positionHistory);
                isEnd = true;
            }
        }

        void AbortThread()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        private void OnApplicationQuit()
        {
            AbortThread();
        }

        private void OnDestroy()
        {
            AbortThread();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + boxCenter, boxSize);
        }
    }
}   // namespace