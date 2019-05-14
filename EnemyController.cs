using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    PATROL,
    CHASE,
    ATTACK
}


public class EnemyController : MonoBehaviour
{
    //private EnemyAnimator enemy_Anim;
    private NavMeshAgent navAgent;

    private EnemyState enemy_State;

    public float walk_Speed = 0.5f;
    public float run_Speed = 4f;

    public float chase_Distance = 7f;/// cat de departe trebuie sa fie enemy-ul de player pana cand incepe sa-l urmareasca,daca e mai mica decat 7 atunci o sa inceapa sa-l urmareasca
    private float current_Chase_Distance;//cand vei trage in enemy o sa inceapa automat sa l urmareasca
    public float attack_Distance = 1.8f;
    public float chase_After_Attack_Distance = 2f;//daca il ataca pe player o sa ii acorde un spatiu pana incepe sa-l urmareasca

    public float patrol_Radius_Min = 20f, patrol_Radius_Max = 60f;
    public float patrol_For_This_Time = 15f;
    private float patrol_timer;

    public float wait_Before_Attack = 2f;
    private float attack_Timer;

    private Transform target;

    public GameObject attack_Point;

    private EnemyAudio enemy_Audio;

   void Awake()
    {
        enemy_Anim = GetComponent<EnemyAnimator>();
        navAgent = GetComponent<NavMeshAgent>();

        target = GameObject.FindWithTag(Tags.PLAYER_TAG).transform;

        EnemyAudio = GetComponentInChildren<EnemyAudio>();
    }

    // Start is called before the first frame update
    void Start()
    {
        enemy_State = EnemyState.PATROL;

        patrol_timer = patrol_For_This_Time;//cat timp enemyul patruleaza pana setez o noua destinatie
        //cand enemyul ajunge prima data la player ataca imediat
        attack_Timer = wait_Before_Attack;
        //memoreaza valoarea chase distance
        current_Chase_Distance = chase_Distance;
    }

    // Update is called once per frame
    void Update()
    {
        if(enemy_State == EnemyState.PATROL)
        {
            Patrol();
        }
        if(enemy_State == EnemyState.CHASE)
        {
            Chase();
        }
        if (enemy_State == EnemyState.ATTACK)
        {
            Attack();
        }

    }

    void Patrol() {
        navAgent.isStopped = false; //nav agent se poate misca
        navAgent.speed = walk_Speed;

        patrol_timer += Time.deltaTime;

        if(patrol_timer > patrol_For_This_Time)
        {
            SetNewRandomDestination();

            patrol_timer = 0f;
        }

        if(navAgent.velocity.sqrMagnitude > 0)
        {
            //enemy_Anim.Walk(true);
        }
        else
        {
            //enemy_Anim.Walk(false);
        }

        //testez distanta dintre player si enemy
        if(Vector3.Distance(transform.position,target.position)<= chase_Distance)//daca e true incepe sa-l urmareasca pe player
        {
            //enemy_Anim.Walk(false);

            enemy_State = EnemyState.CHASE;
            //de adaugat audio

            enemy_Audio.Play_ScreamSound();

        }

    }//patrol

    void Chase() {
        //ii dau voie agentului sa se miste din nou
        navAgent.isStopped = false;
        navAgent.speed = run_Speed;

        navAgent.SetDestination(target.position);//setez destinatia playerului ca destinatie pentru enemy deoarece il urmaresc alergand

        if (navAgent.velocity.sqrMagnitude > 0)
        {
            //enemy_Anim.Run(true);
        }
        else
        {
            //enemy_Anim.Run(false);
        }

        if (Vector3.Distance(transform.position, target.position) <= attack_Distance)//verific daca pot sa il atac
        {
            //opresc animatia
           // enemy_Anim.Run(false);
            //enemy_Anim.Walk(false);
            enemy_State = EnemyState.ATTACK;
            //resetez distanta pentru urmarit la cea anterioara
            if(chase_Distance != current_Chase_Distance)
            {
                chase_Distance = current_Chase_Distance;
            }
        }
        else if(Vector3.Distance(transform.position,target.position)>chase_Distance)
        {
            //playerul fuge de enemy

            //enemy_Anim.Run(false);

            enemy_State = EnemyState.PATROL;//resetez timerul pentru patrulare astfel incat functia sa calculeze o noua destinatie pentru patrulare
            patrol_timer = patrol_For_This_Time;

            if(chase_Distance != current_Chase_Distance)//resetez chase distance
            {
                chase_Distance = current_Chase_Distance;
            }

        }

    }//chase

    void Attack() {
        navAgent.velocity = Vector3.zero;//oprim de tot enemy ul
        navAgent.isStopped = true;

        attack_Timer += Time.deltaTime;

        if(attack_Timer > wait_Before_Attack)
        {
            //enemy_Anim.Attack();

            attack_Timer = 0f;

            //sunet pentru atac

            enemy_Audio.Play_AttackSound();
        }
        if(Vector3.Distance(transform.position,target.position)> attack_Distance + chase_After_Attack_Distance)//daca playerul fuge marim distanta pentru urmarit
        {
            enemy_State = EnemyState.CHASE;
        }

    }//attack

    void SetNewRandomDestination() {
        float rand_Radius = Random.Range(patrol_Radius_Min, patrol_Radius_Max);

        Vector3 randDir = Random.insideUnitSphere * rand_Radius;
        randDir += transform.position; //genereaza o pozitie random

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDir, out navHit,rand_Radius, -1);//daca se genereaza o pozitie care nu e navigabila (in afara hartii etc),-1 urmareste pe fiecare layer

        navAgent.SetDestination(navHit.position);

          

    }

    void Turn_On_AttackPoint()
    {
        attack_Point.SetActive(true);
    }
    void Turn_Off_AttackPoint()
    {
        if (attack_Point.activeInHierarchy)
        {
            attack_Point.SetActive(false);
        }
    }

}//class
