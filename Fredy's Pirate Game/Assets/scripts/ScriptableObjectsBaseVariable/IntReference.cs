using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntReference : MonoBehaviour
{

    public IntVariable Variable;
    public int ConstantValue;
    public bool IsConstant;

    public int Value
    {
        get { return IsConstant ? ConstantValue : Variable.Value; }
    }

}
