using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FoxForm : AbstractPlayer
{
    // Customizeable Variables
    [Header("Fox Variables")]
    [SerializeField]
    private GameObject spiritBallPrefab;
    [SerializeField]
    private Vector3 spiritSlotHeight;
    [SerializeField]
    private Vector3 spiritSlotYOffset;
    [SerializeField]
    private Vector3 spiritSlotXOffset;
    [Range(0.5f, 5)]
    public float spiritRespawnTime = 1;

    // Reference Variables
    // TODO: Fix spirit ball positions, relative to local space, and fix attack conditions
    private Vector3 pos
    {
        get { return transform.position; }
    }
    private Vector3 spiritSlotBase
    {
        get { return pos + transform.TransformDirection(spiritSlotHeight); }
    }
    private Vector3 spiritSlotLeft
    {
        get { return spiritSlotBase - transform.TransformDirection(2 * spiritSlotYOffset - spiritSlotXOffset); }
    }
    private Vector3 spiritSlotRight
    {
        get { return spiritSlotBase - transform.TransformDirection(2 * spiritSlotYOffset + spiritSlotXOffset); }
    }

    // Object Variables
    private List<GameObject> spiritBalls = new List<GameObject>();
    private bool attack;

    protected override void Update()
    {
        base.Update();

        if (spiritBalls.Count > 0 && Input.GetButtonDown("Attack"))
            ShootSpiritBall();
    }

    protected override void ToggleWorlds()
    {
        base.ToggleWorlds();

        for (int i = 0; i < spiritBalls.Count; i++)
            Destroy(spiritBalls[i].gameObject);

        spiritBalls.Clear();
    }

    private void OnEnable()
    {
        AddSpiritBall();
        AddSpiritBall();
        AddSpiritBall();
    }

    private void AddSpiritBall()
    {
        GameObject spiritBall = Instantiate(spiritBallPrefab);
        spiritBalls.Add(spiritBall);

        int i = spiritBalls.IndexOf(spiritBall);
        spiritBalls[i].transform.parent = transform;
        spiritBalls[i].transform.rotation = transform.rotation;

        RepositionSpiritBalls();
    }

    private void RepositionSpiritBalls()
    {
        Vector3 pos = transform.position;
        switch (spiritBalls.Count)
        {
            case 1:
                spiritBalls[0].transform.position = spiritSlotBase;
                break;
            case 2:
                spiritBalls[0].transform.position = spiritSlotLeft + spiritSlotYOffset;
                spiritBalls[1].transform.position = spiritSlotRight + spiritSlotYOffset;
                break;
            case 3:
                spiritBalls[0].transform.position = spiritSlotLeft;
                spiritBalls[1].transform.position = spiritSlotRight;
                spiritBalls[2].transform.position = spiritSlotBase;
                break;
        }
    }

    private void ShootSpiritBall()
    {
        GameObject shooterBall = spiritBalls[spiritBalls.Count - 1];
        spiritBalls.Remove(shooterBall);
        shooterBall.SendMessage("Shoot");

        RepositionSpiritBalls();
        StartCoroutine(RespawnSpiritBall());
    }

    private IEnumerator RespawnSpiritBall()
    {
        yield return new WaitForSeconds(spiritRespawnTime);

        AddSpiritBall();
    }
}