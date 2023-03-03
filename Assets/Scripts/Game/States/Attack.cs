using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Attack : State
{
    private AtkType atkType;
    private Chip chip = null;
    private bool resolvedAttack;
    private float attackStart;
    private int iterations;
    private List<Tile> tilesToAdvance;

    //04/11/2022
    private float m_SlidingTime = 0.2f;
    //

    //Create a class that actually inheritance from MonoBehaviour
    public class MyStaticMB : MonoBehaviour { }

    MyStaticMB myStaticMB;

    //Now Initialize the variable (instance)
    private void Init()
    {
        //If the instance not exit the first time we call the static class
        if (myStaticMB == null)
        {
            //Create an empty object called MyStatic
            GameObject gameObject = new GameObject("MyStatic");

            //Add this script to the object
            myStaticMB = gameObject.AddComponent<MyStaticMB>();
        }
    }


    public Attack(Character character, Chip currentChip) : base(character)
    {
        atkType = currentChip.atkType;
        stateType = StateType.Attack;
        chip = currentChip;
        resolvedAttack = false;
        attackStart = Time.time;
        iterations = 0;
        tilesToAdvance = new List<Tile>();
        if (character.mambaActive && chip.level == 1)
        {
            //Debug.LogError("Mamba");

            //chip.Damage *= 2;
            //character.mambaActive = false;
        }
    }

    public override void OnStateEnter()
    {
        if (character.waiting)
        {
            character.SetState(new Idle(character));
            return;
        }

        if(Global.MyCT == character.characterType)
		{
            PunManager._Instance.SendAttack(chip);
		}

        switch (atkType)
        {
            case AtkType.AreaAdvance:
                character.PlayAnim("AreaAdvance");
                break;
            case AtkType.Blaster:
                character.PlayAnim("Blaster");
                break;
            case AtkType.ForcePalm:
                character.PlayAnim("ForcePalm");
                break;
            case AtkType.ForcePulse:
                character.PlayAnim("ForcePulse");
                break;
            case AtkType.K9:
                character.PlayAnim("K9");
                break;
            case AtkType.MambaMentality:
                character.PlayAnim("MambaMentality");
                character.particleSystem.gameObject.SetActive(true);
                break;
            case AtkType.MGun:
                character.PlayAnim("MGun");
                break;
            case AtkType.NanoSword:
                character.PlayAnim("NanoSword");
                break;
            case AtkType.Orbitar:
                character.PlayAnim("Orbitar");
                break;
            case AtkType.ThorsFury:
                character.PlayAnim("ThorsFury");
                break;
            case AtkType.Basic:
                //if(character.GetComponent<Animator>().GetBool("FirstBlast"))
                character.PlayAnim("Basic");
                break;
            case AtkType.Charge:
                character.PlayAnim("ChargeAttack");
                break;
            case AtkType.FireBlast:
                character.PlayAnim("FireBlast");
                break;
            case AtkType.Recovery50:
                //character.PlayAnim("MambaMentality");
                break;
            case AtkType.Boulder:
                character.PlayAnim("ForcePulse");
                break;
            case AtkType.SlapOff:
                character.PlayAnim("ForcePulse");
                break;
            case AtkType.BubbleShield:
                character.PlayAnim("ForcePulse");
                break;
            case AtkType.PopFire:
                character.PlayAnim("MGun");
                break;
            case AtkType.ShadowVeil:
                character.PlayAnim("ForcePulse");
                break;
            case AtkType.PeaceMaker:
                character.PlayAnim("Dash");
                break;
            case AtkType.Earthquake:
                character.PlayAnim("ThorsFury");
                break;
            default:
                character.PlayAnim("Basic");
                break;
        }
    }

    public override void OnStateExit()
    {
        if (character.mambaActive && chip.atkType != AtkType.MambaMentality)
        {
            character.mambaActive = false;
            //chip.Damage /= 2;
        }

        character.particleSystem.gameObject.SetActive(false);
    }

    public override void Tick()
    {
        switch (atkType)
        {
            case AtkType.AreaAdvance:
                AreaAdvance();
                break;
            case AtkType.Blaster:
                Blaster();
                break;
            case AtkType.ForcePalm:
                ForcePalm();
                break;
            case AtkType.ForcePulse:
                ForcePulse();
                break;
            case AtkType.K9:
                K9();
                break;
            case AtkType.MambaMentality:
                MambaMentality();
                break;
            case AtkType.MGun:
                MGun();
                break;
            case AtkType.NanoSword:
                NanoSword();
                break;
            case AtkType.Orbitar:
                Orbitar();
                break;
            case AtkType.ThorsFury:
                ThorsFury();
                break;
            case AtkType.Basic:
                BasicAttack();
                break;
            case AtkType.FireBlast:
                FireBlast();
                break;
            case AtkType.Charge:
                ChargeAttack();
                break;
            case AtkType.Boulder:
                Boulder();
                break;
            case AtkType.BubbleShield:
                BubbleShield();
                break;
            case AtkType.Earthquake:
                Earthquake();
                break;
            case AtkType.PeaceMaker:
                Peacemaker();
                break;
            case AtkType.PopFire:
                PopFire();
                break;
            case AtkType.RadiantBurst:
                RadiantBurst();
                break;
            case AtkType.Recovery50:
                Recovery50();
                break;
            case AtkType.SlapOff:
                SlapOff();
                break;
            case AtkType.ShadowVeil:
                ShadowVeil();
                break;
        }        
    }

    // hack: added to allow moving without using states
    public override void ForceSetTarget(Character character, Vector2Int target)
    {
        if (atkType != AtkType.ThorsFury) return;
        MoveRequestManager.Get.RequestMove(character, target);
    }

    private void FireBlast()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            GameController.SpawnFireBlast(character, character == CharacterController.Player ? true: false);

            //character.gameObject.GetComponentInChildren<FireBlast>(true).EnableParticles();

            resolvedAttack = true;
            for (int y = Mathf.Max(character.currentPos.y - 1, 0); y <= Mathf.Min(character.currentPos.y + 1, TileController.BoardSize.y - 1); y++)
            {
                if (character.characterType == CharacterType.Player)
                {
                    for (int x = character.currentPos.x + 1; x <= Mathf.Min(TileController.BoardSize.x - 1, character.currentPos.x + 3); x++)
                    {
                        if (CharacterController.Enemy.currentPos == new Vector2Int(x, y))
                        {
                            if (x != character.currentPos.x + 1 || y == character.currentPos.y)
                            {
                                //process mamba/stun status, give damage
                                //18/10/2022
                                if(CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                                {
                                    if (character.mambaActive)
                                    {
                                        CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        CharacterController.Enemy.TakeDamage(chip.Damage);
                                    }

                                    CharacterController.Enemy.Stun = 1.5f;
                                    CharacterController.Enemy.StunDelay = 0f;
                                    CharacterController.Enemy.Mode = 1;
                                }
                                if (TileController.Info(new Vector2Int(x, y)).owner != character)
                                {
                                    if (x != character.currentPos.x + 1 || y == character.currentPos.y)
                                        TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                }
                                y = TileController.BoardSize.y;
                                break;
                                //
                            }
                        }
                        else 
                        {
                            //25/10/2022
                            for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                            {
                                if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y)) //?
                                {
                                    //process mamba, give damage into boulder
                                    if (character.mambaActive)
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                    }

                                    if (TileController.Info(new Vector2Int(x, y)).owner != character)
                                    {
                                        if (x != character.currentPos.x + 1 || y == character.currentPos.y)
                                            TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                    }
                                    x = Mathf.Min(TileController.BoardSize.x - 1, character.currentPos.x + 3) + 1;
                                    y = TileController.BoardSize.y;
                                    break;
                                }
                            }
                            //
                        }

                    }
                }
                else
                {
                    for (int x = character.currentPos.x - 1; x >= Mathf.Max(0, character.currentPos.x - 3); x--)
                    {
                        if (CharacterController.Player.currentPos == new Vector2Int(x, y))
                        {
                            if (x != character.currentPos.x - 1 || y == character.currentPos.y)
                            {
                                //process mamba/stun status, give damage

                                //18/10/2022
                                if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                                {
                                    if (character.mambaActive)
                                    {
                                        CharacterController.Player.TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        CharacterController.Player.TakeDamage(chip.Damage);
                                    }

                                    CharacterController.Player.Stun = 1.5f;
                                    CharacterController.Player.StunDelay = 0f;
                                    CharacterController.Player.Mode = 1;
                                }
                                if (TileController.Info(new Vector2Int(x, y)).owner != character)
                                {
                                    if (x != character.currentPos.x - 1 || y == character.currentPos.y)
                                        TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                }
                                y = TileController.BoardSize.y;
                                break;
                                //
                            }
                            else 
                            {
                                //25/10/2022
                                for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                                {
                                    if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y)) //?
                                    {
                                        //process mamba, give damage into boulder
                                        if (character.mambaActive)
                                        {
                                            GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                        }
                                        else
                                        {
                                            GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                        }

                                        if (TileController.Info(new Vector2Int(x, y)).owner != character)
                                        {
                                            if (x != character.currentPos.x - 1 || y == character.currentPos.y)
                                                TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                        }
                                        x = Mathf.Max(0, character.currentPos.x - 3) - 1;
                                        y = TileController.BoardSize.y;
                                        break;
                                    }
                                }
                                //
                            }
                        }
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
        {
            //character.gameObject.GetComponentInChildren<FireBlast>().DisableParticles();
            character.SetState(new Idle(character));
        }
    }

    private void BasicAttack(bool stun = false)
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;
            if (character.characterType == CharacterType.Player)
            {
                for (int x = character.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                {
                    //process mamba/stun status, give damage
                    if (CharacterController.Enemy.currentPos == new Vector2Int(x, character.currentPos.y))
                    {
                        //18/10/2022
                        if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                        {
                            if (character.mambaActive)
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage);
                            }

                            CharacterController.Enemy.Stun = 0.6f;
                            CharacterController.Enemy.StunDelay = 0f;
                            CharacterController.Enemy.Mode = 3;
                        }
                        TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                        break;
                        //
                    }
                    else
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y))
                            {
                                TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                x = TileController.BoardSize.x;
                                break;
                            }
                        }
                        //
                    }
                }
            }
            else
            {
                for (int x = character.currentPos.x - 1; x >= 0; x--)
                {
                    if (CharacterController.Player.currentPos == new Vector2Int(x, character.currentPos.y))
                    {
                        //18/10/2022
                        if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                        {
                            //process mamba/stun status, give damage
                            if (character.mambaActive)
                            {
                                CharacterController.Player.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Player.TakeDamage(chip.Damage);
                            }

                            CharacterController.Player.Stun = 0.6f;
                            CharacterController.Player.StunDelay = 0f;
                            CharacterController.Player.Mode = 3;
                        }
                        TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                        break;
                        //
                    }
                    else{
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y))
                            {
                                TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                x = -1;
                                break;
                            }
                        }
                        //
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown && !character.GetComponent<Animator>().GetBool("ContinueBlasting"))
            character.SetState(new Idle(character));
    }

    private void ChargeAttack(bool stun = false)
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;
            if (character.characterType == CharacterType.Player)
            {
                for (int x = character.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                {
                    if (x >= TileController.BoardSize.x)
                        break;

                    if (CharacterController.Enemy.currentPos == new Vector2Int(x, character.currentPos.y))
                    {
                        //process mamba/stun status, give damage
                        //18/10/2022
                        if (CharacterController.Enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage);
                            }

                            CharacterController.Enemy.Stun = 0.6f;
                            CharacterController.Enemy.StunDelay = 0f;
                            CharacterController.Enemy.Mode = 3;
                        }
                        TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                        break;
                        //
                    }
                    else
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y))
                            {
                                TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                x = TileController.BoardSize.x;
                                break;
                            }
                        }
                        //
                    }
                }
            }
            else
            {
                for (int x = character.currentPos.x - 1; x >= 0; x--)
                {
                    if (CharacterController.Player.currentPos == new Vector2Int(x, character.currentPos.y))
                    {
                        //process mamba/stun status, give damage
                        //18/10/2022
                        if (CharacterController.Player.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                CharacterController.Player.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Player.TakeDamage(chip.Damage);
                            }

                            CharacterController.Player.Stun = 0.6f;
                            CharacterController.Player.StunDelay = 0f;
                            CharacterController.Player.Mode = 3;
                        }
                        TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                        break;
                        //
                    }
                    else 
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y))
                            {
                                TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                x = -1;
                                break;
                            }
                        }
                        //
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown && !character.GetComponent<Animator>().GetBool("ContinueBlasting"))
            character.SetState(new Idle(character));
    }

    private void ThorsFury()
    {
        //Call the Initialization
        Init();

        //Call the Coroutine
        myStaticMB.StartCoroutine(ActiveThorsFury());
    }

    //And finally, our coroutine (a simple console print)
    private IEnumerator ActiveThorsFury()
    {
        Debug.Log("ThorsFury");

        if (iterations < 9 && Time.time > attackStart + (chip.damageDelay * iterations))
        {
            iterations++;

            List<Vector2Int> validTiles = new List<Vector2Int>();

            for (int x = 0; x < TileController.BoardSize.x; x++)
            {
                for (int y = 0; y < TileController.BoardSize.y; y++)
                {
                    var pos = new Vector2Int(x, y);

                    if (TileController.Info(pos).OwnerType != character.characterType)
                        validTiles.Add(pos);
                }
            }
            var opponent = character.characterType == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;

            Vector2Int strikePos;
            if (Random.Range(0, 4) == 0)
                strikePos = opponent.currentPos;
            else
                strikePos = validTiles[Random.Range(0, validTiles.Count)];

            TileController.Info(strikePos).HighAlert(chip.cooldown + 0.3f, true);
            yield return new WaitForSeconds(0.5f);

            var bolt = GameObject.Instantiate(GameController.LightningPrefab, TileController.Info(strikePos).transform.position, Quaternion.identity, GameController.Canvas.transform);
            GameController.ShakeScreen(50);
            GameObject.Destroy(bolt, 0.6f);

            if (opponent.currentPos == strikePos)
            {
                //process stun status, give damage
                //18/10/2022
                //if (opponent.Stun == 0)
                {
                    opponent.TakeDamage(chip.Damage);
                    opponent.Stun = 1.5f;
                    opponent.StunDelay = 0f;
                    opponent.Mode = 1;
                }
                //
            }
            {
                //25/10/2022
                for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                {
                    if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == strikePos)
                    {
                        //process mamba, give damage into boulder
                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                        //break;
                    }
                }
                //
            }
            yield return new WaitForSeconds(0.6f);
        }

        if (character.mambaActive)
        {
            character.mambaActive = false;
        }

        if (iterations == 9)
            character.SetState(new Idle(character));
    }


    bool spawnedOrbitar = false;
    GameObject orbitarProjectileInstance = null;
    private void Orbitar()
    {
        if (!spawnedOrbitar && Time.time > attackStart + 0.5f)
        {
            orbitarProjectileInstance = GameController.SpawnOrbitar(character, character.currentPos + (character == CharacterController.Player ? (Vector2Int.right * 3) : -(Vector2Int.right * 3)));
            spawnedOrbitar = true;
        }

        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            foreach (var child in orbitarProjectileInstance.GetComponentsInChildren<Transform>(true))
                child.gameObject.SetActive(true);


            if (character.characterType == CharacterType.Player)
            {
                for (int x = character.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                {
                    //if (x >= TileController.BoardSize.x)
                    //    continue;
                    for (int y = 0; y < TileController.BoardSize.y; y++)
                    {
                        if (x == character.currentPos.x + 3 || y == character.currentPos.y)
                        {
                            if (CharacterController.Enemy.currentPos == new Vector2Int(x, y))
                            {
                                //process mamba/stun status, give damage
                                //18/10/2022
                                if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                                {
                                    if (character.mambaActive)
                                    {
                                        CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        CharacterController.Enemy.TakeDamage(chip.Damage);
                                    }

                                    CharacterController.Enemy.Stun = 1.5f;
                                    CharacterController.Enemy.StunDelay = 0f;
                                    CharacterController.Enemy.Mode = 1;
                                }
                                TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                //x = TileController.BoardSize.x;
                                //break;
                                //
                            }
                            //else
                            {
                                //25/10/2022
                                for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                                {
                                    if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, y))
                                    {
                                        TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                        //process mamba, give damage into boulder
                                        if (character.mambaActive)
                                        {
                                            GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                        }
                                        else
                                        {
                                            GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                        }

                                        //x = TileController.BoardSize.x;
                                        //y = TileController.BoardSize.y;
                                        //break;
                                    }
                                }
                                //
                            }
                        }
                    }
                }
            }
            else
            {
                for (int x = character.currentPos.x - 1; x >= 0; x--)
                {
                    for (int y = 0; y < TileController.BoardSize.y; y++)
                    {
                        if (x == character.currentPos.x - 3 || y == character.currentPos.y)
                        {
                            if (CharacterController.Player.currentPos == new Vector2Int(x, y))
                            {

                                //18/10/2022
                                if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                                {
                                    //process mamba/stun status, give damage
                                    if (character.mambaActive)
                                    {
                                        CharacterController.Player.TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        CharacterController.Player.TakeDamage(chip.Damage);
                                    }

                                    CharacterController.Player.Stun = 1.5f;
                                    CharacterController.Player.StunDelay = 0f;
                                    CharacterController.Player.Mode = 1;
                                }
                                TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                //x = -1;
                                //break;
                                //
                            }
                            //else
                            {
                                //25/10/2022
                                for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                                {
                                    if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, y))
                                    {
                                        TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                        //process mamba, give damage into boulder
                                        if (character.mambaActive)
                                        {
                                            GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                        }
                                        else
                                        {
                                            GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                        }

                                        //x = -1;
                                        //y = TileController.BoardSize.y;
                                        //break;
                                    }
                                }
                                //
                            }
                        }
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
        {
            character.SetState(new Idle(character));
            spawnedOrbitar = false;
        }
    }
    
    private void NanoSword()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            var beam = character.transform.GetChild(2);
            beam.gameObject.SetActive(true);
            beam.GetComponent<ParticleSystemRenderer>().material = GameController.BeamMaterials[(int)character.characterName];
            beam.GetComponent<ParticleSystem>().Play();

            GameController.DisableAfterSeconds(beam, 0.3f);

            resolvedAttack = true;
            if (character.characterType == CharacterType.Player)
            {
                for (int x = character.currentPos.x + 1; x < character.currentPos.x + 3; x++)
                {
                    if(x >= TileController.BoardSize.x)
                        break;

                    if (CharacterController.Enemy.currentPos == new Vector2Int(x, character.currentPos.y))
                    {
                        //process mamba/stun status, give damage
                        //18/10/2022
                        if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                        {
                            if (character.mambaActive)
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage);
                            }

                            CharacterController.Enemy.Stun = 1.5f;
                            CharacterController.Enemy.StunDelay = 0f;
                            CharacterController.Enemy.Mode = 1;
                        }
                        TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                        //break;
                        //
                    }
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y))
                            {
                                TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                //x = character.currentPos.x + 3;
                                //break;
                            }
                        }
                        //
                    }

                }
            }
            else
            {
                for (int x = character.currentPos.x - 1; x > character.currentPos.x - 3; x--)
                {
                    if(x < 0)
                    {
                        break;
                    }

                    if (CharacterController.Player.currentPos == new Vector2Int(x, character.currentPos.y))
                    {
                        //18/10/2022
                        if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                        {
                            //process mamba/stun status, give damage
                            if (character.mambaActive)
                            {
                                CharacterController.Player.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Player.TakeDamage(chip.Damage);
                            }

                            CharacterController.Player.Stun = 1.5f;
                            CharacterController.Player.StunDelay = 0f;
                            CharacterController.Player.Mode = 1;
                        }
                        TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                        //break;
                        //
                    }
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, character.currentPos.y))
                            {
                                TileController.Info(new Vector2Int(x, character.currentPos.y)).Highlight(chip.cooldown);
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                //x = character.currentPos.x - 3;
                                //break;
                            }
                        }
                        //
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void MGun() => BasicAttack(true);

    private void MambaMentality()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;
            character.mambaActive = true;
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void K9()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            var enemyPos= CharacterController.Enemy.currentPos;
            GameController.SpawnWolf(character, enemyPos);

            int yStart = character.currentPos.y > 0 ? character.currentPos.y - 1 : character.currentPos.y;

            if (character.characterType == CharacterType.Player)
            {
                for (int x = character.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                {
                    if (x >= TileController.BoardSize.x)
                        break;
                    for (int y = yStart; y < yStart + 2; y++)
                    {
                        if (CharacterController.Enemy.currentPos == new Vector2Int(x, y))
                        {
                            //18/10/2022
                            if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                            {
                                //process mamba/stun status, give damage
                                if (character.mambaActive)
                                {
                                    CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    CharacterController.Enemy.TakeDamage(chip.Damage);
                                }

                                CharacterController.Enemy.Stun = 1.5f;
                                CharacterController.Enemy.StunDelay = 0f;
                                CharacterController.Enemy.Mode = 1;
                            }
                            TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                            x = TileController.BoardSize.x;
                            break;
                            //
                        }
                        else
                        {
                            //25/10/2022
                            for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                            {
                                if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, y))
                                {
                                    TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                    //process mamba, give damage into boulder
                                    if (character.mambaActive)
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                    }

                                    x = TileController.BoardSize.x;
                                    y = yStart + 2;
                                    break;
                                }
                            }
                            //
                        }

                    }
                }
            }
            else
            {
                for (int x = character.currentPos.x - 1; x >= 0; x--)
                {
                    for (int y = yStart; y < yStart + 2; y++)
                    {
                        if (CharacterController.Player.currentPos == new Vector2Int(x, y))
                        {
                            //18/10/2022
                            if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                            {
                                //process mamba/stun status, give damage
                                if (character.mambaActive)
                                {
                                    CharacterController.Player.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    CharacterController.Player.TakeDamage(chip.Damage);
                                }

                                CharacterController.Player.Stun = 1.5f;
                                CharacterController.Player.StunDelay = 0f;
                                CharacterController.Player.Mode = 1;
                            }
                            TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                            x = 0;
                            break;
                            //
                        }
                        else
                        {
                            //25/10/2022
                            for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                            {
                                if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos == new Vector2Int(x, y))
                                {
                                    TileController.Info(new Vector2Int(x, y)).Highlight(chip.cooldown);
                                    //process mamba, give damage into boulder
                                    if (character.mambaActive)
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                    }

                                    x = 0;
                                    y = yStart + 2;
                                    break;
                                }
                            }
                            //
                        }
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void ForcePulse()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            character.transform.GetChild(4).gameObject.SetActive(true);
            resolvedAttack = true;

            if (character.characterType == CharacterType.Player)
            {
                //10/11/2022
                m_BoulderIndexList.Clear();
                m_BoulderPosList.Clear();
                int x1 = -1; int x2 = -1;
                //

                for (int x = CharacterController.Player.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                {
                    if (x >= TileController.BoardSize.x)
                        break;
                    TileController.Info(new Vector2Int(x, CharacterController.Player.currentPos.y)).Highlight(chip.cooldown);
                }
                for (int x = 0; x < TileController.BoardSize.x; x++)
                {
                    if (character.currentPos.y == CharacterController.Enemy.currentPos.y && x == CharacterController.Enemy.currentPos.x)
                    {
                        x1 = x;
                    }
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (character.currentPos.y == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                            {
                                x2 = x;
                                TileController.Info(GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos).boulder = null;

                                int index = i;
                                Vector2Int pos = new Vector2Int(TileController.BoardSize.x - 1, GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y);
                                m_BoulderIndexList.Add(index);
                                m_BoulderPosList.Add(pos);

                                //x = TileController.BoardSize.x;
                                //break;
                            }
                        }
                        //
                    }
                }

                if (x1 >= 0)
                {
                    if (x2 >= 0)
                    {
                        var enemy = CharacterController.Enemy;
                        //process mamba/stun status, give damage
                        //18/10/2022
                        //if (enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                enemy.TakeDamage(100 * m_BoulderIndexList.Count * 2);
                            }
                            else
                            {
                                enemy.TakeDamage(100 * m_BoulderIndexList.Count);
                            }

                            enemy.Stun = 1.5f;
                            enemy.StunDelay = 0f;
                            enemy.Mode = 1;
                        }
                        //
                        for (int i = 0; i < m_BoulderIndexList.Count; i++)
                        {
                            LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                            //give damage into boulder
                            GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>().TakeDamage(50);
                        }
                    }
                    else
                    {
                        var enemy = CharacterController.Enemy;
                        //process mamba/stun status, give damage
                        //18/10/2022
                        //if (enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                enemy.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                enemy.TakeDamage(chip.Damage);
                            }

                            enemy.Stun = 0.5f;
                            enemy.StunDelay = 0f;
                            enemy.Mode = 0;
                        }
                        //
                    }
                    if (character.currentPos.y == CharacterController.Enemy.currentPos.y)
                    {
                        var enemy = CharacterController.Enemy;
                        enemy.SetState(new SetTarget(enemy, new Vector2Int(TileController.BoardSize.x - 1, enemy.currentPos.y)));
                    }
                }
                else
                {
                    if (x2 >= 0)
                    {
                        //process mamba, give damage into boulder
                        if (character.mambaActive)
                        {
                            GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                        }
                        else
                        {
                            GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage);
                        }
                        for (int i = 0; i < m_BoulderIndexList.Count; i++)
                        {
                            LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                        }
                        //Call the Initialization
                        Init();

                        //Call the Coroutine
                        myStaticMB.StartCoroutine(SetBoulderPos());
                    }
                }
            }
            else
            {
                //10/11/2022
                m_BoulderIndexList.Clear();
                m_BoulderPosList.Clear();
                int x1 = -1; int x2 = -1;
                //

                if (CharacterController.Enemy.currentPos.x - 1 >= 0)
                {
                    for (int x = CharacterController.Enemy.currentPos.x - 1; x >= 0; x--)
                    {
                        TileController.Info(new Vector2Int(x, CharacterController.Enemy.currentPos.y)).Highlight(chip.cooldown);
                    }
                    for (int x = TileController.BoardSize.x - 1; x >= 0; x--)
                    {
                        if (character.currentPos.y == CharacterController.Player.currentPos.y && x == CharacterController.Enemy.currentPos.x)
                        {
                            x1 = x;
                        }
                        else
                        {
                            //25/10/2022
                            for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                            {
                                if (character.currentPos.y == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                                {
                                    x2 = x;
                                    TileController.Info(GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos).boulder = null;

                                    int index = i;
                                    Vector2Int pos = new Vector2Int(0, GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y);
                                    m_BoulderIndexList.Add(index);
                                    m_BoulderPosList.Add(pos);

                                    //x = -1;
                                    //break;
                                }
                            }
                            //
                        }
                    }

                    if (x1 >= 0)
                    {
                        if (x2 >= 0)
                        {
                            var player = CharacterController.Player;
                            //process mamba/stun status, give damage
                            //18/10/2022
                            //if (enemy.Stun == 0)
                            {
                                if (character.mambaActive)
                                {
                                    player.TakeDamage(100 * m_BoulderIndexList.Count * 2);
                                }
                                else
                                {
                                    player.TakeDamage(100 * m_BoulderIndexList.Count);
                                }

                                player.Stun = 1.5f;
                                player.StunDelay = 0f;
                                player.Mode = 1;
                            }
                            //
                            for (int i = 0; i < m_BoulderIndexList.Count; i++)
                            {
                                LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                                //give damage into boulder
                                GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>().TakeDamage(50);
                            }
                        }
                        else
                        {
                            var player = CharacterController.Player;
                            //process mamba/stun status, give damage
                            //18/10/2022
                            //if (enemy.Stun == 0)
                            {
                                if (character.mambaActive)
                                {
                                    player.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    player.TakeDamage(chip.Damage);
                                }

                                player.Stun = 0.5f;
                                player.StunDelay = 0f;
                                player.Mode = 0;
                            }
                            //
                        }
                        if (character.currentPos.y == CharacterController.Player.currentPos.y)
                        {
                            var player = CharacterController.Player;
                            player.SetState(new SetTarget(player, new Vector2Int(0, player.currentPos.y)));
                        }
                    }
                    else
                    {
                        if (x2 >= 0)
                        {
                            //process mamba, give damage into boulder
                            if (character.mambaActive)
                            {
                                GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage);
                            }

                            for (int i = 0; i < m_BoulderIndexList.Count; i++)
                            {
                                LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                            }
                            //Call the Initialization
                            Init();

                            //Call the Coroutine
                            myStaticMB.StartCoroutine(SetBoulderPos());
                        }
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
        {
            character.transform.GetChild(4).gameObject.SetActive(false);
        }
        if (Time.time > attackStart + chip.cooldown)
        {
            character.SetState(new Idle(character));
        }
    }

    //04/11/2022
    private List<int> m_BoulderIndexList = new List<int>();
    private List<Vector2Int> m_BoulderPosList = new List<Vector2Int>();

    private IEnumerator SetBoulderPos()
    {
        yield return new WaitForSeconds(m_SlidingTime);
        for(int i = 0; i < m_BoulderIndexList.Count; i++)
        {
            GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>().SetPos(m_BoulderPosList[i]);
            TileController.Info(GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>().currentPos).boulder = 
                GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>();
        }
    }
    //

    private void ForcePalm()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            character.transform.GetChild(3).gameObject.SetActive(true);
            resolvedAttack = true;


            if (character.characterType == CharacterType.Player)
            {
                //10/11/2022
                m_BoulderIndexList.Clear();
                m_BoulderPosList.Clear();
                int x1 = -1; int x2 = -1;
                //
                var enemy = CharacterController.Enemy;
                int furthestColumn = enemy.currentPos.x;

                for (int x = TileController.BoardSize.x - 1; x >= 0; x--)
                {
                    if (TileController.Info(new Vector2Int(x, CharacterController.Player.currentPos.y)).OwnerType == CharacterType.Player)
                    {
                        TileController.Info(new Vector2Int(x, CharacterController.Player.currentPos.y)).Highlight(chip.cooldown);
                        furthestColumn = x + 1;
                        break;
                    }
                }

                for (int x = TileController.BoardSize.x - 1; x >= 0; x--)
                {
                    if (character.currentPos.y == CharacterController.Enemy.currentPos.y && x == CharacterController.Enemy.currentPos.x)
                    {
                        x1 = x;
                    }
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (character.currentPos.y == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                            {
                                x2 = x;
                                TileController.Info(GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos).boulder = null;

                                int index = i;
                                Vector2Int pos = new Vector2Int(furthestColumn, GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y);
                                m_BoulderIndexList.Add(index);
                                m_BoulderPosList.Add(pos);

                                //x = -1;
                                //break;
                            }
                        }
                        //
                    }
                }

                if(x1 >= 0)
                {
                    if (character.currentPos.y == CharacterController.Enemy.currentPos.y)
                    {
                        enemy.SetState(new SetTarget(enemy, new Vector2Int(furthestColumn, enemy.currentPos.y)));
                    }
                    if (x2 >= 0)
                    {
                        //process mamba/stun status, give damage
                        //if (enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                enemy.TakeDamage(100 * m_BoulderIndexList.Count * 2);
                            }
                            else
                            {
                                enemy.TakeDamage(100 * m_BoulderIndexList.Count);
                            }

                            enemy.Stun = 1.5f;
                            enemy.StunDelay = 0f;
                            enemy.Mode = 1;
                        }
                        for(int i = 0; i < m_BoulderIndexList.Count; i++)
                        {
                            LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                            //give damage into boulder
                            GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>().TakeDamage(50);
                        }
                    }
                    else
                    {
                        //process mamba/stun status, give damage
                        //if (enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                enemy.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                enemy.TakeDamage(chip.Damage);
                            }

                            enemy.Stun = 3f;
                            enemy.StunDelay = 0f;
                            enemy.Mode = 2;
                        }
                    }
                }
                else
                {
                    if (x2 >= 0)
                    {
                        //process mamba, give damage into boulder
                        if (character.mambaActive)
                        {
                            GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                        }
                        else
                        {
                            GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage);
                        }

                        for (int i = 0; i < m_BoulderIndexList.Count; i++)
                        {
                            LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                        }
                        //Call the Initialization
                        Init();

                        //Call the Coroutine
                        myStaticMB.StartCoroutine(SetBoulderPos());
                    }
                }
            }
            else
            {
                //10/11/2022
                m_BoulderIndexList.Clear();
                m_BoulderPosList.Clear();
                int x1 = -1; int x2 = -1;
                //
                var player = CharacterController.Player;
                int furthestColumn = player.currentPos.x;

                for (int x = 0; x < TileController.BoardSize.x; x++)
                {
                    if (TileController.Info(new Vector2Int(x, CharacterController.Enemy.currentPos.y)).OwnerType == CharacterType.Enemy)
                    {
                        TileController.Info(new Vector2Int(x, CharacterController.Enemy.currentPos.y)).Highlight(chip.cooldown);

                        furthestColumn = x - 1;
                        break;
                    }
                }

                for (int x = 0; x < TileController.BoardSize.x; x++)
                {
                    if (character.currentPos.y == CharacterController.Player.currentPos.y && x == CharacterController.Player.currentPos.x)
                    {
                        x1 = x;
                    }
                    else
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (character.currentPos.y == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                            {
                                TileController.Info(GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos).boulder = null;

                                x2 = x;
                                TileController.Info(GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos).boulder = null;

                                int index = i;
                                Vector2Int pos = new Vector2Int(furthestColumn, GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y);
                                m_BoulderIndexList.Add(index);
                                m_BoulderPosList.Add(pos);

                                //x = TileController.BoardSize.x;
                                //break;
                            }
                        }
                        //
                    }
                }

                if (x1 >= 0)
                {
                    if (character.currentPos.y == CharacterController.Player.currentPos.y)
                    {
                        player.SetState(new SetTarget(player, new Vector2Int(furthestColumn, player.currentPos.y)));
                    }
                    if (x2 >= 0)
                    {
                        //process mamba/stun status, give damage
                        //if (enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                player.TakeDamage(100 * m_BoulderIndexList.Count * 2);
                            }
                            else
                            {
                                player.TakeDamage(100 * m_BoulderIndexList.Count);
                            }

                            player.Stun = 1.5f;
                            player.StunDelay = 0f;
                            player.Mode = 1;
                        }
                        for (int i = 0; i < m_BoulderIndexList.Count; i++)
                        {
                            LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                            //give damage into boulder
                            GameController.instance.m_BoulderList[m_BoulderIndexList[i]].GetComponent<Boulder>().TakeDamage(50);
                        }
                    }
                    else
                    {
                        //process mamba/stun status, give damage
                        //if (enemy.Stun == 0)
                        {
                            if (character.mambaActive)
                            {
                                player.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                player.TakeDamage(chip.Damage);
                            }

                            player.Stun = 3f;
                            player.StunDelay = 0f;
                            player.Mode = 2;
                        }
                    }
                }
                else
                {
                    if (x2 >= 0)
                    {
                        //process mamba, give damage into boulder
                        if (character.mambaActive)
                        {
                            GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                        }
                        else
                        {
                            GameController.instance.m_BoulderList[m_BoulderIndexList[0]].GetComponent<Boulder>().TakeDamage(chip.Damage);
                        }

                        for (int i = 0; i < m_BoulderIndexList.Count; i++)
                        {
                            LeanTween.move(GameController.instance.m_BoulderList[m_BoulderIndexList[i]], GameController.GetRealPosition(m_BoulderPosList[i]), m_SlidingTime);
                        }
                        //Call the Initialization
                        Init();

                        //Call the Coroutine
                        myStaticMB.StartCoroutine(SetBoulderPos());
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
        {
            character.transform.GetChild(3).gameObject.SetActive(false);
        }
        if (Time.time > attackStart + chip.cooldown)
        {
            character.SetState(new Idle(character));
        }
    }

    private void Blaster() => BasicAttack(true);

    private void AreaAdvance()
    {
        if (!resolvedAttack)
        {
            if (CharacterController.Player.CurrentStateType == StateType.Move || CharacterController.Player.CurrentStateType == StateType.SetTarget)
                CharacterController.Player.SetState(new Idle(CharacterController.Player));

            if (CharacterController.Enemy.CurrentStateType == StateType.Move || CharacterController.Enemy.CurrentStateType == StateType.SetTarget)
                CharacterController.Enemy.SetState(new Idle(CharacterController.Enemy));

            resolvedAttack = true;
            if (character.characterType == CharacterType.Player)
            {
                CharacterController.Enemy.NoDamageStun = 1.0f;
                int nearestColumn = TileController.BoardSize.x - 2;
                for (int x = 1; x < TileController.BoardSize.x - 2; x++)
                {
                    for (int y = 0; y < TileController.BoardSize.y; y++)
                    {
                        if (TileController.Info(new Vector2Int(x, y)).OwnerType == CharacterType.Enemy)
                        {
                            if (CharacterController.Enemy.currentPos == new Vector2Int(x, y))
                                continue;

                            if (x < nearestColumn)
                                nearestColumn = x;
                            break;
                        }
                    }
                }

                for (int y = 0; y < TileController.BoardSize.y; y++)
                {
                    if (CharacterController.Enemy.currentPos.x <= nearestColumn && CharacterController.Enemy.currentPos.y == y)
                        continue;

                    var tile = TileController.Info(new Vector2Int(nearestColumn, y));
                    tilesToAdvance.Add(tile);
                    tile.Highlight(0.5f, true);
                    GameController.SpawnOrb(tile.coordinate);
                }
            }
            else
            {
                CharacterController.Player.NoDamageStun = 1.0f;
                int nearestColumn = 1;
                for (int x = TileController.BoardSize.x - 2; x > 0; x--)
                {
                    for (int y = 0; y < TileController.BoardSize.y; y++)
                    {
                        if (TileController.Info(new Vector2Int(x, y)).OwnerType == CharacterType.Player)
                        {
                            if (CharacterController.Player.currentPos == new Vector2Int(x, y))
                                continue;

                            if (x > nearestColumn)
                                nearestColumn = x;
                            break;
                        }
                    }
                }

                for (int y = 0; y < TileController.BoardSize.y; y++)
                {
                    if (CharacterController.Player.currentPos.x >= nearestColumn && CharacterController.Player.currentPos.y == y)
                        continue;

                    var tile = TileController.Info(new Vector2Int(nearestColumn, y));
                    tilesToAdvance.Add(tile);
                    tile.Highlight(0.5f, true);
                    GameController.SpawnOrb(tile.coordinate);
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.damageDelay && resolvedAttack)
        {
            foreach (var t in tilesToAdvance)
            {
                if (character.characterType == CharacterType.Player)
                {
                    t.owner = CharacterController.Player;
                    t.RevertOwnership(10.0f);
                }
                else
                {
                    t.owner = CharacterController.Enemy;
                    t.RevertOwnership(10.0f);
                }
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void Boulder()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }

            Vector2Int targetIndex = character.currentPos + (character == CharacterController.Player ? (Vector2Int.right * 1) : -(Vector2Int.right * 1));
            GameController.SpawnBoulder(targetIndex, character.characterType);

            Debug.Log("Boulder spawned at vector index : " + targetIndex);

            //if (character.characterType == CharacterType.Player)
            //{
            //    Debug.Log("player used Boulder attack, enemy current position = " + CharacterController.Enemy.currentPos);
            //    var enemy = CharacterController.Enemy;
            //    if(targetIndex == enemy.currentPos)
            //    {
            //        //18/10/2022
            //        if (enemy.Stun == 0)
            //        {
            //            enemy.TakeDamage(chip.Damage);
            //            enemy.Stun = 1.5f;
            //            enemy.StunDelay = 0f;
            //            enemy.Mode = 1;
            //        }
            //        //
            //    }
            //}
            //else
            //{
            //    Debug.Log("enemy used Boulder attack");
            //    var player = CharacterController.Player;
            //    if (targetIndex == player.currentPos)
            //    {
            //        //18/10/2022
            //        if (player.Stun == 0)
            //        {
            //            player.TakeDamage(chip.Damage);
            //            player.Stun = 1.5f;
            //            player.StunDelay = 0f;
            //            player.Mode = 1;
            //        }
            //        //
            //    }
            //}
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void BubbleShield()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }

            GameController.SpawnBubbleShield(character, character.currentPos);
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void Earthquake()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
            
            // spawn tile on all the board, then each walked tile should get broken
            // and marked as non walkable. time before tile recover to normal state is about 5s
            var allTiles = TileController.Board;

            for(int i = 0; i < allTiles.GetLength(0); i++)
            {
                for(int j = 0; j < allTiles.GetLength(1); j++)
                {
                    Vector2Int target = new Vector2Int(i, j);
                    GameController.SpawnEarthquakeTile(target, false, 5f);
                }
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void Peacemaker()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            // play weapon animation, whatever we will hit
            GameController.SpawnPeacemaker(character, character.currentPos);

            GameController.SetTimer(chip.cooldown, () =>
            {
                // solve hit impact after charge delay ( based on cooldown value
                if (character.characterType == CharacterType.Player)
                {
                    // check if we hit opponent or obstacle, otherwise we will blow the last back row behind
                    // the opponent.

                    bool bEffect = false;
                    for (int x = character.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                    {
                        if (character.currentPos.y == CharacterController.Enemy.currentPos.y && x == CharacterController.Enemy.currentPos.x)
                        {
                            //process mamba/stun status, give damage
                            //18/10/2022
                            GameController.SpawnPeacemakerPopFire(CharacterController.Enemy.currentPos);
                            if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                            {
                                if (character.mambaActive)
                                {
                                    CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    CharacterController.Enemy.TakeDamage(chip.Damage);
                                }

                                CharacterController.Enemy.Stun = 1.5f;
                                CharacterController.Enemy.StunDelay = 0f;
                                CharacterController.Enemy.Mode = 1;
                            }
                            bEffect = true;
                            x = TileController.BoardSize.x;
                            break;
                        }
                        else
                        {
                            //25/10/2022
                            for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                            {
                                if (character.currentPos.y == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                                {
                                    //process mamba, give damage into boulder
                                    if (character.mambaActive)
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                    }
                                    bEffect = true;
                                    x = TileController.BoardSize.x;
                                    break;
                                }
                            }
                        }
                    }

                    if(!bEffect)
                    {
                        if(CharacterController.Enemy.currentPos.x == TileController.BoardSize.x - 1)
                        {
                            //process mamba/stun status, give damage
                            if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                            {
                                bEffect = true;

                                if (character.mambaActive)
                                {
                                    CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    CharacterController.Enemy.TakeDamage(chip.Damage);
                                }
                                CharacterController.Enemy.Stun = 1.5f;
                                CharacterController.Enemy.StunDelay = 0f;
                                CharacterController.Enemy.Mode = 1;
                            }
                        }
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x == TileController.BoardSize.x - 1)
                            {
                                bEffect = true;
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }
                            }
                        }
                    }

                    if(!bEffect)
                    {
                        GameController.ShakeScreen(chip.Damage);
                    }

                    // get back row behind opponent
                    int direction = CharacterController.Enemy.currentPos.x - character.currentPos.x;
                    List<Vector2Int> backRows = TileController.GetAllLastRows(direction);
                    foreach (var r in backRows)
                    {
                        // spawn both popfire2 and earthquake tiles
                        GameController.SpawnPeacemakerPopFire(r, true);
                    }
                    //
                }
                else
                {
                    bool bEffect = false;
                    for (int x = character.currentPos.x - 1; x >= 0; x--)
                    {
                        if (character.currentPos.y == CharacterController.Player.currentPos.y && x == CharacterController.Player.currentPos.x)
                        {
                            //process mamba/stun status, give damage
                            //18/10/2022
                            GameController.SpawnPeacemakerPopFire(CharacterController.Player.currentPos);
                            if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                            {
                                if (character.mambaActive)
                                {
                                    CharacterController.Player.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    CharacterController.Player.TakeDamage(chip.Damage);
                                }
                                CharacterController.Player.Stun = 1.5f;
                                CharacterController.Player.StunDelay = 0f;
                                CharacterController.Player.Mode = 1;
                            }
                            bEffect = true;
                            x = -1;
                            break;
                            //
                        }
                        else
                        {
                            //25/10/2022
                            for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                            {
                                if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y == character.currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                                {
                                    //process mamba, give damage into boulder
                                    if (character.mambaActive)
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                    }
                                    else
                                    {
                                        GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                    }
                                    bEffect = true;
                                    x = -1;
                                    break;
                                }
                            }
                            //
                        }
                    }

                    if (!bEffect)
                    {
                        if (CharacterController.Player.currentPos.x == 0)
                        {
                            //process mamba/stun status, give damage
                            if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                            {
                                bEffect = true;
                                if (character.mambaActive)
                                {
                                    CharacterController.Player.TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    CharacterController.Player.TakeDamage(chip.Damage);
                                }
                                CharacterController.Player.Stun = 1.5f;
                                CharacterController.Player.StunDelay = 0f;
                                CharacterController.Player.Mode = 1;
                            }
                        }
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x == 0)
                            {
                                bEffect = true;
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }
                            }
                        }
                    }

                    if (!bEffect)
                    {
                        GameController.ShakeScreen(chip.Damage);
                    }

                    // get back row behind opponent
                    int direction = CharacterController.Player.currentPos.x - character.currentPos.x;
                    List<Vector2Int> backRows = TileController.GetAllLastRows(direction);

                    foreach (var r in backRows)
                    {
                        // spawn both popfire2 and earthquake tiles
                        GameController.SpawnPeacemakerPopFire(r, true);
                    }
                }
            });

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void PopFire()
    {
        if (!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            // todo check for item other than player.

            if(character.characterType == CharacterType.Player)
            {
                for (int x = character.currentPos.x + 1; x < TileController.BoardSize.x; x++)
                {
                    // check if opponent in same row
                    if (character.currentPos.y == CharacterController.Enemy.currentPos.y && x == CharacterController.Enemy.currentPos.x)
                    {
                        //process mamba/stun status, give damage
                        //18/10/2022
                        GameController.SpawnPopFire(CharacterController.Enemy.currentPos);
                        if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                        {
                            if (character.mambaActive)
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Enemy.TakeDamage(chip.Damage);
                            }

                            CharacterController.Enemy.Stun = 1.5f;
                            CharacterController.Enemy.StunDelay = 0f;
                            CharacterController.Enemy.Mode = 1;
                        }
                        break;
                        //
                    }
                    else
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y == character.currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                            {
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                x = TileController.BoardSize.x;
                                break;
                            }
                        }
                        //
                    }
                }
            }
            else
            {
                for (int x = CharacterController.Enemy.currentPos.x - 1; x >= 0; x--)
                {
                    if (character.currentPos.y == CharacterController.Player.currentPos.y && x == CharacterController.Player.currentPos.x)
                    {
                        //process mamba/stun status, give damage
                        //18/10/2022
                        GameController.SpawnPopFire(CharacterController.Player.currentPos);
                        if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                        {
                            if (character.mambaActive)
                            {
                                CharacterController.Player.TakeDamage(chip.Damage * 2);
                            }
                            else
                            {
                                CharacterController.Player.TakeDamage(chip.Damage);
                            }
                            CharacterController.Player.Stun = 1.5f;
                            CharacterController.Player.StunDelay = 0f;
                            CharacterController.Player.Mode = 1;
                        }
                        break;
                        //
                    }
                    else
                    {
                        //25/10/2022
                        for (int i = 0; i < GameController.instance.m_BoulderList.Count; i++)
                        {
                            if (GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.y == character.currentPos.y && x == GameController.instance.m_BoulderList[i].GetComponent<Boulder>().currentPos.x)
                            {
                                //process mamba, give damage into boulder
                                if (character.mambaActive)
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage * 2);
                                }
                                else
                                {
                                    GameController.instance.m_BoulderList[i].GetComponent<Boulder>().TakeDamage(chip.Damage);
                                }

                                x = -1;
                                break;
                            }
                        }
                        //
                    }
                }
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void RadiantBurst()
    {

    }

    private void Recovery50()
    {
        if(!resolvedAttack)
        {
            resolvedAttack = true;
            GameController.SpawnRecovery50(character.currentPos);
            //process mamba/stun status, heal
            if (character.mambaActive)
            {
                character.Heal(chip.Damage * 2);
            }
            else
            {
                character.Heal(chip.Damage);
            }
        }

        if (character.mambaActive)
        {
            character.mambaActive = false;
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void SlapOff()
    {
        if(!resolvedAttack && Time.time > attackStart + chip.damageDelay)
        {
            resolvedAttack = true;

            if (character.characterType == CharacterType.Player)
            {
                var targetIndex = CharacterController.Enemy.currentPos;
                //process mamba/stun status, give damage
                //18/10/2022
                GameController.SpawnSlapOff(character, targetIndex);
                if (CharacterController.Enemy.Stun == 0 || CharacterController.Enemy.Mode == 2)
                {
                    if (character.mambaActive)
                    {
                        CharacterController.Enemy.TakeDamage(chip.Damage * 2);
                    }
                    else
                    {
                        CharacterController.Enemy.TakeDamage(chip.Damage);
                    }
                    CharacterController.Enemy.TakeDamage(chip.Damage);
                    CharacterController.Enemy.Stun = 1.5f;
                    CharacterController.Enemy.StunDelay = 0f;
                    CharacterController.Enemy.Mode = 1;
                }
                //
            }
            else
            {
                var targetIndex = CharacterController.Player.currentPos;
                //process mamba/stun status, give damage
                //18/10/2022
                GameController.SpawnSlapOff(character, targetIndex);
                if (CharacterController.Player.Stun == 0 || CharacterController.Player.Mode == 2)
                {
                    if (character.mambaActive)
                    {
                        CharacterController.Player.TakeDamage(chip.Damage * 2);
                    }
                    else
                    {
                        CharacterController.Player.TakeDamage(chip.Damage);
                    }
                    CharacterController.Player.Stun = 1.5f;
                    CharacterController.Player.StunDelay = 0f;
                    CharacterController.Player.Mode = 1;
                }
                //
            }

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }

    private void ShadowVeil()
    {
        if (!resolvedAttack)
        {
            resolvedAttack = true;

            character.SetCharacterTransparency(0.5f);
            GameController.SetTimer(5f, () => character.SetCharacterTransparency(1f));

            if (character.mambaActive)
            {
                character.mambaActive = false;
            }
        }

        if (Time.time > attackStart + chip.cooldown)
            character.SetState(new Idle(character));
    }
}