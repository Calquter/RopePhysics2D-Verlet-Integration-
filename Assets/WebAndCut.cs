using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebAndCut : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private List<WebPoints> _webPoints = new List<WebPoints>();
    private float _webLength = .35f;
    private int _pointLength = 20;
    private float _lineRendererWidth  = .1f;

    private Vector2 constPointOne;
    private Vector2 constPointTwo;
    
    void Start()
    {
        constPointOne = Vector2.zero;
        constPointOne = Vector2.zero;

        _lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for (int y = 0; y < _pointLength; y++)
        {
            _webPoints.Add(new WebPoints(startPoint));
            startPoint.y -= _webLength;
        }
    }


    void Update()
    {
        FixPositionsOfRope();
        DrawWeb();
    }

    private void FixedUpdate()
    {
        Simulating();
    }

    private void FixPositionsOfRope()
    {
        if (Input.GetMouseButtonDown(0))
            constPointOne = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else if (Input.GetMouseButtonDown(1))
            constPointTwo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else if (Input.GetMouseButtonDown(2))
        {
            constPointOne = Vector2.zero;
            constPointTwo = Vector2.zero;
        }
    }

    private void Simulating()
    {
        Vector2 gravitiy = new Vector2(0, -1f);

        for (int i = 0; i < _pointLength; i++)
        {
            WebPoints point = _webPoints[i];
            Vector2 velocity = _webPoints[i].curPos - _webPoints[i].oldPos;
            point.oldPos = point.curPos;
            point.curPos += velocity;
            point.curPos += gravitiy * Time.deltaTime;
            _webPoints[i] = point;
        }

        for (int i = 0; i < 100; i++)
        {
            Constrains();
        }

    }

    private void Constrains()
    {
        if (constPointOne == Vector2.zero)
        {
            WebPoints firstPoint = _webPoints[0];
            firstPoint.curPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _webPoints[0] = firstPoint;
        }
        else
        {
            WebPoints firstPoint = _webPoints[0];
            firstPoint.curPos = constPointOne;
            _webPoints[0] = firstPoint;
        }

        if (constPointTwo != Vector2.zero)
        {
            WebPoints firstPoint = _webPoints[_webPoints.Count - 1];
            firstPoint.curPos = constPointTwo;
            _webPoints[_webPoints.Count - 1] = firstPoint;
        }

        for (int i = 0; i < _pointLength - 1; i++)
        {
            WebPoints firstP = _webPoints[i];
            WebPoints secondP = _webPoints[i + 1];

            float distance = (firstP.curPos - secondP.curPos).magnitude;
            float error = Mathf.Abs(distance - _webLength);

            Vector2 changeDir = Vector2.zero;

            if (distance > _webLength)
            {
                changeDir = (firstP.curPos - secondP.curPos).normalized;
            }
            else if (distance < _webLength)
            {
                changeDir = (secondP.curPos - firstP.curPos).normalized;
            }

            Vector2 changeAmount = changeDir * error;

            if (i != 0)
            {
                firstP.curPos -= changeAmount * 0.5f;
                _webPoints[i] = firstP;
                secondP.curPos += changeAmount * 0.5f;
                _webPoints[i + 1] = secondP;
            }
            else
            {
                secondP.curPos += changeAmount * 0.5f;
                _webPoints[i + 1] = secondP;
            }
        }

    }


    private void DrawWeb()
    {
        _lineRenderer.startWidth = _lineRendererWidth;
        _lineRenderer.endWidth = _lineRendererWidth;

        Vector3[] webPoses = new Vector3[_pointLength];
        for (int i = 0; i < webPoses.Length; i++)
        {
            webPoses[i] = _webPoints[i].curPos;
        }
        _lineRenderer.positionCount = webPoses.Length;
        _lineRenderer.SetPositions(webPoses);
    }
}




public struct WebPoints
{
    public Vector2 curPos;
    public Vector2 oldPos;

    public WebPoints(Vector2 pos)
    {
        curPos = pos;
        oldPos = pos;
    }
}
