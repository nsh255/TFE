using UnityEngine;

[CreateAssetMenu(menuName = "Items/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float atackSpeed;
    public Sprite sprite;
    public GameObject bulletPrefab;
    public GameObject weaponPrefab;
}
