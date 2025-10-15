using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClass", menuName = "Roguelike/PlayerClass")]
public class PlayerClass : ScriptableObject
{
    public string className;
    public int maxHealth = 5;
    public float speed = 32f;
    public WeaponData startingWeaponData;
}
