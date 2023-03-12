using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDetection : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Animator _rotationAnimator;
    [SerializeField] private Animator _playerAnimator;

    public LayerMask mask;

    public GameObject player;
    public GameObject mousePos;

    public GameObject cube;

    public bool canAttack = true;
    public bool isAttacking;

    public int attackIndex = 0;

    private void Update()
    {
        /*
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = transform.parent.position.z;
        cube.transform.position = mouseWorldPosition;
        */

        //Debug.DrawRay(transform.position, mouseWorldPosition - transform.position, Color.red);

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
       
        if (isAttacking && !isPlaying(_playerAnimator, "attack2") && !isPlaying(_playerAnimator, "attack1"))
        {
            print("caca");
            ResetAttak();
        }
        else if (isAttacking && !isPlaying(_playerAnimator, "attack1") && !_playerAnimator.GetBool("Attack2"))
        {
            ResetAttak();
        }

        if (Input.GetMouseButtonDown(0) && attackIndex == 0)
        {
            if(Physics.Raycast(ray, out hit, 100, mask) && canAttack && !isAttacking)
            {
                canAttack = false;
                isAttacking = true;
                PlayerController.canMove = false;
                _rotationAnimator.enabled = false;

                attackIndex++;

                _playerAnimator.Play("attack1");

                cube.transform.position = hit.point;

                mousePos.transform.LookAt(cube.transform.position, Vector3.up);

                player.transform.eulerAngles = Quaternion.Euler(0, mousePos.transform.eulerAngles.y, 0).eulerAngles;
            }
        }
        else if (Input.GetMouseButtonDown(0) && attackIndex > 0)
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
