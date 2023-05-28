using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HUABotManager : MonoBehaviour
{
    [Serializable]
    public struct HitPointInfo
    {
        public Transform transform; // hitpoint position
        public float time; // swing time
    };

    [Header("Game Object")]
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;

    [Header("HitPoint")]
    [SerializeField] HitPointInfo[] UnderHandBack;
    [SerializeField] HitPointInfo[] UnderHandFront;
    [SerializeField] HitPointInfo[] OverHand;
    [SerializeField] HitPointInfo[] Smash;

    [SerializeField] [ReadOnly] HitPointInfo botSwingStyle;
    [SerializeField] [ReadOnly] BallTrackDraw.TrackPointInfo[] trackPointInfos;

    Vector3 preBallVel;

    bool botMove = false;
    bool botSwing = false;
    [SerializeField] private float testHeight;

    // Update is called once per frame
    void Update()
    {
        ResetInputflag();
        if (botPlayer.PrepareServe)
        {
            Serve();
        }
        else
        {
            
            // If ball turn around.
            if (preBallVel.x * ball.rb.velocity.x <= 0 || preBallVel.y <= 0 && ball.rb.velocity.y > 0)
            {
                botSwingStyle = RandomChooseSwingStyle();
                Vector3[] track = BallTrackDraw.getTrack(ball.rb, ball.transform.position);

                //if ball fall position is in bot side then move and swing
                if (track.Length > 0 && track[track.Length - 1].x * botPlayer.transform.position.x >= 0)
                {
                    if (BallTrackDraw.isBallReachHeight(ball.rb, ball.transform.position, botSwingStyle.transform.position.y , Time.time, ref trackPointInfos))
                    {
                        botMove = true;
                    }
                }
            }

            if(trackPointInfos.Length > 0)
            {
               
                if (botMove)
                {
                    MoveHitPointTo(trackPointInfos[trackPointInfos.Length - 1].position.x, 0.09f, botSwingStyle);
                    if (botPlayer.moveInputFlag == 0)
                    {
                        botSwing = true;
                        botMove = false;
                    }
                }

                if (botSwing)
                {
                    if (Swing(botSwingStyle))
                        botSwing = false;
                }
                else
                {
                    botPlayer.swinDownInputFlag = false;
                }
            }
        }

        preBallVel = ball.rb.velocity;

    }

    private void OnDrawGizmos()
    {
        Vector3[] track = BallTrackDraw.getTrack(ball.rb, ball.transform.position);

        for (int idx = 0; idx < track.Length - 1; idx++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(track[idx], track[idx + 1]);
        }

        int i = (int)BallTrackDraw.MaxPointTime(ball.rb.velocity, ball.rb.drag);
        print("I: " + i);
        print("I(float): " + BallTrackDraw.MaxPointTime(ball.rb.velocity, ball.rb.drag));
        Gizmos.DrawCube(BallTrackDraw.getPointByTime(ball.transform.position, ball.rb.velocity, ball.rb.drag, i),Vector3.one * 0.1f);

        i = (int)BallTrackDraw.getPointTimeByHeight(ball.transform.position, ball.rb.velocity, ball.rb.drag, testHeight);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(BallTrackDraw.getPointByTime(ball.transform.position, ball.rb.velocity, ball.rb.drag, i), Vector3.one * 0.1f);
    }

    private void MoveHitPointTo(float position, float moveRange, HitPointInfo hitPoint)
    {
        if (hitPoint.transform.position.x > position + moveRange)
        {
            botPlayer.moveInputFlag = -1;
        }
        else if (hitPoint.transform.position.x < position - moveRange)
        {
            botPlayer.moveInputFlag = 1;
        }
        else
        {
            botPlayer.moveInputFlag = 0;
        }
    }
    private bool Swing(HitPointInfo hitPoint)
    {
        float offsetTime = trackPointInfos[trackPointInfos.Length - 1].time - Time.time;
        if (offsetTime <= hitPoint.time)
        {
            botPlayer.swinDownInputFlag = true;
            return true;
        }
        else
        {
            return false;
        }
    }
    private void Serve()
    {
        botPlayer.swinUpInputFlag = true;
    }
    private void ResetInputflag()
    {
        botPlayer.swinUpInputFlag = false;
        botPlayer.swinDownInputFlag = false;
        botPlayer.jumpInputFlag = false;
        botPlayer.moveInputFlag = 0;
    }
    private HitPointInfo RandomChooseSwingStyle()
    {
        return UnderHandBack[UnityEngine.Random.Range(0, 2)];
    }
}

public class BallTrackDraw
{
    [Serializable]
    public struct TrackPointInfo
    {
        public Vector3 position;
        public float time;

        public TrackPointInfo(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }
    }

    public static Vector3[] getTrack(Rigidbody rb, Vector3 originPos, float time, float timeStep)
    {
        List<Vector3> tracksPos = new List<Vector3>();
        List<Vector3> tracksVel = new List<Vector3>();

        Vector3 velocity0 = rb.velocity;
        float drag = rb.drag;

        tracksPos.Add(originPos);
        tracksVel.Add(velocity0);

        // velocityNew = velocity0 * ( 1 - deltaTime * drag);
        for (float i = 0f; i <= time; i += timeStep)
        {
            tracksVel.Add((tracksVel[tracksVel.Count - 1] + Physics.gravity * timeStep) * (1 - timeStep * drag));

            tracksPos.Add(tracksVel[tracksVel.Count - 1] * timeStep + tracksPos[tracksPos.Count - 1]);

            tracksPos.Add(tracksPos[tracksPos.Count - 1]);

        }
        return tracksPos.ToArray();
    }
    public static Vector3[] getTrack(Rigidbody rb, Vector3 originPos)
    {
        List<Vector3> tracksPos = new List<Vector3>();
        List<Vector3> tracksVel = new List<Vector3>();

        Vector3 velocity0 = rb.velocity;
        float drag = rb.drag;

        tracksPos.Add(originPos);
        tracksVel.Add(velocity0);

        while (tracksPos[tracksPos.Count - 1].y > 0)
        {
            tracksVel.Add((tracksVel[tracksVel.Count - 1] + Physics.gravity * Time.fixedDeltaTime) * (1 - Time.fixedDeltaTime * drag));

            tracksPos.Add(tracksVel[tracksVel.Count - 1] * Time.fixedDeltaTime + tracksPos[tracksPos.Count - 1]);
        }

        return tracksPos.ToArray();
    }

    // if return false, then the ball will not reach the height,
    // otherwise, the ball will reach, and return a trackPointInfos by reference.
    public static bool isBallReachHeight(Rigidbody rb, Vector3 originPos, float height, float currentTime, ref TrackPointInfo[] trackPointInfos)
    {

        bool reackFlag = false;

        List<Vector3> tracksPos = new List<Vector3>();
        List<Vector3> tracksVel = new List<Vector3>();
        float accT = 0;

        List<TrackPointInfo> tpInfos = new List<TrackPointInfo>();

        Vector3 velocity0 = rb.velocity;
        float drag = rb.drag;

        tracksPos.Add(originPos);
        tracksVel.Add(velocity0);
        trackPointInfos = tpInfos.ToArray();

        while (tracksPos[tracksPos.Count - 1].y > 0)
        {
            tracksVel.Add((tracksVel[tracksVel.Count - 1] + Physics.gravity * Time.fixedDeltaTime) * (1 - Time.fixedDeltaTime * drag));

            tracksPos.Add(tracksVel[tracksVel.Count - 1] * Time.fixedDeltaTime + tracksPos[tracksPos.Count - 1]);

            accT += Time.fixedDeltaTime;

            if (tracksPos[tracksPos.Count - 1].y >= tracksPos[tracksPos.Count - 2].y)
            {
                if (tracksPos[tracksPos.Count - 1].y >= height && height >= tracksPos[tracksPos.Count - 2].y)
                {
                    reackFlag = true;

                    float percentage = (height - tracksPos[tracksPos.Count - 2].y) / (tracksPos[tracksPos.Count - 1].y - tracksPos[tracksPos.Count - 2].y);
                    tpInfos.Add(new TrackPointInfo((tracksPos[tracksPos.Count - 1] - tracksPos[tracksPos.Count - 2]) * percentage + tracksPos[tracksPos.Count - 2],
                        currentTime + accT - Time.fixedDeltaTime + Time.fixedDeltaTime * percentage));

                    trackPointInfos = tpInfos.ToArray();
                }
            }
            else
            {
                if (tracksPos[tracksPos.Count - 2].y >= height && height >= tracksPos[tracksPos.Count - 1].y)
                {
                    reackFlag = true;

                    float percentage = (height - tracksPos[tracksPos.Count - 1].y) / (tracksPos[tracksPos.Count - 2].y - tracksPos[tracksPos.Count - 1].y);
                    tpInfos.Add(new TrackPointInfo((tracksPos[tracksPos.Count - 2] - tracksPos[tracksPos.Count - 1]) * percentage + tracksPos[tracksPos.Count - 1],
                        currentTime + accT - Time.fixedDeltaTime + Time.fixedDeltaTime * (1 - percentage)));

                    trackPointInfos = tpInfos.ToArray();
                }

                // Optimize: Dpecific height only encounter 0, 1 or 2 time(s),
                //              so if it reach 2 times, then break;
                if (tpInfos.Count >= 2)
                {
                    break;
                }
            }
        }
        return reackFlag;
    }
    public static bool isBallReachHeight(Vector3[] BallPositions, float height, float currentTime, ref TrackPointInfo[] trackPointInfos)
    {
        List<TrackPointInfo> tpInfos = new List<TrackPointInfo>();
        trackPointInfos = tpInfos.ToArray();

        bool reackFlag = false;
        float accT = 0;

        for (int i = 1; i < BallPositions.Length; i++)
        {
            accT += Time.fixedDeltaTime;

            // If height is between current position and previous position.
            if (BallPositions[i - 1].y >= BallPositions[i - 2].y)
            {
                if (BallPositions[i - 1].y >= height && height >= BallPositions[i - 2].y)
                {
                    reackFlag = true;

                    float percentage = (height - BallPositions[i - 2].y) / (BallPositions[i - 1].y - BallPositions[i - 2].y);
                    tpInfos.Add(new TrackPointInfo((BallPositions[i - 1] - BallPositions[i - 2]) * percentage + BallPositions[i - 2],
                        currentTime + accT - Time.fixedDeltaTime + Time.fixedDeltaTime * percentage));

                    trackPointInfos = tpInfos.ToArray();
                }
            }
            else
            {
                if (BallPositions[i - 2].y >= height && height >= BallPositions[i - 1].y)
                {
                    reackFlag = true;

                    float percentage = (height - BallPositions[i - 1].y) / (BallPositions[i - 2].y - BallPositions[i - 1].y);
                    tpInfos.Add(new TrackPointInfo((BallPositions[i - 2] - BallPositions[i - 1]) * percentage + BallPositions[i - 1],
                        currentTime + accT - Time.fixedDeltaTime + Time.fixedDeltaTime * (1 - percentage)));

                    trackPointInfos = tpInfos.ToArray();
                }

                // Optimize: Dpecific height only encounter 0, 1 or 2 time(s),
                //              so if it reach 2 times, then break;
                if (tpInfos.Count >= 2)
                {
                    break;
                }
            }
        }
        return reackFlag;
    }

    public static float getPointTimeByHeight(Vector3 p0, Vector3 v0, float drag, float height)
    {
        float t = MaxPointTime(v0, drag);
        float maxH = getPointByTime(p0, v0, drag, (int)t).y;
        if (maxH < height) return -1;
        else if (maxH == height) return t;

        float targetEV = (height - p0.y) / Time.fixedDeltaTime;
        int i = ((int)t); // start with the heighest point
        for (; velocitySumCalculator(v0, drag, i).y > targetEV; i++) ;

        return i;
    }
    public static Vector3 getPointByTime(Vector3 p0, Vector3 v0, float drag, int i)
    {
        if (i == 0) return p0;

        return p0 + Time.fixedDeltaTime * velocitySumCalculator(v0, drag, i);
    }
    public static float MaxPointTime(Vector3 v0, float drag)
    {
        if (v0.y <= 0) return 0;

        float K = (1.0f - Time.fixedDeltaTime * drag);

        return -Mathf.Log(1.0f - (v0.y * (1.0f - K) / (Physics.gravity.y * Time.fixedDeltaTime * K))) / Mathf.Log(K);
    }

    private static Vector3 velocityCalculator(Vector3 v0, float drag, int i)
    {
        if (i == 0) return v0;

        float K = (1 - Time.fixedDeltaTime * drag);

        return v0 * Mathf.Pow(K, i) + Physics.gravity * Time.fixedDeltaTime * (K * (1.0f - Mathf.Pow(K, i))) / (1.0f - K);
    }
    private static Vector3 velocitySumCalculator(Vector3 v0, float drag, int i)
    {
        if (i == 0) return v0;

        float K = (1 - Time.fixedDeltaTime * drag);

        return v0 * ((1.0f - Mathf.Pow(K, i)) / (1.0f - K) * K) + 
            Physics.gravity * Time.fixedDeltaTime * (K * (-K * i + i + Mathf.Pow(K, i + 1) - K) / Mathf.Pow(1.0f - K, 2));
    }
}