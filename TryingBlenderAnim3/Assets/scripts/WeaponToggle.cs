using System.Collections;
using System.Collections.Generic;
//sheathing rot: -132, -701, -104, sheathing trans:
//attacking rot: -113, -539, -271, attacking trans: -0.358, 0.108, 0.003

using UnityEngine;

public class WeaponToggle : MonoBehaviour
{

    public Animator myAnimator;
    public AudioSource Sheath;
    public AudioSource Unsheath;

    private string weaponOut;
    private string newWeaponOut;
    GameObject[] allWeps;
    Dictionary<string, GameObject> weaponsTable;
    GameObject ShieldIn;
    GameObject ShieldOut;

    //	private Vector3 scimInRot = new Vector3(230.35f, -97.044f, -144.8f);
    //	private Vector3 scimInTrans = new Vector3(0.288f, -0.174f, -0.405f);
    //		
    //	private Vector3 scimOutTrans = new Vector3(-1.0f, - 0.045f, -0.24f); 
    //	private Vector3 scimOutRot = new Vector3(78.8f, - 236.2f, 48.31f); 

    //scimOut:
    // transform: -1 -0.045 -0.24
    // rotation: 78.8 -236.2 48.31

    //	bool isShieldOut;

    // Use this for initialization
    void Start()
    {
        weaponsTable = new Dictionary<string, GameObject>();
        allWeps = GameObject.FindGameObjectsWithTag("OurWeapons");
        myAnimator = DevMain.Player.GetComponent<Animator>();
        //		ShieldIn = GameObject.Find ("ShieldIn");
        ShieldOut = GameObject.Find("ShieldOut");
        //		ShieldIn.SetActive (true);
        ShieldOut.SetActive(false);
        weaponOut = "";
        initTable();
        setOutInactive();
        //		isShieldOut = false;
    }

    void initTable()
    {
        foreach (GameObject g in allWeps)
        {
            weaponsTable.Add(g.name, g);
        }
        //		if(weaponOut!="")
        //			weaponsTable[weaponOut + "In"].SetActive(false);
    }

    // Update is called once per frame

    //scimIn:
    // transform: 0.288 -0.174 -0.405
    // rotation: 230.35 -97.044 -144.8

    //scimOut:
    // transform: -1 -0.045 -0.24
    // rotation: 78.8 -236.2 48.31

    public void drawScim()
    {
        if (weaponOut == "Scimitar")
        {
            return;
        }

        //a weapon is already equipped (not a scimitar)
        if (weaponOut != "")
        {
            newWeaponOut = "Scimitar";
            StartSwitch();

            //no weapon is already equipped
        }

        else
        {
            StartShieldDraw();
            weaponOut = "Scimitar";
            Invoke("StartDraw", 1.0f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (weaponOut != "")
            {
                StartSheath();
                //				Invoke ("StartShieldSheath", 0.5f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            drawScim();
        }
        //		else if (Input.GetKeyDown(KeyCode.Alpha2)) {	
        //			if (weaponOut == "Spear"){
        //				return;
        //			}
        //
        //			//a weapon is already equipped (not a scimitar)
        //			if (weaponOut!="") {
        //				newWeaponOut = "Spear";
        //				StartSwitch ();
        //
        //			//no weapon is already equipped
        //			} else {
        //				StartShieldDraw ();
        //				weaponOut = "Spear";
        //				Invoke ("StartDraw", 0.5f);
        //			}
        //		}
    }

    void StartSwitch()
    {
        myAnimator.SetBool("SwitchingWeps", true);
        Sheath.PlayDelayed(0.2f);
        myAnimator.SetBool("Drawing", true);
        Unsheath.PlayDelayed(0.3f);
        myAnimator.SetBool("WeaponDrawn", true);
    }


    void StartShieldSheath()
    {
        if (weaponOut != "")
        {
            myAnimator.SetBool("ShieldDraw", false);
            myAnimator.SetBool("ShieldSheath", true);
        }
    }

    void FinishShieldSheath()
    {
        //		ShieldIn.SetActive (true);
        ShieldOut.SetActive(false);
        //		isShieldOut = false;
    }

    void StartShieldDraw()
    {
        if (weaponOut == "")
        {
            myAnimator.SetBool("ShieldSheath", false);
            myAnimator.SetBool("ShieldDraw", true);
        }
    }

    void FinishShieldDraw()
    {
        //		ShieldIn.SetActive (false);
        ShieldOut.SetActive(true);
        //		isShieldOut = true;
    }

    public void StartSheath()
    {
        if (weaponOut != "")
        {
            myAnimator.SetBool("Sheathing", true);
            Sheath.PlayDelayed(0.3f);
        }
    }
    public void FinishSheath()
    {
        weaponsTable[weaponOut + "Out"].SetActive(false);
        weaponsTable[weaponOut + "In"].SetActive(true);
        StartShieldSheath();
        //		ShieldIn.SetActive (true);
        //		ShieldOut.SetActive (false);

        if (myAnimator.GetBool("SwitchingWeps"))
        {
            weaponOut = newWeaponOut;
        }
        else
        {
            weaponOut = "";
        }
        newWeaponOut = "";
        myAnimator.SetBool("Sheathing", false);

        if (myAnimator.GetBool("SwitchingWeps"))
        {
            myAnimator.SetBool("WeaponDrawn", true);
        }
        else
        {
            myAnimator.SetBool("WeaponDrawn", false);
        }
    }

    //assumes that previous weapon (if any) has been sheathed
    void StartDraw()
    {
        if (weaponOut == "")
        {
            Debug.LogAssertion("In StartDraw but weaponOut is blank");
            return;
        }
        Unsheath.PlayDelayed(0.3f);
        myAnimator.SetBool("Drawing", true);
    }

    public void FinishDrawing()
    {
        weaponsTable[weaponOut + "Out"].SetActive(true);
        weaponsTable[weaponOut + "In"].SetActive(false);
        //		ShieldIn.SetActive (false);
        //		ShieldOut.SetActive (true);
        myAnimator.SetBool("Drawing", false);
        myAnimator.SetBool("SwitchingWeps", false);
        myAnimator.SetBool("WeaponDrawn", true);
    }

    void setOutInactive()
    {
        foreach (GameObject g in allWeps)
        {
            if (g.name.Contains("Out"))
                g.SetActive(false);
            else
                g.SetActive(true);
        }
    }
}
