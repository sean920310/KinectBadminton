using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class AttackStateMachine
{
    public enum States
    {
        Idle,
        RightHandOverHead,
        RightHandUnderSpineMid,
        RightHandSwinDown, // under hand
        RightHandSwinUp, // over hand
        //TODO: Add State
    }

    private States m_State;
    private bool m_overHand;
    private bool m_underHand;

    public States state { get => m_State; }
    public bool OverHand { get => m_overHand; }
    public bool UnderHand { get => m_underHand; }

    public void CheckSwitchState(Kinect.Body body, PlayerMovement player)
    {
        Kinect.Joint jointHandRight = body.Joints[Kinect.JointType.HandRight];
        Kinect.Joint jointSpineMid = body.Joints[Kinect.JointType.SpineMid];
        Kinect.Joint jointShoulderRight = body.Joints[Kinect.JointType.ShoulderRight];
        Kinect.Joint jointElbowRight = body.Joints[Kinect.JointType.ElbowRight];
        Kinect.Joint jointHead = body.Joints[Kinect.JointType.Head];
        switch (m_State)
        {
            case States.Idle:
                //TODO: On Idle 
                if (jointHandRight.Position.Y > jointHead.Position.Y)
                {
                    m_State = States.RightHandOverHead;
                }
                if (jointHandRight.Position.Y < jointSpineMid.Position.Y)
                {
                    m_State = States.RightHandUnderSpineMid;
                }
                break;
            case States.RightHandOverHead:
                //TODO: On HandOverElbow 
                if (jointHandRight.Position.Y < jointHead.Position.Y)
                {
                    m_State = States.RightHandSwinUp; // over hand 
                }
                break;
            case States.RightHandUnderSpineMid:
                //TODO: On HandUnderElbow 
                if (jointHandRight.Position.Y > jointElbowRight.Position.Y)
                {
                    m_State = States.RightHandSwinDown; // under hand 
                }
                break;
            case States.RightHandSwinUp:
                player.OnKinectSwinUp();
                m_State = States.Idle;
                break;
            case States.RightHandSwinDown:
                player.OnKinectSwinDown();
                m_State = States.Idle;
                break;
            default:
                break;
        }
    }

}
