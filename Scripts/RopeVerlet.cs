using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( LineRenderer ) )]
[RequireComponent( typeof( EdgeCollider2D ) )]
[RequireComponent( typeof( DistanceJoint2D ) )]
[RequireComponent( typeof( FixedJoint2D ) )]
public class RopeVerlet : MonoBehaviour
{
    /* README
     * 
     * DistanceJoint : Doit etre connecté à l'objet du début de la corbe
     * FixedJoint : Connecté à l'objet de fin de la corde
     * 
     */

    #region Exposed
    [Tooltip("Longueur de la corde")]
    [SerializeField] private float _ropeLength = 5f;
    [Tooltip("Longueur des segments de la corde, plus les segments sont petits plus ça va couter cher en calcul")]
    [SerializeField] private float _ropeSegmentLength = 0.25f;
    [Tooltip("Epaisseur de la corde et de son collider")]
    [SerializeField] private float _ropeWidth = 0.25f;
    private int _ropeSegmentQuantity = 35;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        Init();
    }

    void Update()
    {
        DrawRope();
    }

    void FixedUpdate()
    {
        Simulate();
    }

    #endregion

    #region Main Methods

    private void Init()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startWidth = _ropeWidth;
        _lineRenderer.endWidth = _ropeWidth;

        _rb2d = GetComponent<Rigidbody2D>();
        _rb2d.bodyType = RigidbodyType2D.Dynamic;

        _distance = GetComponent<DistanceJoint2D>();
        _distance.autoConfigureDistance = false;
        _distance.maxDistanceOnly = true;
        _distance.anchor = Vector2.zero;
        _distance.connectedAnchor = Vector2.zero;
        _distance.distance = _ropeLength;

        _fixed = GetComponent<FixedJoint2D>();
        _fixed.autoConfigureConnectedAnchor = false;
        _fixed.anchor = Vector2.zero;
        _fixed.connectedAnchor = Vector2.zero;

        _edgeCollider = GetComponent<EdgeCollider2D>();
        _edgeCollider.edgeRadius = _ropeWidth / 2f;

        _ropeSegmentQuantity = Mathf.FloorToInt( _distance.distance / _ropeSegmentLength );

        if ( _ropeSegmentQuantity == 0 )
        {
            throw new System.Exception( "Vous devez décocher 'Auto Configure Distance' et configurer la Distance de la Distance Joint. " );
        }

        Vector3 ropeStartPoint = transform.position;

        for ( int i = 0; i < _ropeSegmentQuantity; i++ )
        {
            _ropeSegments.Add( new RopeSegment( ropeStartPoint ) );
            ropeStartPoint.y -= _ropeSegmentLength;

        }
    }

    private void DrawRope()
    {
        if ( _ropeSegmentQuantity == 0 ) return;

        Vector3[] ropePosition = new Vector3[ _ropeSegmentQuantity ];
        List<Vector2> ropeCollider = new List<Vector2>();

        for ( int i = 0; i < _ropeSegmentQuantity; i++ )
        {
            ropePosition[ i ] = _ropeSegments[ i ].posNow;
            ropeCollider.Add( transform.InverseTransformPoint( _ropeSegments[ i ].posNow ) );
        }

        _lineRenderer.positionCount = _ropeSegments.Count;
        _lineRenderer.SetPositions( ropePosition );
        _edgeCollider.SetPoints( ropeCollider );
    }

    private void Simulate()
    {
        if ( _ropeSegmentQuantity == 0 ) return;

        for ( int i = 0; i < _ropeSegmentQuantity; i++ )
        {
            RopeSegment currentSegment = _ropeSegments[ i ];
            Vector2 velocity = currentSegment.posNow - currentSegment.posOld;
            currentSegment.posOld = currentSegment.posNow;
            currentSegment.posNow += velocity;
            currentSegment.posNow += Physics2D.gravity * Time.deltaTime;
            _ropeSegments[ i ] = currentSegment;
        }

        for ( int i = 0; i < _constraintCalculation; i++ )
        {
            ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        RopeSegment firstSegment = _ropeSegments[ 0 ];
        firstSegment.posNow = _distance.connectedBody.transform.position;
        _ropeSegments[ 0 ] = firstSegment;

        RopeSegment lastSegment = _ropeSegments[ _ropeSegments.Count - 1 ];
        lastSegment.posNow = _fixed.connectedBody.transform.position;
        _ropeSegments[ _ropeSegments.Count - 1 ] = lastSegment;

        for ( int i = 0; i < _ropeSegmentQuantity - 1; i++ )
        {
            RopeSegment currentSegment = _ropeSegments[ i ];
            RopeSegment nextSegment = _ropeSegments[ i + 1 ];

            float dist = ( currentSegment.posNow - nextSegment.posNow ).magnitude;
            float error = Mathf.Abs( dist - _ropeSegmentLength );

            Vector2 changeDir = Vector2.zero;

            if ( dist > _ropeSegmentLength )
            {
                changeDir = ( currentSegment.posNow - nextSegment.posNow ).normalized;
            }
            else if ( dist < _ropeSegmentLength )
            {
                changeDir = ( nextSegment.posNow - currentSegment.posNow ).normalized;
            }

            Vector2 changeAmount = changeDir * error;

            if ( i != 0 )
            {
                currentSegment.posNow -= changeAmount;
                _ropeSegments[ i ] = currentSegment;
                nextSegment.posNow += changeAmount;
                _ropeSegments[ i + 1 ] = nextSegment;
            }
            else
            {
                nextSegment.posNow += changeAmount;
                _ropeSegments[ i + 1 ] = nextSegment;
            }
        }
    }

    #endregion

    #region Privates & Protected

    private LineRenderer _lineRenderer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();
    private DistanceJoint2D _distance;
    private FixedJoint2D _fixed;
    private Rigidbody2D _rb2d;
    private int _constraintCalculation = 20;
    private EdgeCollider2D _edgeCollider;

    #endregion

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment( Vector2 pos )
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }

}