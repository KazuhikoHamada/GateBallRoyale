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

    public Vector2 Position { get { return transform.position; } set { transform.position = value; } }

    private Vector2 _beginPosition;

    private bool _isShot = false;


    private float _preveMagnitude = 0f;

    private float _maxMagnitude = 0f;

    private float _stopMagnitude = 0.7f;

    private float _delaySeconds = 3.0f;

    private float _power = 5f;

    private IDisposable _disposable = null;

    private void Start()
    {
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

        _beginPosition = Position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");



    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isShot) return;

        //Debug.Log($"IDisposable:{_disposable}");

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(eventData.position);

        var direction = (targetPosition - _beginPosition).normalized * -1f;

        var distance = Vector2.Distance(targetPosition, _beginPosition);

        Vector2 power = direction * distance * _power;

        _preveMagnitude = (power / _rb.mass).magnitude;

        Debug.Log($"OnEndDrag. {_preveMagnitude}");

        _rb.AddForce(power, ForceMode2D.Impulse);

        //_rb.AddForce(direction * distance * 10f);

        _isShot = true;


        _disposable?.Dispose();
        _disposable = null;

        _disposable = GetMoveObservable();

    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
        _disposable = null;
    }

}
