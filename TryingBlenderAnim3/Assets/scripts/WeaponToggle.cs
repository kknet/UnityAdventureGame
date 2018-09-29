using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponToggle : MonoBehaviour
{
    InputController inputController;
    CameraController cameraController;

    [HideInInspector] public Animator myAnimator;
    public AudioSource Sheath;
    public AudioSource Unsheath;
    public GameObject scimitarIn, scimitarOut;
    bool weaponOut;

    public bool disabled;

    void Awake()
    {
        if (disabled) return;
        cameraController = Camera.main.GetComponent<CameraController>();
        inputController = GetComponent<InputController>();
        myAnimator = GetComponent<Animator>();
        scimitarOut.SetActive(false);
        weaponOut = false;
        Debug.Log("got here");
    }

    void Update()
    {
        if (disabled) return;
        if (Input.GetKeyDown(KeyCode.C) && weaponOut)
            StartSheath();
        
        else if (Input.GetKeyDown(KeyCode.Alpha1) && !weaponOut)
            StartDraw();
    }

    public void StartSheath()
    {
        if (disabled) return;
        weaponOut = false;
        myAnimator.SetBool("Sheathing", true);
        Sheath.PlayDelayed(0.3f);
    }
    public void FinishSheath()
    {
        if (disabled) return;
        scimitarOut.SetActive(false);
        scimitarIn.SetActive(true);
        myAnimator.SetBool("Sheathing", false);
        myAnimator.SetBool("WeaponDrawn", false);
    }

    void StartDraw()
    {
        if (disabled) return;
        weaponOut = true;
        Unsheath.PlayDelayed(0.3f);
        myAnimator.SetBool("Drawing", true);
    }

    public void FinishDrawing()
    {
        if (disabled) return;
        scimitarOut.SetActive(true);
        scimitarIn.SetActive(false);
        myAnimator.SetBool("Drawing", false);
        myAnimator.SetBool("WeaponDrawn", true);
    }

    //IEnumerator WaitForCombatStart()
    //{
    //    inputController.DisableInput();
    //    yield return new WaitForSeconds(2f);
    //    inputController.EnableInput();
    //}
}
