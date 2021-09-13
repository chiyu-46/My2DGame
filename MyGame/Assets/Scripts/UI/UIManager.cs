using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject healthBar;
    [SerializeField]
    private GameObject phonePanel;
    [SerializeField]
    private GameObject pauseUI;
    [SerializeField]
    private GameObject loadingUI;

    public HealthBar HealthBar { get => healthBar.GetComponent<HealthBar>(); }
    public GameObject PhonePanel { get => phonePanel; }
    public GameObject PauseUI { get => pauseUI; }
    public GameObject LoadingUI { get => loadingUI; }
}
