using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float invulnerabilityTime = 2f;
    
    [Header("투비 컨티뉴")]
    [SerializeField]
    private GameObject toBeCon;
    
    private GameObject player;
    private bool isPlayerDead = false;
    
    protected override void Awake()
    {
        base.Awake();
        // TODO: GameManager 초기화 코드
    }
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // TODO: SceneSystem 이벤트 구독
    }

    private void OnDestroy()
    {
        
    }

    public void ProcessPlayerFall(int fallDamage, bool water)
    {
        if (player == null) return;
        
        StartCoroutine(FallRespawnProcess(fallDamage, water));
    }

    private IEnumerator FallRespawnProcess(int fallDamage, bool water)
    {
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            playerHp.TakeFallDamage(fallDamage, water);

            if (playerHp.GetCurrentHp() <= 0)
            {
                yield break;
            }
        }

        // 낙사 체크포인트로 이동
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.MoveToFallbackCheckpoint(player);
        }
    }
    
    public void PlayerDied()
    {
        isPlayerDead = true;
        DisableTimeEvents();
    }

    // 타임 매니저 이벤트 중지
    private void DisableTimeEvents()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen())
        {
            TimeManager.Instance.ResumeTime();
        }
    }
    
    // 수동 부활 메서드
    public void RespawnPlayer()
    {
        if (!isPlayerDead) return;

        StartCoroutine(ManualRespawnProcess());
    }

    private IEnumerator ManualRespawnProcess()
    {
        // 딜레이 
        yield return new WaitForSeconds(respawnDelay);
        
        // 체크포인트 매니저가 있는지 확인
        if (CheckpointManager.Instance != null && player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetOnRespawn();
            }
            
            CheckpointManager.Instance.RespawnAtCheckpoint(player);
            
            isPlayerDead = false;
        }
        else
        {
            RestartGame();
        }
    }
    
    public void RestartGame()
    {
        StartCoroutine(RestartTimer());
    }

    private IEnumerator RestartTimer()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        yield return new WaitForSeconds(respawnDelay);
        
        CheckpointManager.Instance.ResetCheckpoints();
        SceneManager.LoadScene(currentScene.name);
    }
    
    public void ToBe()
    {
        if (toBeCon != null)
            toBeCon.SetActive(true);
    }
}
