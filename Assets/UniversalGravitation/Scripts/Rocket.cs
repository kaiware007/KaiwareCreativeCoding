using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalGravitation
{
    public class Rocket : MassObject
    {
        /// <summary>
        /// 遺伝子
        /// </summary>
        public Gene gene = new Gene();

        /// <summary>
        /// 遺伝子情報をロケットのパラメータに反映させる
        /// </summary>
        public void UpdateParam()
        {
            float angleZ = gene.gene[(int)Gene.GeneCode.AngleZ];
            float power = gene.gene[(int)Gene.GeneCode.Power];
            Vector3 angle = Vector3.zero;
            angle.x = Mathf.Cos(angleZ);
            angle.y = Mathf.Sin(angleZ);
            angle.z = 0;
            angle.Normalize();
            velocity = angle * power;
        }
    }
}
