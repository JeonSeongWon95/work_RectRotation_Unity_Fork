using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GridBrushBase;
using static UnityEngine.Rendering.ProbeTouchupVolume;

public class HexagonRotation : MonoBehaviour, MoveAndRotateInterface
{
    public enum CornerName
    {
        LB, // LeftBottom (0)
        LM, // LeftMiddle (1)
        LT, // LeftTop (2)
        RT, // RightTop (3)
        RM, // RightMiddle (4)
        RB  // RightBottom (5)
    }

    [Header("Property")]
    [SerializeField]
    [Tooltip("������ �Ѻ��� ����")]
    float length = 100f;
    [SerializeField]
    Image ownerImage = null;
    [SerializeField]
    Image leftBottomCorner = null;
    [SerializeField]
    Image rightBottomCorner = null;
    [SerializeField]
    Image rightMiddleCorner = null;
    [SerializeField]
    Image leftMiddleCorner = null;
    [SerializeField]
    Image leftTopCorner = null;
    [SerializeField]
    Image rightTopCorner = null;

    [SerializeField]
    [Tooltip("����Ŭ���̵� ��� �ߴ� ��")]
    Image cycloidPoint = null;
    [SerializeField]
    [Tooltip("ȸ�� �߽� ��")]
    Image pivotPoint = null;
    [SerializeField]
    [Tooltip("�ٴ� �浹 ��")]
    Image touchPoint = null;

    [SerializeField]
    [Tooltip("�ʴ� ȸ�� ����")]
    float speed = 10f;

    [Header("Status")]
    [SerializeField]
    [Tooltip("���� ��ġ")]
    Vector3 startPosition = Vector3.zero;
    [SerializeField]
    [Tooltip("�� ��ġ")]
    Vector3 curPosition = Vector3.zero;

    [SerializeField]
    [Tooltip("�� ȸ������")]
    float totalRotation = 0f;
    [SerializeField]
    float prevRotation = 0f;

    [Header("Properties")]
    [SerializeField]
    [Tooltip("�ʴ� ȸ�� ���� (speed)")]
    float rotationAnglePerSecond = 0f;
    [SerializeField]
    [Tooltip("ȸ�� ���� ( true : right(��) , false : left(��) )")]
    bool rotationDirection = true;
    bool prevRotationDirection = true;

    [Header("Statuses")]
    [SerializeField]
    [Tooltip("���� ȸ����")]
    float curRotation = 0f;
    [SerializeField]
    int cycloidPos = 0;
    [SerializeField]
    int pivotPos = 5;
    [SerializeField]
    int touchPos = 4;

    Image[] corners = new Image[6];
    Shape shape;

    void Start()
    {
        // �ʿ��� ������Ʈ���� �����Ǿ� �ִ��� Ȯ��
        if (ownerImage == null ||
            leftBottomCorner == null ||
            rightBottomCorner == null ||
            rightMiddleCorner == null ||
            leftMiddleCorner == null ||
            rightTopCorner == null ||
            leftTopCorner == null)
        {
            throw new System.Exception("��ü�� �������� �ʾҽ��ϴ�.");
        }

        // Status �ʱ�ȭ
        startPosition = ownerImage.rectTransform.localPosition;
        curPosition = startPosition;
        curRotation = ownerImage.rectTransform.eulerAngles.z;
        Debug.Log("A Cur : " + curRotation + ", Prev : " + prevRotation + ", Total : " + totalRotation);

        // �ڳ� �迭�� ������ LB, LM, LT, RT, RM, RB �� ����
        corners[0] = leftBottomCorner;  // LB (240��)
        corners[1] = leftMiddleCorner;  // LM (180��)
        corners[2] = leftTopCorner;     // LT (120��)
        corners[3] = rightTopCorner;    // RT (60��)
        corners[4] = rightMiddleCorner; // RM (0��)
        corners[5] = rightBottomCorner; // RB (300��)

        rotationAnglePerSecond = 1f;

        shape = GetComponent<Shape>();
    }

    // ���� ȸ������ 60���� ����� �����ϴ� �Լ� (������ ����)
    float InterpolationAngle(float angle)
    {
        float snapInterval = 60f;
        // ���� angle���� ���� ����� 60�� ����� ��ǥ ������ ���
        float targetAngle = Mathf.Round(angle / snapInterval) * snapInterval;
        // ���� �ӵ� (���ϴ� �ε巯�� ���� ���� ����)
        float smoothingFactor = 10f;
        return Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * smoothingFactor);
    }

    int ModuloOperatorRight(int curValue)
    {
        int nextValue = (curValue - 1 + 6) % 6;
        return nextValue;
    }

    int ModuloOperatorLeft(int curValue)
    {
        int nextValue = (curValue + 1) % 6;
        return nextValue;
    }

    public void MoveAndRotate(int sensorDist)
    {
        // ���� ���� ���� ȸ�� ���� ����
        if (sensorDist < 0)
        {
            rotationDirection = false;
        }
        else if (sensorDist > 0)
        {
            rotationDirection = true;
        }

        // ȸ�� ������ �ٲ���� �� ��ġ ����Ʈ�� ��ġ�� ����
        if (prevRotationDirection != rotationDirection)
        {
            if (rotationDirection == true)
            {
                // ������ ȸ���� ���: ��ġ ����Ʈ = pivot���� �� ĭ ���� (���� ����)
                touchPos = ModuloOperatorRight(pivotPos);
            }
            else
            {
                // ���� ȸ���� ���: ��ġ ����Ʈ = pivot���� �� ĭ ������
                touchPos = ModuloOperatorLeft(pivotPos);
            }
            touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;
        }

        // ȸ�� ó��
        if (rotationDirection)
        {
            if (shape != null && shape.AutoMove == true)
            {
                ownerImage.rectTransform.RotateAround(pivotPoint.rectTransform.position, Vector3.back, Mathf.Abs(rotationAnglePerSecond) * Time.deltaTime * sensorDist * 50);
            }
            else
            {
                ownerImage.rectTransform.RotateAround(pivotPoint.rectTransform.position, Vector3.back, Mathf.Abs(rotationAnglePerSecond) * sensorDist * 50);
            }

            // ��ġ ����Ʈ�� �ǹ� ����Ʈ�� ���� ��ġ���� �Ʒ��� �������� ȸ�� ���� �� �ǹ�/��ġ ��ġ ����
            if (touchPoint.transform.position.y <= pivotPoint.transform.position.y)
            {
                curRotation = ownerImage.rectTransform.rotation.eulerAngles.z;
                transform.rotation = Quaternion.Euler(0, 0, InterpolationAngle(curRotation));

                pivotPos = ModuloOperatorRight(pivotPos);
                touchPos = ModuloOperatorRight(touchPos);

                pivotPoint.rectTransform.localPosition = corners[pivotPos].rectTransform.localPosition;
                touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;

                Vector3 ResetPosition = ownerImage.transform.localPosition;
                ResetPosition.y = 0;
                ResetPosition.z = 0;
                ownerImage.transform.localPosition = ResetPosition;
            }
        }
        else
        {
            if (shape != null && shape.AutoMove == true)
            {
                ownerImage.rectTransform.RotateAround(pivotPoint.rectTransform.position, Vector3.forward, Mathf.Abs(rotationAnglePerSecond) * Time.deltaTime * sensorDist * 50);
            }
            else
            {
                ownerImage.rectTransform.RotateAround(pivotPoint.rectTransform.position, Vector3.forward, Mathf.Abs(rotationAnglePerSecond) * sensorDist * 50);
            }

            if (touchPoint.transform.position.y <= pivotPoint.transform.position.y)
            {
                pivotPos = ModuloOperatorLeft(pivotPos);
                touchPos = ModuloOperatorLeft(touchPos);
                pivotPoint.rectTransform.localPosition = corners[pivotPos].rectTransform.localPosition;
                touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;
                Vector3 ResetPosition = ownerImage.transform.localPosition;
                ResetPosition.y = 0;
                ResetPosition.z = 0;
                ownerImage.transform.localPosition = ResetPosition;
            }
        }

        curRotation = ownerImage.rectTransform.rotation.eulerAngles.z;
        prevRotationDirection = rotationDirection;
    }

    public void InitPivotPoint()
    {
        // �ʿ�� pivot, cycloid, touch ����Ʈ�� �ʱ� ��ġ�� �����մϴ�.

        cycloidPos = (int)CornerName.LB;
        pivotPos = (int)CornerName.RB;
        touchPos = (int)CornerName.RM;

        cycloidPoint.rectTransform.localPosition = corners[cycloidPos].rectTransform.localPosition;
        pivotPoint.rectTransform.localPosition = corners[pivotPos].rectTransform.localPosition;
        touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;
    }
}
