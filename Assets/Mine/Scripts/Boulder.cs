using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//define boulder(in-game object)
public class Boulder : MonoBehaviour
{
    public GameInType inType;
    public CharacterType characterType;
    public Vector2Int currentPos;
    public int health = 50;
    public int damage = 100;
    public float lifetime = 25;

    public Animator animator;
    public Image image;
    public TMPro.TextMeshProUGUI text;

    bool isDamage = false;

    // Start is called before the first frame update
    void Start()
    {
        isDamage = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if boulder's life time is over, destroy the boulder
        if(lifetime < 1)
        {
            if (!isDamage)
            {
                isDamage = true;
                health = 0;
                animator.SetBool("isLive", false);
                GameController.instance.m_BoulderList.Remove(gameObject);
                Destroy(gameObject, 1f);
            }
        }
        else
        {
            lifetime -= Time.deltaTime;
            text.text = health.ToString() + "HP";
        }
    }

    public void SetHealth(int amount)
    {
        this.health = amount;
    }

    public void SetPos(Vector2Int pos)
    {
        this.currentPos = pos;
        gameObject.transform.position = GameController.GetRealPosition(pos);

        if(lifetime >= 1)
        {
            if (!isDamage)
            {
                //if the boulder pos is same with player or enemy, destroy boulder and give damage to player or enemy
                if (currentPos == CharacterController.Player.currentPos)
                {
                    {
                        isDamage = true;
                        CharacterController.Player.TakeDamage(damage);
                        CharacterController.Player.Stun = 1.5f;
                        CharacterController.Player.StunDelay = 0f;
                        CharacterController.Player.Mode = 1;

                        animator.SetBool("isLive", false);
                        Destroy(gameObject, 1f);
                    }
                }
                else if (currentPos == CharacterController.Enemy.currentPos)
                {
                    {
                        isDamage = true;
                        CharacterController.Enemy.TakeDamage(damage);
                        CharacterController.Enemy.Stun = 1.5f;
                        CharacterController.Enemy.StunDelay = 0f;
                        CharacterController.Enemy.Mode = 1;

                        animator.SetBool("isLive", false);
                        Destroy(gameObject, 1f);
                    }
                }
            }
        }
    }

    //get damage, if health is below than 0, destroy boulder
    public void TakeDamage(int amount)
    {
        int hl = health - amount;
        if (hl < 0)
        {
            hl = 0;
        }
        health = hl;
        if(health == 0)
        {
            animator.SetBool("isLive", false);
            Destroy(gameObject, 1f);
            return;
        }
    }

    public void OnDestroy()
    {
        TileController.Info(currentPos).boulder = null;
        GameController.instance.m_BoulderList.Remove(gameObject);
    }
}
