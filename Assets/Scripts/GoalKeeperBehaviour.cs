using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GoalKeeperBehaviour : MonoBehaviour
{
    [SerializeField] private Vector2 goalCenter;
    [SerializeField] private float maxProtectingRadius;
    [SerializeField] private float idleRadius;
    [SerializeField] private float idleSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float protectingRadius;
    [SerializeField] private float protectingSpeed;
    [SerializeField] private float changePositionSpeed;
    [SerializeField] private string fieldTag;
    [SerializeField] private Vector3 initialDirection;

    private Rigidbody _rigidbody;
    private bool _jumping;
    private bool _inAir;
    private Vector2 _posToProtect;
    private State _curState;
    private Vector3 _oldVelocity;
    
    private enum State
    {
        Idle, Running, Protecting
    }
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _posToProtect = goalCenter;
        Vector3 curPos = transform.position;
        transform.position = new Vector3(_posToProtect.x, curPos.y, _posToProtect.y);
        _jumping = false;
        _inAir = true;
        _curState = State.Idle;
        _oldVelocity = initialDirection * idleSpeed;
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        switch (_curState)
        {
            case State.Idle:
                ProtectPos(goalCenter, idleRadius, idleSpeed);
                break;
            case State.Running:
                RunToPose(_posToProtect);
                break;
            case State.Protecting:
                ProtectPos(_posToProtect, protectingRadius, protectingSpeed);
                break;
        }
        if (_jumping)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (!_inAir)
        {
            _rigidbody.velocity += jumpSpeed * Vector3.up;
            _inAir = true;
        }
    }

    private void ProtectPos(Vector2 destv2, float curRadius, float curSpeed)
    {
        if (_inAir) return;
        Vector3 curPos = transform.position;
        Vector3 destv3 = new Vector3(destv2.x, curPos.y, destv2.y);
        Vector3 dir = destv3 - curPos;
        float bestSpeed = curSpeed;
        
        if (_jumping)
        {
            float s = 2 * curRadius;
            const float g = 9.8f;
            bestSpeed = (s * g) / (2 * jumpSpeed);
            _oldVelocity = -_oldVelocity.normalized * bestSpeed;
        }
        else if (dir.magnitude > curRadius)
        {
            Vector3 newVelocity = dir.normalized * bestSpeed;
            newVelocity.y = 0;
            _oldVelocity = newVelocity;
        }
        _rigidbody.velocity = _oldVelocity;

        
    }

    private void RunToPose(Vector2 destv2)
    {
        if (_inAir) return;
        
        Vector3 curPos = transform.position;
        Vector3 destv3 = new Vector3(destv2.x, curPos.y, destv2.y);
        Vector3 dir = destv3 - curPos;

        float bestSpeed = changePositionSpeed;
        float framePath = changePositionSpeed * Time.deltaTime;
        float s = dir.magnitude;
        if (_jumping)
        {
            const float g = 9.8f;
            bestSpeed = (s * g) / (2 * jumpSpeed);
        }

        if (s > framePath)
        {
            Vector3 velocity = dir.normalized * bestSpeed;
            velocity.y = 0;
            _rigidbody.velocity = velocity;
        }
        else
        {
            _oldVelocity = initialDirection * idleSpeed;
            _curState = State.Protecting;
        }
    }

    private Vector2 CalcBallDest(Vector3 ballPos, Vector3 ballDir)
    {
        float goalKeeperZ = transform.position.z;
        float ballZ = ballPos.z;
        float distance = Mathf.Abs(ballZ - goalKeeperZ);
        float zSign = ballZ - goalKeeperZ > 0 ? -1 : 1;
        Vector2 perpendicular = new Vector2(0, zSign);
        Vector2 ballDirv2 = new Vector2(ballDir.x, ballDir.z);
        float cos = Vector2.Dot(perpendicular, ballDirv2.normalized);
        float sin = Mathf.Sqrt(1 - cos * cos);
        float tg = sin / cos;
        float xSign = ballDirv2.x > 0 ? 1.0f : -1.0f;
        float ballFinalX = xSign *distance * tg + ballPos.x;
        return new Vector2(ballFinalX, _posToProtect.y);
    }
    public void CatchBall(Vector3 ballPos, Vector3 ballDir)
    {
        Vector2 ballDest = CalcBallDest(ballPos, ballDir);
        float distFromCenter = Vector3.Magnitude(ballDest - goalCenter);
        if (distFromCenter < maxProtectingRadius)
        {
            _posToProtect = ballDest;
            _curState = State.Running;
        }
        else
        {
            _posToProtect = goalCenter;
            _curState = State.Idle;
        }
    }

    public void StartJumping()
    {
        _jumping = true;
    }
    
    public void StopJumping()
    {
        _jumping = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(fieldTag))
        {
            _inAir = false;
        }
    }

    public void SetIdle()
    {
        _curState = State.Idle;
        _jumping = false;
    }
}
