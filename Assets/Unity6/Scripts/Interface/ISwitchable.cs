using UnityEngine;

//버튼으로 조작되는 오브젝트
public interface ISwitchable
{
    public bool IsActive { get; } // 활성화된 상태인가?
    void Active(); // 활성화
    void Deactive(); // 비활성화
}
