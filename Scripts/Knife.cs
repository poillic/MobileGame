using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Knife : MonoBehaviour
{
    [SerializeField] InputAction _pointerMove;
    [SerializeField] InputAction _pointerPress;
    [SerializeField] LayerMask _layerToCut;
    [SerializeField] TrailRenderer _trail;

    private Vector3 _lastFramePosition;
    private void OnEnable()
    {
        _pointerMove.Enable();
        _pointerPress.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        _trail.emitting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if( _pointerPress.WasPerformedThisFrame() )
        {
            transform.position = Camera.main.ScreenToWorldPoint( _pointerMove.ReadValue<Vector2>() ) + Vector3.forward * 10f;
        }

        // !_pointerPress.WasPerformedThisFrame() permet de décaler l'activation du TrailRenderer d'une frame pour éviter 
        // les grands traits dans l'écran
        if ( _pointerPress.IsPressed() && !_pointerPress.WasPerformedThisFrame() )
        {
            transform.position = Camera.main.ScreenToWorldPoint( _pointerMove.ReadValue<Vector2>() ) + Vector3.forward * 10f;
            _trail.emitting = true;

            Vector3 cutDirection = transform.position - _lastFramePosition;

            RaycastHit2D hit = Physics2D.Raycast( _lastFramePosition, cutDirection.normalized, cutDirection.magnitude, _layerToCut );

            if( hit.collider != null )
            {
                Destroy( hit.collider.gameObject );
            }
        }

        if( _pointerPress.WasReleasedThisFrame() )
        {
            _trail.emitting = false;
        }
    }

    private void LateUpdate()
    {
        _lastFramePosition = transform.position;
    }
}
