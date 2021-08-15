using System;
using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
/// <summary>
/// 测试类。
/// </summary>
/// <remarks>
/// 已得结论：
/// 1.一个类实例化一个有事件字段的类，可以将自己的private（私有）方法添加到另一个类的事件。并且正常运行（即使涉及自己的私有字段，
/// 效果和直接在本类调用效果相同。）。
/// 2.事件如果为空，直接调用会引发空引用异常。
/// </remarks>
public class Test : MonoBehaviour
{
    private TestA testA = new TestA();
    //private bool sign = false;
    
    private void Start()
    {
        //testA.a = BB;
        Debug.Log("尝试执行测试方法。");
        //testA.a();
        if (!(testA.a is null))
        {
            testA.a();
        }
        Debug.Log("成功执行测试方法。");
        //Debug.Log("当前sign值：" + sign);
        
    }

    private void BB()
    {
        //sign = true;
        Debug.Log("成功执行测试方法。");
    }

}
