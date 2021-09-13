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
        this.lineRenderer = this.GetComponent<LineRenderer>(); // Line Renderer'ý bulup deðiþkenin içine atýlýyor.
        Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Ýpin baþlangýç pozisyonu mouse'un pozisyonu olarak atanýyor.

        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint)); // ropeSegments listesine RopeSegment Struct'ý örneklenerek ekleniyor ve RopeSegment Struct'ýnýn Constructor'u sayesinde posNew ve posOld ropeStartPoint deðiþkenine atanýyor.
            ropeStartPoint.y -= ropeSegLen; // ropeStartPoint deðiþkeninden ropeSegLen çýkartýlarak ipin aþþaðý doðru oluþmasýný saðlyor.
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
        Vector2 forceGravity = new Vector2(0f, -1.5f); // 2 boyutlu yer çekimi ayarlanýyor.

        for (int i = 1; i < this.segmentLength; i++) // Ýpin bütün parçalarýnýn simüle edilmesi için ipin segmentleri uzunluðýnda bir for döngüsü açýlýyor.
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            print(firstSegment);
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld; // Verlet Integration Part 1 - newPos = curPos + [(curPos - oldPos)] -> Part 1 (Hýzýný bulmak için kullanýlýyor. Daha sonra yeni pozisyonu bulmak için þimdiki pozisyona eklenecek.)
            firstSegment.posOld = firstSegment.posNow; // Yeni pozisyonu eski pozsiyon olarak atanýyor. Çünkü hýzý bulduðumuz için daha sonra yeni pozisyonun üstüne ekleyip þimdili pozisyonu bulabiliriz.
            firstSegment.posNow += velocity; // Verlet Integration Part 2 - Yeni pozisyonu bulmak için üst tarafta bulduðumuz hýzý þimdiki pozisyona ekleyip yeni pozisyonu buluyoruz.
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime; // Ýp segmentine yer çekimi uygulayarak aþþaðý doðru düþmesini saðlýyoruz.
            this.ropeSegments[i] = firstSegment; // Deðiþtirilen firstSegment yeni deðerleri ile segment dizisine ekleniyor ve simulasyon gerçekleþtiriliyor.
            
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
        RopeSegment firstSegment = this.ropeSegments[0]; // Ýpin ilk segmentini ayrý bir deðiþkene referans olarak býrakýlýr.
        firstSegment.posNow = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ilk segmentin posizyonu her zaman mouse pozisyonuna sabitleniyor.
        this.ropeSegments[0] = firstSegment; // Kýsýtlanan deðerler ipin ilk segmentine geri atanýyor.

        for (int i = 0; i < this.segmentLength - 1; i++) // For döngüsü segment uzunluðu - 1 þeklinde baþlatýlýr çünkü son segmentten sonra bir segment yoktur bu yüzden aradaki uzunluk hesaplanamaz.
        {
            RopeSegment firstSeg = this.ropeSegments[i]; // Sýradaki segmenti yeni bir deðiþkene atýlýyor.
            RopeSegment secondSeg = this.ropeSegments[i + 1]; // Sýradaki segmentten sonraki segment yeni bir deðiþkene atýlýyor.

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude; // FirstSegment'in þimdiki pozisyonundan SecondSegment'in þimdiki pozisyonu çýkartýlarak magnitude ile vektör uzunluðu bulunur.
            float error = Mathf.Abs(dist - this.ropeSegLen); // Bulunan vektör uzunluðunun hata payýný bulmak için vektör uzunluðundan bir ip segmentinin uzunluðu çýkartýlýr ve mutlak deðeri alýnýr. (Hata Payý : FirstSegmentin uzunluðu distance olarak hesaplanmayacaðý için çýkartýlýr.)
            Vector2 changeDir = Vector2.zero; // Yön deðiþtirmek için bir Vector2 deðiþkeni açýlýr ve 0'a atanýr.

            if (dist > ropeSegLen) // Eðer vektör uzunluðu bir ip segmentinden (0.25f(default)) büyükse; (bir sonraki segmentin arkasýnda veya önünde kaldýðýný anlamak için)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized; // ikinci segment arkasýnda olduðu için ikinci segmentin ilk segmente yönü hesaplanýr ve normal vektöre çevrilir.
            }
            else if (dist < ropeSegLen) // Eðer vektör uzunluðu bir ip segmentinden (0.25f(default)) küçükse; (bir sonraki segmentin arkasýnda veya önünde kaldýðýný anlamak için)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized; // ikinci segment önünde olduðu için ilk segmentin ikinci segmente yönü hesaplanýr ve normal vektöre çevrilir.
            }

            Vector2 changeAmount = changeDir * error; // Hesaplanan segment yönü ile hata payý çýkarýlmýþ vektör uzunluðu çarpýlýp segmentlerin þimdiki pozisyonlarýna eklenip ipin sallanma etkisini yaratacak deðer bulunur.
            if (i != 0) // Eðer ilk segment deðilse
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;

                //Bulunan sallantý deðeri ilgili segmentlerin pozisyonuna eklenir ve ropeSegments dizisindeki ilgili elemanlara atanýr.
            }
            else // Eðer ilk segmentse
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;

                // Bulunan sallantý deðeri sadece ikinci segmente eklenir. Çünkü ilk segmentin pozisyonu hep sabit.;
            }
        }
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth; 
                                                 // 2 satýrda LineRenderer'ýn baþlangýç ve bitiþ uzunluklarý belirleniyor.
        lineRenderer.endWidth = lineWidth / 2; 

        Vector3[] ropePositions = new Vector3[this.segmentLength]; // Ýpin her bir segmentinin pozisyonu için ropePositions adýnda yeni bir vector3 dizisi açýlýyor ve segmentLength uzunluðuna (35) atanýyor.
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow; // LineRender'ýn ipin pozisyonlarýný taklit etmesi için her bir rope segmentinin þimdiki pozisyonu sýralý bir þekilde ropePositions elemanlarýna aktarýlýyor.
        }

        lineRenderer.positionCount = ropePositions.Length; // LineRenderer ropePositions.Length kadar parçaya bölünüyor ve böylece her pozisyona bir nokta düþüyor.
        lineRenderer.SetPositions(ropePositions); // LindeRenderer'ýn her parçasý ropePositions listesindeki ilgili pozisyonlara atanýyor.
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
