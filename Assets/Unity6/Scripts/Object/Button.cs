using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    [SerializeField]
    public ISwitchable client; // 대상
    public void Interact()
    {
        if(client.IsActive) // 활성화 상태라면
        {
            client.Deactive(); // 비활성화
        }
        else
        {
            client.Active(); //활성화
        }
    }
}
