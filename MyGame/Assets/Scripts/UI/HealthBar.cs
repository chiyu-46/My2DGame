using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private GameObject[] hearts;
    [SerializeField]
    private int maxHealth;
    private void Start()
    {
        hearts = new GameObject[3];
        for (int i = 0; i < maxHealth; i++)
        {
            hearts[i] = transform.GetChild(i).gameObject;
        }
    }

    /// <summary>
    /// 更新生命值显示。
    /// </summary>
    /// <param name="value">当前Player的生命值。</param>
    public void UpdateHealthPoint(int value)
    {
        //当前已设置为显示的心的数量。
        int temp = 0;
        for (int i = 0; i < maxHealth; i++)
        {
            if (temp >= value)
            {
                hearts[i].SetActive(false);
                continue;
            }
            hearts[i].SetActive(true);
            temp++;
        }
    }
}
