using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHealth = 3;
    public int damage = 1;
    public float speed = 2f;
    public Sprite sprite;
    public int enemyCost;
}
