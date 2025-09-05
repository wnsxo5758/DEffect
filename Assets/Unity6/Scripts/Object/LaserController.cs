using UnityEngine;

public enum LaserDirection { Down = 0, Up, Right, Left } // 레이저 방향

public class LaserController : MonoBehaviour,ISwitchable
{
    [Header("레이저 설정")]
    [SerializeField]
    private Transform laserPos; //레이저가 발사되는 위치
    [SerializeField]
    private LaserDirection dir; // 레이저 방향

    private bool isActive;

    public bool IsActive => isActive;

    private void OnEnable() // 시작시 설정
    {
        if (isActive) ActiveLaser();
        else DeactiveLaser();

        ApplyRotationByDirection();
    }

    private void Update()
    {

    }


    public void Active() // 활성화시
    {
        ActiveLaser();
    }

    public void Deactive() //비활성화시
    {
        DeactiveLaser();
    }

    private void ActiveLaser() // 레이저 활성화
    {
        isActive = true;
    }

    private void DeactiveLaser() // 레이저 비활성화
    {
        isActive = false;
    }

    private void ApplyRotationByDirection()
    {
        float zRotation = dir switch
        {
            LaserDirection.Up => 180f,
            LaserDirection.Down => 0f,
            LaserDirection.Left => -90f,
            LaserDirection.Right => 90f,
            _ => 0f
        };

        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }

}
