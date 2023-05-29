using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class HUABotManager : MonoBehaviour
{
    [Serializable]
    public enum SwingStyle
    {
        SwingUp,
        SwingDown
    };


    [Serializable]
    public struct HitPointInfo
    {
        public Transform transform; // hitpoint position
        public float time; // swing time
        public SwingStyle swingStyle;
        public bool needJump;
        public bool skip;

        public HitPointInfo(HitPointInfo hitpoint)
        {
            transform = hitpoint.transform;
            time = hitpoint.time;
            swingStyle = hitpoint.swingStyle;
            needJump = hitpoint.needJump;
            skip = hitpoint.skip;
        }

        public void doSwing(PlayerMovement p)
        {
            switch (swingStyle)
            {
                case SwingStyle.SwingUp:
                    p.swinUpInputFlag = true;
                    break;
                case SwingStyle.SwingDown:
                    p.swinDownInputFlag = true;
                    break;
                default:
                    break;
            }
        }
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
    [SerializeField] List<HitPointInfo> allSwingStyle;
    [SerializeField] int WhichStyle;

    [SerializeField][ReadOnly] HitPointInfo botSwingStyle;
    [SerializeField][ReadOnly] BallTrackDraw.TrackPointInfo[] trackPointInfos;

    Vector3 preBallVel;

    [Header("Jump")]
    [SerializeField] float MaxHeight;
    [SerializeField] float MinHeight;
    [SerializeField] [ReadOnly] float jumpHeight;

    float jumpTime;
    float botOnGroundY;

    Coroutine jumpCO;
    Coroutine swinCO;
    Coroutine moveCO;
    Coroutine takeActionCO;


    Vector3 GizmosHitPoint;
    void Start()
    {
        // Trigger OnBallCollide when ball collide 
        ball.OnCollideEvent.AddListener(OnBallCollide);
        ball.OnHitEvent.AddListener(OnBallHit);
        ball.OnServeEvent.AddListener(OnBallServe);

        botOnGroundY = botPlayer.transform.position.y;
        MaxHeight = MathF.Pow((Vector3.up.y * botPlayer.jumpForce) / botPlayer.rb.mass, 2) * -1 / (2 * Physics.gravity.y);

        foreach (var item in UnderHandBack)
        {
            allSwingStyle.Add(item);
        }
        foreach (var item in UnderHandFront)
        {
            allSwingStyle.Add(item);
        }
        foreach (var item in OverHand)
        {
            allSwingStyle.Add(item);
        }

        //foreach (var item in UnderHandBack)
        //{
        //    HitPointInfo tmp = new HitPointInfo(item);
        //    tmp.needJump = true;
        //    allSwingStyle.Add(tmp);
        //}
        //foreach (var item in UnderHandFront)
        //{
        //    HitPointInfo tmp = new HitPointInfo(item);
        //    tmp.needJump = true;
        //    allSwingStyle.Add(tmp);
        //}

        //foreach (var item in Smash)
        //{
        //    if (item.skip)
        //        continue;
        //    HitPointInfo tmp = new HitPointInfo(item);
        //    tmp.needJump = true;
        //    allSwingStyle.Add(tmp);
        //}
    }

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
                if(takeActionCO != null)
                    StopCoroutine(takeActionCO);
                takeActionCO = StartCoroutine(TakeActionCoroutine(1));
            }
        }
        preBallVel = ball.rb.velocity;

    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawCube(botPlayer.transform.position - new Vector3(0.15f, 0.15f, 0.01f), new Vector3(0.3f, 0.3f, 0.01f));
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(GizmosHitPoint - new Vector3(0.1f, 0.1f, 0f), new Vector3(0.2f, 0.2f, 0.01f));
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(botSwingStyle.transform.position.x - 0.1f, botSwingStyle.transform.position.y - 0.1f, 0), new Vector3(0.2f, 0.2f, 0.01f));
            Vector3[] track = BallTrackDraw.getTrack(ball.rb, ball.transform.position, new Vector3(6.8f, 0, 0));

            for (int idx = 0; idx < track.Length - 1; idx++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(track[idx], track[idx + 1]);
            }
        }

        //int i = (int)BallTrackDraw.MaxPointTime(ball.rb.velocity, ball.rb.drag);
        //Gizmos.DrawCube(BallTrackDraw.getPointByTime(ball.transform.position, ball.rb.velocity, ball.rb.drag, i),Vector3.one * 0.1f);

        int i = (int)BallTrackDraw.getPointTimeByHeight(ball.transform.position, ball.rb.velocity, ball.rb.drag, 0.01f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(BallTrackDraw.getPointByTime(ball.transform.position, ball.rb.velocity, ball.rb.drag, i), Vector3.one * 0.05f);
    }

    #region Bot Action

    IEnumerator TakeActionCoroutine(int delayCount)
    {
        for (int i = 0; i < delayCount; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        BotTakeAction();
    }
    private void BotTakeAction()
    {

        int i = (int)BallTrackDraw.getPointTimeByHeight(ball.transform.position, ball.rb.velocity, ball.rb.drag, 0.01f);
        float dropPoint = BallTrackDraw.getPointByTime(ball.transform.position, ball.rb.velocity, ball.rb.drag, i).x;
        if (isPositionXInSameSide(dropPoint))
        {
            Vector3[] track = BallTrackDraw.getTrack(ball.rb, ball.transform.position, new Vector3(6.5f, 0, 0));
            List<HitPointInfo> tmpSwingStyle = new List<HitPointInfo>(allSwingStyle);
            //when list is empty, then break
            while (tmpSwingStyle.Count > 0)
            {
                WhichStyle = UnityEngine.Random.Range(0, tmpSwingStyle.Count);
                botSwingStyle = tmpSwingStyle[WhichStyle];

                //if swing type need jump
                if (botSwingStyle.needJump)
                {
                    //if ball reach height
                    jumpHeight = UnityEngine.Random.Range(MinHeight, MaxHeight);
                    if (BallTrackDraw.isBallReachHeight(ball.rb, ball.transform.position, botSwingStyle.transform.position.y + jumpHeight,  Time.time, ref trackPointInfos)
                        && trackPointInfos[trackPointInfos.Length - 1].position.x > 0.1 * Mathf.Sign(transform.position.x))
                    {
                        GizmosHitPoint = trackPointInfos[trackPointInfos.Length - 1].position;

                        //if bot can jump to ball,then start move jump swing
                        if (getTimeToHeight(jumpHeight, ref jumpTime))
                        {
                            if(jumpCO != null)
                                StopCoroutine(jumpCO);
                            jumpCO = StartCoroutine(JumpCoroutine(trackPointInfos[trackPointInfos.Length - 1].time - Time.time - jumpTime));
                            
                            if (swinCO != null)
                                StopCoroutine(swinCO);
                            swinCO = StartCoroutine(SwingCoroutine(botSwingStyle, trackPointInfos[trackPointInfos.Length - 1].time - Time.time - botSwingStyle.time));

                            if(moveCO != null)
                                StopCoroutine(moveCO);
                            moveCO = StartCoroutine(MoveCoroutine(trackPointInfos[trackPointInfos.Length - 1].position.x, 0.05f, botSwingStyle));

                            break;
                        }
                        //if not,delete the swingStyle
                        else
                        {
                            GizmosHitPoint = Vector3.zero;

                            tmpSwingStyle.RemoveAt(WhichStyle);
                            continue;
                        }

                    }
                    //if the ball can't reach height,delete this swing style
                    else
                    {
                        GizmosHitPoint = Vector3.zero;

                        tmpSwingStyle.RemoveAt(WhichStyle);
                        continue;
                    }
                }
                //if swing type don't need jump
                else
                {
                    //if ball reach height
                    if (BallTrackDraw.isBallReachHeight(ball.rb, ball.transform.position, botSwingStyle.transform.position.y, Time.time, ref trackPointInfos)
                         && trackPointInfos[trackPointInfos.Length - 1].position.x > 0.1 * Mathf.Sign(transform.position.x))
                    {
                        GizmosHitPoint = trackPointInfos[trackPointInfos.Length - 1].position;

                        if (jumpCO != null)
                            StopCoroutine(jumpCO);

                        if (swinCO != null)
                            StopCoroutine(swinCO);
                        swinCO = StartCoroutine(SwingCoroutine(botSwingStyle, trackPointInfos[trackPointInfos.Length - 1].time - Time.time - botSwingStyle.time));

                        if (moveCO != null)
                            StopCoroutine(moveCO);
                        moveCO = StartCoroutine(MoveCoroutine(trackPointInfos[trackPointInfos.Length - 1].position.x, 0.05f, botSwingStyle));

                        break;
                    }
                    //if the ball can't reach height,delete this swing style
                    else
                    {
                        GizmosHitPoint = Vector3.zero;

                        tmpSwingStyle.RemoveAt(WhichStyle);
                        continue;
                    }
                }
            }
        }
        else
        {
            if (swinCO != null)
                StopCoroutine(swinCO);
            if (jumpCO != null)
                StopCoroutine(jumpCO);

            if (moveCO != null)
                StopCoroutine(moveCO);
            moveCO = StartCoroutine(MoveCoroutine(3.5f * Mathf.Sign(botPlayer.transform.position.x), 0.1f));
        }
    }

    private void MoveBotTo(float position, float moveRange)
    {
        if (botPlayer.transform.position.x > position + moveRange)
        {
            botPlayer.moveInputFlag = -1;
        }
        else if (botPlayer.transform.position.x < position - moveRange)
        {
            botPlayer.moveInputFlag = 1;
        }
        else
        {
            botPlayer.moveInputFlag = 0;
        }
    }
    IEnumerator JumpCoroutine(float jumpTime)
    {
        yield return new WaitForSeconds(jumpTime);
        botPlayer.jumpInputFlag = true;
    }
    IEnumerator SwingCoroutine(HitPointInfo hitPoint, float delay)
    {
        yield return new WaitForSeconds(delay);
        hitPoint.doSwing(botPlayer);
    }
    IEnumerator MoveCoroutine(float position, float moveRange, HitPointInfo hitPoint)
    {
        bool toGoal = false;
        while (!toGoal)
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
                toGoal = true;
            }
            yield return null;
        }
    }
    IEnumerator MoveCoroutine(float position, float moveRange)
    {
        bool toGoal = false;
        while (!toGoal)
        {
            if (botPlayer.transform.position.x > position + moveRange)
            {
                botPlayer.moveInputFlag = -1;
            }
            else if (botPlayer.transform.position.x < position - moveRange)
            {
                botPlayer.moveInputFlag = 1;
            }
            else
            {
                botPlayer.moveInputFlag = 0;
                toGoal = true;
            }
            yield return null;
        }
    }
    private void Serve()
    {
        botPlayer.swinUpInputFlag = true;
    }
    #endregion

    void OnBallCollide(Collision other)
    {
        print("Ball collide with something");

        if (other.gameObject.tag != "Ground")
        {
            if(takeActionCO != null)
                StopCoroutine(takeActionCO);
            takeActionCO = StartCoroutine(TakeActionCoroutine(1));
        }
    }
    void OnBallHit(Collider other)
    {
        print("Ball get hit");
        if (takeActionCO != null)
            StopCoroutine(takeActionCO);
        takeActionCO = StartCoroutine(TakeActionCoroutine(1));
    }
    void OnBallServe()
    {
        print("Ball Serve");
        if (takeActionCO != null)
            StopCoroutine(takeActionCO);
        takeActionCO = StartCoroutine(TakeActionCoroutine(1));
    }

    private float getMoveTimeToPosition(float fromX, float toX)
    {
        return Mathf.Abs(fromX - toX) / botPlayer.movementSpeed;
    }
    private bool getTimeToHeight(float height, ref float resultTime)
    {
        // Height =  initalVelocity * Time + gravity * Time * Time / 2.

        float v0 = botPlayer.jumpForce / botPlayer.rb.mass;
        float a = Physics.gravity.y / 2;
        float b = v0;
        float c = -height;

        float D = b * b - 4 * a * c;

        if(D < 0)
        {
            resultTime = 0f;
            return false;

        }
        else if(D == 0)
        {
            resultTime = (-1 * b + Mathf.Pow(D, 0.5f)) / (2 * a);
            return true;
        }

        float t0 = (-1 * b + Mathf.Pow((b * b - 4 * a * c), 0.5f)) / (2 * a);
        float t1 = (-1 * b - Mathf.Pow((b * b - 4 * a * c), 0.5f)) / (2 * a);

        resultTime = t0 > t1 ?  t1 : t0;
        return true;
    }
    private void ResetInputflag()
    {
        //botPlayer.swinUpInputFlag = false;
        //botPlayer.swinDownInputFlag = false;
        //botPlayer.jumpInputFlag = false;
        botPlayer.moveInputFlag = 0;
    }
    private bool isPositionXInSameSide(float posX)
    {
        return (posX * transform.position.x >= 0.0f);
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
    public static Vector3[] getTrack(Rigidbody rb, Vector3 originPos, Vector3 boundary)
    {
        List<Vector3> tracksPos = new List<Vector3>();
        List<Vector3> tracksVel = new List<Vector3>();

        Vector3 velocity0 = rb.velocity;
        float drag = rb.drag;

        tracksPos.Add(originPos);
        tracksVel.Add(velocity0);

        while (tracksPos[tracksPos.Count - 1].y > boundary.y && MathF.Abs(tracksPos[tracksPos.Count - 1].x) < boundary.x)
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
        if (BallPositions.Length <= 0) return false;

        List<TrackPointInfo> tpInfos = new List<TrackPointInfo>();
        trackPointInfos = tpInfos.ToArray();

        bool reackFlag = false;
        float accT = 0;

        for (int i = 1; i < BallPositions.Length; i++)
        {
            accT += Time.fixedDeltaTime;

            // If height is between current position and previous position.
            if (BallPositions[i].y >= BallPositions[i - 1].y)
            {
                if (BallPositions[i].y >= height && height >= BallPositions[i - 1].y)
                {
                    reackFlag = true;

                    float percentage = (height - BallPositions[i - 1].y) / (BallPositions[i].y - BallPositions[i - 1].y);
                    tpInfos.Add(new TrackPointInfo((BallPositions[i] - BallPositions[i - 1]) * percentage + BallPositions[i - 1],
                        currentTime + accT - Time.fixedDeltaTime + Time.fixedDeltaTime * percentage));

                    trackPointInfos = tpInfos.ToArray();
                }
            }
            else
            {
                if (BallPositions[i - 1].y >= height && height >= BallPositions[i].y)
                {
                    reackFlag = true;

                    float percentage = (height - BallPositions[i].y) / (BallPositions[i - 1].y - BallPositions[i].y);
                    tpInfos.Add(new TrackPointInfo((BallPositions[i - 1] - BallPositions[i]) * percentage + BallPositions[i],
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

        for (; velocitySumCalculator(v0, drag, i).y > targetEV; i++);

        float percentage = (targetEV - velocitySumCalculator(v0, drag, i - 1).y) / (velocitySumCalculator(v0, drag, i).y - velocitySumCalculator(v0, drag, i - 1).y);
       
        return (percentage + i);
    }
    public static Vector3 getPointByTime(Vector3 p0, Vector3 v0, float drag, float i)
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
    private static Vector3 velocitySumCalculator(Vector3 v0, float drag, float i)
    {
        if (i == 0) return v0;

        float K = (1 - Time.fixedDeltaTime * drag);

        return v0 * ((1.0f - Mathf.Pow(K, i)) / (1.0f - K) * K) + 
            Physics.gravity * Time.fixedDeltaTime * (K * (-K * i + i + Mathf.Pow(K, i + 1) - K) / Mathf.Pow(1.0f - K, 2));
    }
}