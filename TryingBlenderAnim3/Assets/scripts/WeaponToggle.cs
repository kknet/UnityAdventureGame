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

    void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        inputController = GetComponent<InputController>();
        myAnimator = GetComponent<Animator>();
        scimitarOut.SetActive(false);
        weaponOut = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && weaponOut)
            StartSheath();
        
        else if (Input.GetKeyDown(KeyCode.Alpha1) && !weaponOut)
            StartDraw();
    }

    public void StartSheath()
    {
        weaponOut = false;
        myAnimator.SetBool("Sheathing", true);
        Sheath.PlayDelayed(0.3f);
    }
    public void FinishSheath()
    {
        scimitarOut.SetActive(false);
        scimitarIn.SetActive(true);
        myAnimator.SetBool("Sheathing", false);
        myAnimator.SetBool("WeaponDrawn", false);
    }

    void StartDraw()
    {
        weaponOut = true;
        Unsheath.PlayDelayed(0.3f);
        myAnimator.SetBool("Drawing", true);
    }

    public void FinishDrawing()
    {
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
