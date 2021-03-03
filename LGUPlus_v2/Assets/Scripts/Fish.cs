using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Random = UnityEngine.Random;

public class Fish : MonoBehaviour
{
    // status
    public string id;
    public int fishType;
    public DEFINE.FishState fishState;
    private int rotateType;
    private DEFINE.PositionType positionType;
    private int food;

    // game object(fish)
    private GameObject fish;
    private GameObject balloon;
    private Rigidbody rigidBody;
    private Animator animator;

    // camera
    private Camera camera1, camera2, camera3, camera4;

    private Vector3 targetSpeed;

    private ReactiveProperty<bool> isSay, canSay, wantSay;

    // Use this for initialization
    void Start()
    {
        InitCamera();
        InitFish();
    }

    private void Update()
    {
        checkSay();
    }

    void FixedUpdate()
    {
        FishAction();
    }

    void InitCamera()
    {
        try
        {
            camera1 = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>();
            camera2 = GameObject.FindGameObjectWithTag("camera2").GetComponent<Camera>();
            camera3 = GameObject.FindGameObjectWithTag("camera3").GetComponent<Camera>();
            camera4 = GameObject.FindGameObjectWithTag("camera4").GetComponent<Camera>();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    // fish status 초기화
    private void InitFish()
    {
        fish = gameObject;
        balloon = transform.Find("Plane001").gameObject;

        rigidBody = fish.GetComponent<Rigidbody>();
        animator = fish.GetComponent<Animator>();

        rotateType = DEFINE.GetFishRotateType(fishType);
        positionType = DEFINE.GetPositionType(fishType);

        food = 0;

        RepeatChangeSpeed();

        isSay = canSay = wantSay = new BoolReactiveProperty(false);
        canSay.Value = true; // test
        isSay.Subscribe(b => balloon.SetActive(true)); // 임시, 나중에 가려야하면 true를 b로
        canSay.Subscribe(b => checkSay());
        wantSay.Subscribe(b => checkSay());

        StartCoroutine(RandomSay());
    }

    private void checkSay()
    {
        if(wantSay.Value && canSay.Value)
        {
            isSay.Value = true;
        } else
        {
            isSay.Value = false;
        }
    }

    private IEnumerator RandomSay()
    {
        WaitForSeconds wait;
        float time;
        while (true)
        {
            time = wantSay.Value ? 4 : Random.Range(4, 8);
            wait = new WaitForSeconds(time);
            yield return wait;
            wantSay.Value = !wantSay.Value;
        }
    }

    // Fish Speed(x, y)를 Random하게 바꿈
    private void ChangeSpeedRandom()
    {
        float directionX = Random.Range(0, 2) * 2 - 1; // X축 방향
        float directionY = Random.Range(0, 2) * 2 - 1; // Y축 방향
        float directionZ = Random.Range(0, 3) - 1; // Z축 방향
        float randomMin = DEFINE.FISH_SPEED_RANDOM_MIN; // 랜덤 최저
        float randomMax = DEFINE.FISH_SPEED_RANDOM_MAX; // 랜덤 최대
        float multi = DEFINE.GetFishIdleSpeedMulti(fishType); // 속도 계수

        float speedX = Random.Range(randomMin, randomMax) * multi * directionX;
        float speedY = Random.Range(randomMin, randomMax) * multi * directionY;
        float speedZ = Random.Range(randomMin, randomMax) * multi * directionZ;

        if (rotateType == 1)
        {
            speedY /= 2.5f;
        }

        //rigidBody.velocity = new Vector3(speedX, speedY, speedZ);
        targetSpeed = new Vector3(speedX, speedY, speedZ);
    }

    // Fish Speed를 Random하게 바꾸는 function을 Random하게 Call
    private void RepeatChangeSpeed()
    {
        if (fishState == DEFINE.FishState.Idle)
        {
            ChangeSpeedRandom();
        }

        float time = Random.Range(DEFINE.FISH_TIME_RANDOM_MIN, DEFINE.FISH_TIME_RANDOM_MAX) * DEFINE.FISH_TIME_RANDOM_MULTI + DEFINE.FISH_TIME_RANDOM_BASE;
        Invoke("RepeatChangeSpeed", time);
    }

    // Fish Action
    private void FishAction()
    {
        GameObject target;
        GameObject[] feeds = GameObject.FindGameObjectsWithTag("Feed");
        GameObject[] booms = GameObject.FindGameObjectsWithTag("Boom");

        if (positionType == DEFINE.PositionType.Normal) {
            if (target = ScanObject(booms, DEFINE.FISH_RUN_MAX))
            {
                if (fishState != DEFINE.FishState.Run)
                {
                    Run_On();
                }
                moveToTarget(target, DEFINE.ObejctType.Boom);
            }
            else if (target = ScanObject(feeds, DEFINE.FISH_CHASE_MAX))
            {
                if (fishState != DEFINE.FishState.Chase)
                {
                    Chase_On();
                }
                moveToTarget(target, DEFINE.ObejctType.Feed);
            }
            else
            {
                if (fishState != DEFINE.FishState.Idle)
                {
                    Idle_On();
                    ChangeSpeedRandom();
                }
            }
        }

        CheckPosition();
        FishTurn();
    }

    private GameObject ScanObject(GameObject[] objs, float range)
    {
        GameObject targetObj = null;
        float targetDistance = range;

        if (objs.Length > 0)
        {
            foreach (GameObject obj in objs)
            {
                float distance = GetSqrMagnitude(fish, obj);
                if (distance > range)
                {
                    continue;
                }

                if (distance <= targetDistance)
                {
                    targetObj = obj;
                    targetDistance = distance;
                }
            }
        }

        return targetObj;
    }

    // target으로 추적 혹은 도망
    private void moveToTarget(GameObject target, DEFINE.ObejctType type)
    {
        Vector3 targetP, fishP, distance;
        float multi;

        if (type == DEFINE.ObejctType.Boom)
        {
            multi = -1f;

            targetP = camera1.WorldToViewportPoint(target.transform.position);
            targetP.z = 5f;
            targetP = camera1.ViewportToWorldPoint(targetP);
            targetP.z = target.transform.position.z;

            fishP = camera1.WorldToViewportPoint(fish.transform.position);
            fishP.z = 5f;
            fishP = camera1.ViewportToWorldPoint(fishP);
            fishP.z = fish.transform.position.z;
        }
        else
        {
            multi = 1f;
            targetP = target.transform.position;
            fishP = fish.transform.position;
        }
        distance = targetP - fishP;

        float baseSpeed = DEFINE.GetFishChaseSpeed(fishType) * multi;

        float speedZ = distance.normalized.z * baseSpeed;
        if(positionType == DEFINE.PositionType.Floor)
        {
            speedZ = 0;
        }
        baseSpeed = multi > 0 ? baseSpeed : baseSpeed * 4; // 도망갈땐 x, y축 속도 4배
        float speedX = distance.normalized.x * baseSpeed;
        float speedY = distance.normalized.y * baseSpeed;

        //rigidBody.velocity = new Vector3(speedX, speedY, speedZ);
        targetSpeed = new Vector3(speedX, speedY, speedZ);
    }

    //물고기 방향 전환
    private void FishTurn()
    {
        if (rotateType == 3)
        {
            return;
        }

        Quaternion rotation = fish.transform.localRotation;
        Quaternion targetRotation = Quaternion.LookRotation(GetTargetDir());

        if (rotateType == 1)
        {
            targetRotation = CheckRotateLimit(targetRotation);
        }

        if (fishType == 11)
        {
            targetRotation = ChangeRotateType11(targetRotation);
        }

        fish.transform.localRotation = Quaternion.Lerp(rotation, targetRotation, DEFINE.GetFishRotateSpeed(fishType) * Time.deltaTime);

        if(rigidBody.velocity.x >= 0)
        {
            canSay.Value = false;
        }
        
        /*
        Vector3 balloonRotation = balloon.transform.localEulerAngles;
        balloonRotation.y = -(fish.transform.localEulerAngles.y - 180);
        balloon.transform.localRotation = Quaternion.Euler(balloonRotation);

        Vector3 balloonPosition = balloon.transform.localPosition;
        balloon.transform.localPosition = new Vector3(rigidBody.velocity.x > 0 ? -0.2f : 0.2f , balloonPosition.y, balloonPosition.z);
        */
    }

    private Quaternion CheckRotateLimit(Quaternion quaternion)
    {
        Vector3 euler = quaternion.eulerAngles;

        //x
        if (euler.x > 30 && euler.x < 91)
        {
            euler.x = 30;
        }

        if (euler.x > 270 && euler.x < 330)
        {
            euler.x = 330;
        }

        //y
        if (euler.y > 110 && euler.y < 181)
        {
            euler.y = 110;
        }

        if (euler.y < 70)
        {
            euler.y = 70;
        }

        if (euler.y > 180 && euler.y < 250)
        {
            euler.y = 250;
        }

        if (euler.y > 290)
        {
            euler.y = 290;
        }

        return Quaternion.Euler(euler);
    }

    private Quaternion ChangeRotateType11(Quaternion quaternion)
    {
        Vector3 euler = quaternion.eulerAngles;

        euler.x += rigidBody.velocity.x > 0 ? 90 : -90;

        return Quaternion.Euler(euler.x, euler.y, euler.z);
    }

    private Vector3 GetTargetDir()
    {
        Vector3 speed = -rigidBody.velocity;

        if (rotateType == 2)
        {
            speed.z = 0;
            speed.y = speed.x > 0 ? speed.y : -speed.y;
            speed.x = speed.x > 0 ? speed.x : -speed.x;
        }

        return speed;
    }

    // 현재 position을 check해 범위를 벗어났을 경우 적절한 position으로 이동시키거나 velocity를 조절
    private void CheckPosition()
    {
        float maxX, minX, maxY, minY, maxZ, minZ;

        bool b = true;

        if (b)
        {
            maxX = DEFINE.OBJECT_IDLE_MAX_X;
            minX = DEFINE.OBJECT_IDLE_MIN_X;
            maxY = positionType == DEFINE.PositionType.Floor ? DEFINE.OBJECT_IDLE_MAX_Y : 0.66f;
            minY = DEFINE.OBJECT_IDLE_MIN_Y;
            maxZ = DEFINE.OBJECT_IDLE_MAX_Z;
            minZ = DEFINE.OBJECT_IDLE_MIN_Z;
        }
        else
        {
            maxX = DEFINE.OBJECT_RUN_MAX_X;
            minX = DEFINE.OBJECT_RUN_MIN_X;
            maxY = positionType == DEFINE.PositionType.Floor ? DEFINE.OBJECT_RUN_MAX_Y : 0.8f;
            minY = DEFINE.OBJECT_RUN_MIN_Y;
            maxZ = DEFINE.OBJECT_RUN_MAX_Z;
            minZ = DEFINE.OBJECT_RUN_MIN_Z;
        }

        Vector3 speed = rigidBody.velocity;

        speed.x = FishAccel(speed.x, targetSpeed.x);
        speed.y = FishAccel(speed.y, targetSpeed.y);
        speed.z = FishAccel(speed.z, targetSpeed.z);

        //게임 화면 밖으로 나가지 않게함
        Vector3 cam1, cam3;
        if (positionType == DEFINE.PositionType.Normal) {
            cam1 = camera1.WorldToViewportPoint(fish.transform.position);
            cam3 = camera3.WorldToViewportPoint(fish.transform.position);
        } else
        {
            cam1 = cam3 = camera4.WorldToViewportPoint(fish.transform.position);
            speed.z = 0;
        }

        float posZ = fish.transform.position.z;

        if ((cam1.x <= minX && targetSpeed.x < 0) || (cam3.x >= maxX && targetSpeed.x > 0))
        {
            targetSpeed.x *= -1;
            speed.x = 0;
        }

        if ((cam1.y <= minY && targetSpeed.y < 0) || (cam1.y >= maxY && targetSpeed.y > 0))
        {
            targetSpeed.y *= -1;
            speed.y = 0;
        }

        if ((posZ <= minZ && targetSpeed.z < 0) || (posZ >= maxZ && targetSpeed.z > 0))
        {
            targetSpeed.z *= -1;
            speed.z = 0;
        }

        rigidBody.velocity = speed;
    }

    private float FishAccel(float nowSpeed, float targetSpeed)
    {
        if(Mathf.Abs(nowSpeed - targetSpeed) < DEFINE.FISH_ACCEL)
        {
            return targetSpeed;
        }

        return nowSpeed + (nowSpeed < targetSpeed ? DEFINE.FISH_ACCEL : -DEFINE.FISH_ACCEL);
    }

    //먹이 추적상태로 변경
    private void Chase_On()
    {
        fishState = DEFINE.FishState.Chase;
        animator.SetInteger("Status", 2);
    }

    //평소 상태로 변경
    private void Idle_On()
    {
        fishState = DEFINE.FishState.Idle;
        animator.SetInteger("Status", 1);
    }

    //도망가는 상태로 변경
    private void Run_On()
    {
        fishState = DEFINE.FishState.Run;
        animator.SetInteger("Status", 2);
    }

    //먹이 먹기
    private void ConsumeFood(Collider feed)
    {
        FeedScript feedScript = feed.gameObject.GetComponent<FeedScript>();
        ObjectPool.Instance.PushToPool("Feed", feed.gameObject);

        if(food < DEFINE.FISH_FOOD_MAX)
        {
            food++;
            float scale = 1 + (DEFINE.FISH_FOOD_MULTI * food);
            fish.transform.localScale = new Vector3(1, scale, scale);
        }
    }

    //두 오브젝트 거리의 제곱을 리턴
    private float GetSqrMagnitude(GameObject a, GameObject b)
    {
        //return (a.transform.position - b.transform.position).sqrMagnitude;
        //임시로 z축 값은 같도록 처리
        Vector3 aP = camera1.WorldToViewportPoint(a.transform.position);
        Vector3 bP = camera1.WorldToViewportPoint(b.transform.position);
        aP.z = 5f;
        bP.z = 5f;
        aP = camera1.ViewportToWorldPoint(aP);
        bP = camera1.ViewportToWorldPoint(bP);
        float ret = (aP - bP).sqrMagnitude;

        return ret;
    }

    //충돌 이벤트
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Feed"))
        {
            ConsumeFood(other);
        }
    }

    /*
    private float TurnY(Vector3 speed)
    {
        float angleY = fish.transform.localEulerAngles.y;
        float ret = angleY;

        if (DEFINE.FISH_ROTATE_TYPE[fishType - 1] != 1)
        {
            return ret;
        }

        float angleMaxY, angleMinY, rotateSpeedY, targetAngleY, fishAngleY;
        float speedX, speedZ;

        angleMaxY = DEFINE.FISH_BASE_ANGLE_MAX_Y; // 오른쪽
        angleMinY = DEFINE.FISH_BASE_ANGLE_MIN_Y; // 왼쪽

        rotateSpeedY = DEFINE.FISH_ROTATE_SPEED_Y[fishType - 1]; // 물고기 회전 속도
        fishAngleY = fish.transform.localEulerAngles.y; // 물고기 현재 각도

        speedX = rigidBody.velocity.x;
        speedZ = rigidBody.velocity.z;

        //Z축 속도에 따라 max/min angle 설정.
        if (speedZ < 0)
        {
            angleMaxY = DEFINE.FISH_NEAR_ANGLE_MAX_Y;
            angleMinY = DEFINE.FISH_NEAR_ANGLE_MIN_Y;
        }
        else if (speedZ > 0)
        {
            angleMaxY = DEFINE.FISH_FAR_ANGLE_MAX_Y;
            angleMinY = DEFINE.FISH_FAR_ANGLE_MIN_Y;
        }

        //X축 속도에 따라 target angle 설정.
        if (speedX > 0)
        {
            targetAngleY = angleMaxY;
        }
        else
        {
            targetAngleY = angleMinY;
        }

        //10도 미만 차이일 경우 넘어가지 않도록.
        if (Mathf.Abs(targetAngleY - fishAngleY) < 10)
        {
            return ret;
        }

        //target angle에 가까운 회전각으로 회전.
        if (targetAngleY > 180)
        {
            if (fishAngleY > Mathf.Abs(targetAngleY - 180) && fishAngleY < targetAngleY)
            {
                ret = rotateSpeedY;
            }
            else
            {
                ret = -rotateSpeedY;
            }
        }
        else
        {
            if (fishAngleY > Mathf.Abs(targetAngleY - 180) || fishAngleY < targetAngleY)
            {
                ret = rotateSpeedY;
            }
            else
            {
                ret = -rotateSpeedY;
            }
        }

        return ret + angleY;
    }

    private float TurnX()
    {
        float angleX = fish.transform.localEulerAngles.x;
        float ret = angleX;
        ret = 130;

        if (DEFINE.FISH_ROTATE_TYPE[fishType - 1] == 3)
        {
            return ret;
        }

        if (DEFINE.FISH_ROTATE_TYPE[fishType - 1] == 1)
        {
            float angleMaxX, angleMinX, rotateSpeedX, targetAngleX, fishAngleX;
            float speedY;

            angleMaxX = DEFINE.FISH_BASE_ANGLE_MAX_X; // 오른쪽
            angleMinX = DEFINE.FISH_BASE_ANGLE_MIN_X; // 왼쪽

            rotateSpeedX = DEFINE.FISH_ROTATE_SPEED_X; // 물고기 회전 속도
            fishAngleX = fish.transform.localEulerAngles.x; // 물고기 현재 각도
            fishAngleX = fishAngleX < 180 ? fishAngleX : fishAngleX - 360;

            speedY = rigidBody.velocity.y;

            //Y축 속도에 따라 target angle 설정.
            if (speedY > 0)
            {
                targetAngleX = angleMaxX;
            }
            else if (speedY < 0)
            {
                targetAngleX = angleMinX;
            }
            else
            {
                targetAngleX = 0;
            }

            //10도 미만 차이일 경우 넘어가지 않도록.
            if (Mathf.Abs(targetAngleX - fishAngleX) < 10)
            {
                return ret;
            }

            if (fishAngleX < targetAngleX)
            {
                ret = rotateSpeedX;
            }
            else
            {
                ret = -rotateSpeedX;
            }

            ret += angleX;
        }
        else if (DEFINE.FISH_ROTATE_TYPE[fishType - 1] == 2)
        {
            Vector3 speed = rigidBody.velocity;
            float angle = Mathf.Atan2(speed.y, speed.x) * Mathf.Rad2Deg;
            ret = angle;
            Debug.Log(ret);
        }

        return ret;
    }
    */
}
