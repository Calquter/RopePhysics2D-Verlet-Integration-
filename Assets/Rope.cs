using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = .25f;
    private int segmentLength = 35;
    private float lineWidth = 0.1f;

    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>(); // Line Renderer'� bulup de�i�kenin i�ine at�l�yor.
        Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition); // �pin ba�lang�� pozisyonu mouse'un pozisyonu olarak atan�yor.

        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint)); // ropeSegments listesine RopeSegment Struct'� �rneklenerek ekleniyor ve RopeSegment Struct'�n�n Constructor'u sayesinde posNew ve posOld ropeStartPoint de�i�kenine atan�yor.
            ropeStartPoint.y -= ropeSegLen; // ropeStartPoint de�i�keninden ropeSegLen ��kart�larak ipin a��a�� do�ru olu�mas�n� sa�lyor.
        }
    }

    void Update()
    {
        this.DrawRope();
    }

    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void Simulate()
    {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1.5f); // 2 boyutlu yer �ekimi ayarlan�yor.

        for (int i = 1; i < this.segmentLength; i++) // �pin b�t�n par�alar�n�n sim�le edilmesi i�in ipin segmentleri uzunlu��nda bir for d�ng�s� a��l�yor.
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            print(firstSegment);
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld; // Verlet Integration Part 1 - newPos = curPos + [(curPos - oldPos)] -> Part 1 (H�z�n� bulmak i�in kullan�l�yor. Daha sonra yeni pozisyonu bulmak i�in �imdiki pozisyona eklenecek.)
            firstSegment.posOld = firstSegment.posNow; // Yeni pozisyonu eski pozsiyon olarak atan�yor. ��nk� h�z� buldu�umuz i�in daha sonra yeni pozisyonun �st�ne ekleyip �imdili pozisyonu bulabiliriz.
            firstSegment.posNow += velocity; // Verlet Integration Part 2 - Yeni pozisyonu bulmak i�in �st tarafta buldu�umuz h�z� �imdiki pozisyona ekleyip yeni pozisyonu buluyoruz.
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime; // �p segmentine yer �ekimi uygulayarak a��a�� do�ru d��mesini sa�l�yoruz.
            this.ropeSegments[i] = firstSegment; // De�i�tirilen firstSegment yeni de�erleri ile segment dizisine ekleniyor ve simulasyon ger�ekle�tiriliyor.
            
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        //Constrant to Mouse
        RopeSegment firstSegment = this.ropeSegments[0]; // �pin ilk segmentini ayr� bir de�i�kene referans olarak b�rak�l�r.
        firstSegment.posNow = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ilk segmentin posizyonu her zaman mouse pozisyonuna sabitleniyor.
        this.ropeSegments[0] = firstSegment; // K�s�tlanan de�erler ipin ilk segmentine geri atan�yor.

        for (int i = 0; i < this.segmentLength - 1; i++) // For d�ng�s� segment uzunlu�u - 1 �eklinde ba�lat�l�r ��nk� son segmentten sonra bir segment yoktur bu y�zden aradaki uzunluk hesaplanamaz.
        {
            RopeSegment firstSeg = this.ropeSegments[i]; // S�radaki segmenti yeni bir de�i�kene at�l�yor.
            RopeSegment secondSeg = this.ropeSegments[i + 1]; // S�radaki segmentten sonraki segment yeni bir de�i�kene at�l�yor.

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude; // FirstSegment'in �imdiki pozisyonundan SecondSegment'in �imdiki pozisyonu ��kart�larak magnitude ile vekt�r uzunlu�u bulunur.
            float error = Mathf.Abs(dist - this.ropeSegLen); // Bulunan vekt�r uzunlu�unun hata pay�n� bulmak i�in vekt�r uzunlu�undan bir ip segmentinin uzunlu�u ��kart�l�r ve mutlak de�eri al�n�r. (Hata Pay� : FirstSegmentin uzunlu�u distance olarak hesaplanmayaca�� i�in ��kart�l�r.)
            Vector2 changeDir = Vector2.zero; // Y�n de�i�tirmek i�in bir Vector2 de�i�keni a��l�r ve 0'a atan�r.

            if (dist > ropeSegLen) // E�er vekt�r uzunlu�u bir ip segmentinden (0.25f(default)) b�y�kse; (bir sonraki segmentin arkas�nda veya �n�nde kald���n� anlamak i�in)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized; // ikinci segment arkas�nda oldu�u i�in ikinci segmentin ilk segmente y�n� hesaplan�r ve normal vekt�re �evrilir.
            }
            else if (dist < ropeSegLen) // E�er vekt�r uzunlu�u bir ip segmentinden (0.25f(default)) k���kse; (bir sonraki segmentin arkas�nda veya �n�nde kald���n� anlamak i�in)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized; // ikinci segment �n�nde oldu�u i�in ilk segmentin ikinci segmente y�n� hesaplan�r ve normal vekt�re �evrilir.
            }

            Vector2 changeAmount = changeDir * error; // Hesaplanan segment y�n� ile hata pay� ��kar�lm�� vekt�r uzunlu�u �arp�l�p segmentlerin �imdiki pozisyonlar�na eklenip ipin sallanma etkisini yaratacak de�er bulunur.
            if (i != 0) // E�er ilk segment de�ilse
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;

                //Bulunan sallant� de�eri ilgili segmentlerin pozisyonuna eklenir ve ropeSegments dizisindeki ilgili elemanlara atan�r.
            }
            else // E�er ilk segmentse
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;

                // Bulunan sallant� de�eri sadece ikinci segmente eklenir. ��nk� ilk segmentin pozisyonu hep sabit.;
            }
        }
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth; 
                                                 // 2 sat�rda LineRenderer'�n ba�lang�� ve biti� uzunluklar� belirleniyor.
        lineRenderer.endWidth = lineWidth / 2; 

        Vector3[] ropePositions = new Vector3[this.segmentLength]; // �pin her bir segmentinin pozisyonu i�in ropePositions ad�nda yeni bir vector3 dizisi a��l�yor ve segmentLength uzunlu�una (35) atan�yor.
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow; // LineRender'�n ipin pozisyonlar�n� taklit etmesi i�in her bir rope segmentinin �imdiki pozisyonu s�ral� bir �ekilde ropePositions elemanlar�na aktar�l�yor.
        }

        lineRenderer.positionCount = ropePositions.Length; // LineRenderer ropePositions.Length kadar par�aya b�l�n�yor ve b�ylece her pozisyona bir nokta d���yor.
        lineRenderer.SetPositions(ropePositions); // LindeRenderer'�n her par�as� ropePositions listesindeki ilgili pozisyonlara atan�yor.
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}
