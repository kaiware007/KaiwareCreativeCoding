using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gene {

    public enum GeneCode {
        AngleZ, // 発射角度（Z軸,度）
        Power,  // 発射速度

        Max,
    }

    public float[] gene = null;

    public Gene()
    {
        gene = new float[(int)GeneCode.Max];
    }

    public void RandomParam(float rangeZMin, float rangeZMax, float powerMin, float powerMax)
    {
        gene[(int)GeneCode.AngleZ] = Random.Range(rangeZMin, rangeZMax);
        gene[(int)GeneCode.Power] = Random.Range(powerMin, powerMax);

        Debug.Log("AngleZ " + gene[0] + " Power " + gene[1]);
    }

    public void Copy(ref Gene dist)
    {
        int count = (int)GeneCode.Max;
        for(int i = 0; i < count; i++)
        {
            dist.gene[i] = gene[i];
        }
    }
}
