using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [SerializeField] private Camera cursorCam;

    [Header("Animators \n")]
    [SerializeField] private Animator _rotationAnimator;
    [SerializeField] private Animator _playerAnimator;


    public GameObject player;

    [Header("Cursor Detection \n")]
    public LayerMask mask;

    public GameObject mousePos;
    public GameObject mousePoint;

    private bool canAttack = true;
    private bool isAttacking;

    private int attackIndex = 0;

    private void Update()
    {
        /*
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = transform.parent.position.z;
        cube.transform.position = mouseWorldPosition;
        */

        //Debug.DrawRay(transform.position, mouseWorldPosition - transform.position, Color.red);

        Ray ray = cursorCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
       
        
        
        if (isAttacking && !_playerAnimator.IsInTransition(0) && !isPlaying(_playerAnimator, "attack1") && !_playerAnimator.GetBool("Attack2"))
        {
            print("caca2");
            ResetAttak();
        }
        else if(isAttacking && !_playerAnimator.IsInTransition(0) && !isPlaying(_playerAnimator, "attack2") && !isPlaying(_playerAnimator, "attack1"))
        {
            print("caca");
            ResetAttak();
        }

        if (Input.GetMouseButtonDown(0) && attackIndex == 0 && !isPlaying(_playerAnimator, "attack1"))
        {
            if(Physics.Raycast(ray, out hit, 100, mask) && canAttack && !isAttacking)
            {
                canAttack = false;
                isAttacking = true;
                PlayerController.canMove = false;
                _rotationAnimator.enabled = false;

                attackIndex++;

                _playerAnimator.Play("attack1");

                mousePoint.transform.position = hit.point;

                mousePos.transform.LookAt(mousePoint.transform.position, Vector3.up);

                player.transform.eulerAngles = Quaternion.Euler(0, mousePos.transform.eulerAngles.y, 0).eulerAngles;
            }
        }
        else if (Input.GetMouseButtonDown(0) && attackIndex > 0 && !isPlaying(_playerAnimator, "attack2"))
        {
            canAttack = false;
            isAttacking = true;
            PlayerController.canMove = false;
            _rotationAnimator.enabled = false;

            attackIndex++;

            _playerAnimator.SetBool("Attack2", true);
        }        
    }

    bool isPlaying(Animator anim, string stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void ResetAttak()
    {
        _playerAnimator.SetBool("Attack2", false);
        canAttack = true;
        isAttacking = false;
        attackIndex = 0;
        _rotationAnimator.enabled = true;
        PlayerController.canMove = true;
    }
}
