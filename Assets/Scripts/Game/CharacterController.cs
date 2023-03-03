using UnityEngine;
public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private Character player;
    [SerializeField]
    private Character enemy;
    public static Character Player => instance.player;
    public static Character Enemy => instance.enemy;
    //ADD SCRIPT
    public static Character MyChar => Global.MyCT == CharacterType.Player ? instance.player : instance.enemy;
    //
    private static CharacterController instance;
    private void Awake() => instance = this;

    /*
    private void Update()
    {
        
        if (player.transform.position.y > enemy.transform.position.y)
        {
            player.transform.SetSiblingIndex(3);
            enemy.transform.SetSiblingIndex(4);
        }
        else
        {
            player.transform.SetSiblingIndex(4);
            enemy.transform.SetSiblingIndex(3);
        }
        
    }
    */
}