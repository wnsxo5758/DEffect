using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    [SerializeField]
    private MonoBehaviour clientObject; // 대상

    private ISwitchable client;

    private void Awake()
    {
        client = clientObject as ISwitchable;

        if (client == null)
        {
            Debug.LogWarning($"{clientObject.name}은 ISwitchable 인터페이스가 없습니다");
        }

    }
    public void Interact()
    {
        if (client == null) return;
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
