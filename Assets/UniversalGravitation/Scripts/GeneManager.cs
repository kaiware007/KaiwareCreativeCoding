using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniversalGravitation;

public class GeneManager : MonoBehaviour {

    public GameObject UGPrefab;

    /// <summary>
    /// 1世代ごとの個体数
    /// </summary>
    public int unitNum = 100;

    /// <summary>
    /// 次の世代にそのまま残すエリート数(最上位成績の個体）
    /// </summary>
    public int eliteNum = 4;

    /// <summary>
    /// 次の世代に交配させて残す数
    /// </summary>
    public int nextNum = 10;

    /// <summary>
    /// 突然変異確率(0～1)
    /// </summary>
    public float mutationPer = 0.1f;

    /// <summary>
    /// 初期遺伝子パラメータの範囲
    /// </summary>
    public Vector2[] defaultGeneParamRange;

    // シミュレーション空間の配置について
    public int numX = 10;
    public int numY = 10;

    /// <summary>
    /// 世代数
    /// </summary>
    public int generationNum = 0;

    private List<UniversalGravitationManager> ugManagerList = new List<UniversalGravitationManager>();
    private Gene[] nextGenerations;

    private Gene bestGene = new Gene();

    UniversalGravitationManager CreateUGManager(Vector3 position)
    {
        GameObject go = Instantiate(UGPrefab, position, Quaternion.identity);
        UniversalGravitationManager ug = go.GetComponent<UniversalGravitationManager>();
        ug.Initialize();
        return ug;
    }

    // Use this for initialization
    void Start () {
        UniversalGravitationManager ug = UGPrefab.GetComponent<UniversalGravitationManager>();
        Vector3 size = ug.boxSize;

        nextGenerations = new Gene[unitNum];

        float x = 0;
        float y = 0;
        for(int i = 0; i < unitNum; i++)
        {
            x = (i % numX) * size.x;
            y = (i / numX) * size.x;
            ug = CreateUGManager(new Vector3(x, y, 0));

            // 初期化
            ug.rocket.gene.RandomParam(defaultGeneParamRange[0].x, defaultGeneParamRange[0].y, defaultGeneParamRange[1].x, defaultGeneParamRange[1].y);

            ugManagerList.Add(ug);
            nextGenerations[i] = new Gene();
        }

        for (int i = 0; i < unitNum; i++)
        {
            ugManagerList[i].Restart();
        }

    }

    // Update is called once per frame
    void Update () {

        int endCount = 0;

        for (int i = 0; i < unitNum; i++)
        {
            if (ugManagerList[i].isEnd)
            {
                endCount++;
            }
        }

        // すべて終了したか
        if (endCount == ugManagerList.Count)
        {
            // 成績上位をソート
            ugManagerList.Sort(compareGene);
            //for (int i = 0; i < ugManagerList.Count; i++)
            //{
            //    Debug.Log("[" + i + "] distance " + ugManagerList[i].goalDistanceMin);
            //}

            ugManagerList[0].rocket.gene.Copy(ref bestGene);
            
            // 上位数名の遺伝子はそのまま残す
            for (int i = 0; i < eliteNum; i++)
            {
                ugManagerList[i].rocket.gene.Copy(ref nextGenerations[i]);
            }

            // 上位数名の遺伝子を交配させて残りの個体数分を新しい遺伝子を作成する
            int remainCount = unitNum - eliteNum;
            int[] parentIndices = new int[2];
            for (int i = 0; i < remainCount; i++)
            {
                parentIndices[0] = Random.Range(0, nextNum);
                parentIndices[1] = Random.Range(0, nextNum);

                // ランダムにどちらかの値をコピーする
                for (int j = 0; j < (int)Gene.GeneCode.Max; j++)
                {
                    nextGenerations[eliteNum + i].gene[j] = ugManagerList[parentIndices[Random.Range(0, 2)]].rocket.gene.gene[j];

                    // 突然変異
                    if (Random.value <= mutationPer)
                    {
                        //Debug.Log("Mutation!! ugManagerNo " + i + " GeneCode " + ((Gene.GeneCode)j));
                        nextGenerations[eliteNum + i].gene[j] = Random.Range(defaultGeneParamRange[j].x, defaultGeneParamRange[j].y);
                    }
                }
            }

            // 次世代に遺伝子コピー
            for (int i = 0; i < ugManagerList.Count; i++)
            {
                nextGenerations[i].Copy(ref ugManagerList[i].rocket.gene);

                ugManagerList[i].Restart();
            }
            generationNum++;
        }
    }

    int compareGene(UniversalGravitationManager a, UniversalGravitationManager b)
    {
        return (int)Mathf.Sign(a.goalDistanceMin - b.goalDistanceMin);
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Generation " + generationNum);
        for (int i = 0; i < (int)Gene.GeneCode.Max; i++)
        {
            GUILayout.Label("[" + ((Gene.GeneCode)i) + "] " + bestGene.gene[i]);
        }
        GUILayout.EndVertical();
    }
}
