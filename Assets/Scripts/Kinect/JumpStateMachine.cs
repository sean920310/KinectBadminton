using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
public class JumpStateMachine
{
    public enum States
    {
        Idle,
        SquatDown,
        Jump,
    }

    private States m_State;
    private bool m_jump;

    public States state { get => m_State; }
    public bool OverHand { get => m_jump; }

    public void CheckSwitchState(Kinect.Body body, PlayerMovement player)
    {
        Kinect.Joint jointHipRight = body.Joints[Kinect.JointType.HipRight];
        Kinect.Joint jointKneeRight = body.Joints[Kinect.JointType.KneeRight];
        Kinect.Joint jointSpineMid = body.Joints[Kinect.JointType.SpineMid];
        float bodyProportion = (jointHipRight.Position.Y - jointKneeRight.Position.Y) / (jointSpineMid.Position.Y - jointHipRight.Position.Y);
        float hipKneeDis;
        hipKneeDis = jointHipRight.Position.Y - jointKneeRight.Position.Y;

        switch (m_State)
        {
            case States.Idle:
                //TODO: On Idle 
                if (hipKneeDis < 0.2f/*(jointSpineMid.Position.Y - jointHipRight.Position.Y) * bodyProportion*/)
                {
                    m_State = States.SquatDown;
                }
                break;
            case States.SquatDown:
                //TODO: On HandOverElbow 
                hipKneeDis = jointHipRight.Position.Y - jointKneeRight.Position.Y;
                if (hipKneeDis > 0.3f/*(jointSpineMid.Position.Y - jointHipRight.Position.Y) * bodyProportion*/)
                {
                    m_State = States.Jump;
                }
                break;
            case States.Jump:
                //TODO: On HandUnderElbow 
                player.OnKinectJump();
                m_State = States.Idle;
                break;
            default:
                break;
        }
    }
}