using NUnit;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Elevator : MonoBehaviour,ISwitchable
{
    [SerializeField]
    private Vector2 movePos;// 움직일 위치
    [SerializeField]
    private float moveDuration = 2f; // 이동하는데 걸리는 시간(기본값 : 2초)
    private bool isActive = false; // 작동했는가?
    public bool IsActive => isActive;


    private bool isMoving; // 움직이고 있는 중인가

    private Vector2 startPos; // 시작위치(엘리베이터 초기 위치)

    private Animator animator2D;
    private void Awake()
    {
        animator2D = GetComponentInChildren<Animator>();
    }
    private void OnEnable()
    {
        startPos = transform.position;// 초기 위치 설정
    }
    public void Active() // 위로 올라가기
    {
        if (isMoving) return;
        isActive = true;
        StartCoroutine(MoveTo(startPos, movePos));
    }

    public void Deactive() // 내려가기
    {
        if (isMoving) return;
        isActive = false;
        StartCoroutine(MoveTo(movePos, startPos));
    }

    private IEnumerator MoveTo(Vector2 start , Vector2 end)
    {
        isMoving = true;
        if(animator2D != null)
        {
            animator2D.SetBool("IsMoving", true); // 애니메이터가 있다면 움직이는 애니메이션으로 변경
        }
        float elapsedTime = 0f;
        while(elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            Debug.Log($"t 의 값 : {t}");
            transform.position = Vector2.Lerp(start, end, t);
            yield return null; // 다음 프레임까지 대기
        }
        transform.position = end;

        if (animator2D != null) 
        { 
            animator2D.SetBool("IsMoving", false); 
        }
        isMoving = false;
    }
}
