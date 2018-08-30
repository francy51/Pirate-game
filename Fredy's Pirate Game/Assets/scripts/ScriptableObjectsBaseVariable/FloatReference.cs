using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FloatReference
{

    public FloatVariable Variable;
    public float ConstantValue;
    public bool IsConstant;

    public float Value
    {
        get { return IsConstant ? ConstantValue : Variable.Value; }
    }


}
