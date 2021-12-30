using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using System;

public class Player : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private LineRenderer _direction = null;

    public Vector2 Position { get { return transform.position; } set { transform.position = value; } }

    private bool _isShot = false;

    private float _preveMagnitude = 0f;

    private float _maxMagnitude = 1f;

    private float _stopMagnitude = 1.0f;

    private float _delaySeconds = 3.0f;

    private float _power = 5.0f;

    private IDisposable _disposable = null;

    private Vector2 _force;

    private void Start()
    {
        _direction.enabled = false;

        this.OnCollisionEnter2DAsObservable().Where(_ => _disposable == null).Subscribe(_ => 
        {
            _disposable = GetMoveObservable();

        }).AddTo(this);
    }

    private IDisposable GetMoveObservable()
    {
        return Observable.EveryFixedUpdate().DelaySubscription(TimeSpan.FromSeconds(_delaySeconds)).TakeWhile(_ => IsMoved()).Subscribe(_ =>
        {
            Debug.Log($"magnitude:{_rb.velocity.magnitude}, {_preveMagnitude - _rb.velocity.magnitude}");

            _preveMagnitude = _rb.velocity.magnitude;

        }, () =>
        {
            Debug.Log($"complete.");

            _preveMagnitude = 0f;

            _rb.velocity = Vector2.zero;

            _isShot = false;

            _disposable?.Dispose();
            _disposable = null;
        });
    }

    private bool IsMoved()
    {
        // falseになるとストップする
        Debug.Log($"_preveMagnitude:{_preveMagnitude}, _rb.velocity.magnitude:{_rb.velocity.magnitude}, {_preveMagnitude - _rb.velocity.magnitude}");

        //return _preveMagnitude - _rb.velocity.magnitude >= _stopMagnitude;
        return _rb.velocity.magnitude >= _stopMagnitude;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("OnPointerClick");

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag");

        _direction.enabled = true;
        _direction.SetPosition(0, Position);
        _direction.SetPosition(1, Position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");

        Vector2 targetPosition = GetMousePosition(eventData);

        _force = Position - targetPosition;

        if (_force.magnitude > _maxMagnitude * _maxMagnitude)
        {
            _force *= _maxMagnitude / _force.magnitude; 
        }

        var direction = Position + _force;

        _direction.SetPosition(0, new Vector3(Position.x, Position.y, Vector3.back.z));
        _direction.SetPosition(1, new Vector3(direction.x, direction.y, Vector3.back.z));

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isShot) return;

        _direction.enabled = false;

        Vector2 power = _force * _power;
        
        _preveMagnitude = (power / _rb.mass).magnitude;

        Debug.Log($"OnEndDrag. {_preveMagnitude}");

        _rb.AddForce(power, ForceMode2D.Impulse);

        _isShot = true;

        _disposable?.Dispose();
        _disposable = null;

        _disposable = GetMoveObservable();

    }

    private Vector2 GetMousePosition(PointerEventData eventData)
    {
        return Camera.main.ScreenToWorldPoint(eventData.position);
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
        _disposable = null;
    }

}
