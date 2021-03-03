using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상수 클래스
public static class DEFINE
{
    public enum SwitchStatus
    {
        On,
        Off
    }

    // Object Type
    public enum ObejctType
    {
        Fish,
        Feed,
        Bubble,
        Boom
    }

    // Fish State
    public enum FishState
    {
        Idle,
        Chase,
        Run
    }

    // Input Type
    public enum InputType
    {
        LeftClick,
        DoubleClick,
        RightClick
    }

    public enum PositionType
    {
        Normal,
        Floor
    }

    // Mouse position Max/Min on Screen.
    // X
    public const float MOUSE_MAX_X = 0.99f;
    public const float MOUSE_MIN_X = 0.01f;
    // Y
    public const float MOUSE_MAX_Y = 1.0f;
    public const float MOUSE_MIN_Y = 0.05f;

    // Object의 생성 시 기본 positionZ.
    public const float BASE_POSITION_Z = 5f;

    // Bubble
    // Speed Random Range.
    public const float BUBBLE_SPEED_RANDOM_MIN = 2f;
    public const float BUBBLE_SPEED_RANDOM_MAX = 5f;
    public const float BUBBLE_SPEED_RANDOM_DIV = 1.2f;
    // Speed
    public const float BUBBLE_SPEED_X_PER_Y_DIV = 8f;
    public const float BUBBLE_SPEED_BASE_X = 1.5f;
    // Scale
    public const float BUBBLE_SCALE_PER_SPEED_DIV = 3f;
    // Random Spawn Speed Range.
    public const float BUBBLE_TIME_RANDOM_MIN = 1f;
    public const float BUBBLE_TIME_RANDOM_MAX = 15f;
    public const float BUBBLE_TIME_RANDOM_MULTI = 3f;

    //Feed
    public const float FEED_SPEED_Y = -0.1f;

    //Fish
    private readonly static int[] FISH_ROTATE_TYPE = new int[12]
    {
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        3,
        3,
        2,
        2,
        3
    };

    private readonly static PositionType[] FISH_POSITION_TYPE = new PositionType[12]
    {
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Normal,
        PositionType.Floor,
        PositionType.Floor,
        PositionType.Floor,
    };

    private readonly static float[] FISH_ROTATE_SPEED = new float[3]
    {
        1.5f,
        4.5f,
        0
    };

    private readonly static float[] FISH_CHASE_SPEED = new float[12]
    {
        0.5f,
        0.6f,
        0.5f,
        0.5f,
        0.7f,
        0.5f,
        0.4f,
        0.4f,
        0.4f,
        0.4f,
        0.4f,
        0.4f
    };

    public const float FISH_BASE_ANGLE_MAX_Y = 270f;
    public const float FISH_BASE_ANGLE_MIN_Y = 90f;
    public const float FISH_FAR_ANGLE_MAX_Y = 285f;
    public const float FISH_FAR_ANGLE_MIN_Y = 75f;
    public const float FISH_NEAR_ANGLE_MAX_Y = 255f;
    public const float FISH_NEAR_ANGLE_MIN_Y = 105f;

    public readonly static float[] FISH_ROTATE_SPEED_Y = new float[12]
    {
        5f,
        5f,
        5f,
        5f,
        5f,
        5f,
        3f,
        3f,
        3f,
        3f,
        3f,
        3f
    };

    public const float FISH_BASE_ANGLE_MAX_X = 45f;
    public const float FISH_BASE_ANGLE_MIN_X = -45;
    public const float FISH_ROTATE_SPEED_X = 5f;

    public const float FISH_SPEED_RANDOM_MIN = 0.1f;
    public const float FISH_SPEED_RANDOM_MAX = 3.0f;
    public readonly static float[] FISH_IDLE_SPEED_MULTI = new float[12]
    {
        0.1f,
        0.12f,
        0.1f,
        0.1f,
        0.14f,
        0.1f,
        0.05f,
        0.05f,
        0.05f,
        0.05f,
        0.05f,
        0.05f
    };
    private readonly static float[] FISH_CHASE_SPEED_MULTI = FISH_CHASE_SPEED;

    public const float FISH_TIME_RANDOM_MIN = 1f;
    public const float FISH_TIME_RANDOM_MAX = 3f;
    public const float FISH_TIME_RANDOM_MULTI = 3f;
    public const float FISH_TIME_RANDOM_BASE = 3f;

    public const float FISH_CHASE_MAX = 4f;
    public const float FISH_RUN_MAX = 6f;

    public const float FISH_ACCEL = 0.05f;

    // FOOD
    public const int FISH_FOOD_MAX = 10;
    public const float FISH_FOOD_MULTI = 0.05f;

    // Object
    //IDLE
    // Position Max/Min on Screen.
    public const float OBJECT_IDLE_MAX_X = 0.95f;
    public const float OBJECT_IDLE_MIN_X = 0.05f;
    public const float OBJECT_IDLE_MAX_Y = 0.95f;
    public const float OBJECT_IDLE_MIN_Y = 0.05f;
    // PositionZ Max/Min
    public const float OBJECT_IDLE_MAX_Z = 6.5f;
    public const float OBJECT_IDLE_MIN_Z = 5f;

    //RUN & CHASE
    // Position Max/Min on Screen.
    public const float OBJECT_RUN_MAX_X = 1.05f;
    public const float OBJECT_RUN_MIN_X = -0.05f;
    public const float OBJECT_RUN_MAX_Y = 1.05f;
    public const float OBJECT_RUN_MIN_Y = -0.05f;
    // PositionZ Max/Min
    public const float OBJECT_RUN_MAX_Z = 8.5f;
    public const float OBJECT_RUN_MIN_Z = 5f;

    // ANIMATION STATUS

    //getter & setter
    public static int GetFishRotateType(int fishType)
    {
        return FISH_ROTATE_TYPE[fishType - 1];
    }
    public static float GetFishRotateSpeed(int fishType)
    {
        return FISH_ROTATE_SPEED[GetFishRotateType(fishType) - 1];
    }

    public static float GetFishIdleSpeedMulti(int fishType)
    {
        return FISH_IDLE_SPEED_MULTI[fishType - 1];
    }

    public static float GetFishChaseSpeed(int fishType)
    {
        return FISH_CHASE_SPEED[fishType - 1];
    }

    public static PositionType GetPositionType(int fishType)
    {
        return FISH_POSITION_TYPE[fishType - 1];
    }
}
