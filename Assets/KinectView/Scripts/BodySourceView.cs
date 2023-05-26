using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    [SerializeField] PlayerMovement playerMovement1;
    [SerializeField] PlayerMovement playerMovement2;
    private KinectStateMachine player1StateMachine;
    private KinectStateMachine player2StateMachine;

    public Material BoneMaterial;
    public GameObject BodySourceManager;
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    void Start()
    {
        player1StateMachine = new KinectStateMachine();
        player2StateMachine = new KinectStateMachine();
    }

    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        //print(data.Length);
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                    if (body.Joints[Kinect.JointType.HandRight].Position.X < 0) //  右手座標在左邊
                    {
                        player1Id = body.TrackingId;
                        if (player1Id == player2Id)
                        {
                            player2Id = 0;
                        }
                    }
                    else
                    {
                        player2Id = body.TrackingId;
                        if (player1Id == player2Id)
                        {
                            player1Id = 0;
                        }
                    }
                }
                if (body.TrackingId == player1Id)
                {
                    //print(body.TrackingId);
                    player1StateMachine.CheckSwitchState(body, playerMovement1);
                    //attackDetection(body, playerMovement1, 1);
                    movementDetection(body, playerMovement1, 1);
                }
                if (body.TrackingId == player2Id)
                {
                    //print(body.TrackingId);
                    player2StateMachine.CheckSwitchState(body, playerMovement2);
                    //attackDetection(body, playerMovement2, 2);
                    movementDetection(body, playerMovement2, 2);
                }
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
                Kinect.Joint jointHandRight = body.Joints[Kinect.JointType.HandRight];
                print(jointHandRight.Position.X < 0);
            }
        }
    }
    private ulong player1Id;
    private ulong player2Id;
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 2, joint.Position.Y * 2 + 5, joint.Position.Z * 0-1);
    }

    private bool lastHigher = false;
    private bool lastLower = false;
    private bool rightHigher = false;
    private bool rightLower = true;
    private bool attackState = false;
    

    private void attackDetection(Kinect.Body body, PlayerMovement player, int playerNum)
    {
        if (playerNum == 1)
        {
            // Player1
            if(player1StateMachine.OverHand)
                player.OnKinectSwinDown();
            if(player1StateMachine.UnderHand)
                player.OnKinectSwinUp();
        }
        else
        {
            // Player2
            if (player2StateMachine.OverHand)
                player.OnKinectSwinDown();
            if (player2StateMachine.UnderHand)
                player.OnKinectSwinUp();
        }
    }

    private static double frontEdge = 1.0, backEdge = 1.5; // 實際的
    private static double realRange = backEdge - frontEdge;

    private static double playerLeft = 0.5;
    private static double playerRight = 7.0;

    //private static double courtRange = rightEdge; // 遊戲場景的
    private void movementDetection(Kinect.Body body, PlayerMovement player, int playerNum)
    {
        double bodyPos = (body.Joints[Kinect.JointType.HipRight].Position.Z + body.Joints[Kinect.JointType.HipLeft].Position.Z) / 2;

        Mathf.Clamp((float)bodyPos, (float)frontEdge, (float)backEdge);
        double playerPos;
        if (playerNum == 1)
        {
            playerPos = -1 * Mathf.Lerp((float)playerLeft, (float)playerRight, (float)((bodyPos - frontEdge) / (backEdge - frontEdge)));
        }
        else
        {
            playerPos = 1 * Mathf.Lerp((float)playerLeft, (float)playerRight, (float)((bodyPos - frontEdge) / (backEdge - frontEdge)));
        }
            // TODO: 把 playerPos 回傳給指定的 PlayerMovemont player
        player.OnKinectPositionMapping(playerPos);
    }

    private void jumpDetection(Kinect.Body body)
    {

    }
}