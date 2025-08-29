
public interface IDamageable
{
    void DecreaseHp(int amount); // 데미지 입기
    void RestoreHp(int amount); // 힐 또는 회복

    void Death(); // 사망
}
