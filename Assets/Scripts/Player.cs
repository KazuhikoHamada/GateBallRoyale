using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Rigidbody2D _rb;

    public Vector2 Position { get { return transform.position; } set { transform.position = value; } }

    private Vector2 _beginPosition;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");

        _beginPosition = Position;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");



    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(eventData.position);

        var a = (targetPosition - _beginPosition).normalized * -1f;

        _rb.AddForce(a * 10f, ForceMode2D.Impulse);
    }


}
