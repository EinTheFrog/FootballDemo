using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class BallBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float forceTimeMult = 2.0f;
    [SerializeField] private float forceExpStart = 4.0f;
    [SerializeField] private Vector3 defaultPosition = Vector3.zero;
    [SerializeField] private float maxKickDistance = 1.0f;
    [SerializeField] private Image forcePanel;
    [SerializeField] private GoalKeeperBehaviour goalKeeper;
    [SerializeField] private String fieldTag;
    [SerializeField] private float minForce = 20.0f;
    [SerializeField] private float maxForce = 400.0f;

    public bool isValid = true;

    private Rigidbody _rigidbody;
    private float _holdStartTime;
    private Camera _camera;
    private bool _isKicked;
    private static readonly int ShaderDeltaTime = Shader.PropertyToID("_DeltaTime");
    private static readonly int ShaderForceMult = Shader.PropertyToID("_ForceMult");
    private static readonly int ShaderForceExpStart = Shader.PropertyToID("_ForceExpStart");
    private static readonly int ShaderMinForce = Shader.PropertyToID("_MinForce");
    private static readonly int ShaderMaxForce = Shader.PropertyToID("_MaxForce");

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _camera = FindObjectOfType<Camera>();
        _isKicked = false;
        forcePanel.material.SetFloat(ShaderForceMult, forceTimeMult);
        forcePanel.material.SetFloat(ShaderForceExpStart, forceExpStart);
        forcePanel.material.SetFloat(ShaderMinForce, minForce);
        forcePanel.material.SetFloat(ShaderMaxForce, maxForce);
        forcePanel.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        bool canKick = CheckIfCanKick();
        if (!canKick) return;
        
        _holdStartTime = Time.time;
        forcePanel.gameObject.SetActive(true);
        
        //Debug.Log("BallBehaviour: pointer down");
    }

    private void Update()
    {
        float dTime = Time.time - _holdStartTime;
        forcePanel.material.SetFloat(ShaderDeltaTime, dTime);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        bool canKick = CheckIfCanKick();
        if (!canKick) return;
        
        Vector3 clickPos = eventData.pointerPressRaycast.worldPosition;
        float force = CalculateKickForce(_holdStartTime);
        KickBall(clickPos, force);
        forcePanel.gameObject.SetActive(false);
        
        //Debug.Log($"BallBehaviour: pointer up (force = {force})");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(fieldTag))
        {
            goalKeeper.StopJumping();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag(fieldTag) && _isKicked)
        {
            goalKeeper.StartJumping();
        }
    }

    private bool CheckIfCanKick()
    {
        Vector3 ballPos = transform.position;
        Vector3 cameraPos = _camera.transform.position;
        float distance = (ballPos - cameraPos).magnitude;
        return distance <= maxKickDistance;
    }
    
    private float CalculateKickForce(float holdStartTime)
    {
        float holdEndTime = Time.time;
        float dTime = holdEndTime - holdStartTime;
        float force = dTime * forceTimeMult;
        force = Mathf.Exp(force + forceExpStart);
        force = Mathf.Clamp(force, minForce, maxForce);
        return force;
    }
    
    private void KickBall(Vector3 clickPos, float kickForce)
    {
        _isKicked = true;
        Vector3 centerPos = transform.position;
        Vector3 direction = centerPos - clickPos;
        direction.Normalize();
        _rigidbody.AddForce(direction * kickForce);
        goalKeeper.CatchBall(centerPos, direction);
    }

    public void ResetBall()
    {
        ResetRigidbody();
        transform.position = defaultPosition;
        isValid = true;
        _isKicked = false;
    }

    private void ResetRigidbody()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
}
