using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RectRottation : MonoBehaviour
{
    public enum CornerName
    {
        LB, // LeftBottom (0)
        LT, // LeftTop (1)
        RT, // RightTop (2)
        RB  // RightBottom (3)
    }

    [Header("Essential Settings")]
    [SerializeField] Image rectImage = null;
    [SerializeField] Image leftBottomCorner = null;
    [SerializeField] Image leftTopCorner = null;
    [SerializeField] Image rightTopCorner = null;
    [SerializeField] Image rightBottomCorner = null;
    [SerializeField][Tooltip("����Ŭ���̵� ��� �ߴ� ��")] Image cycloidPoint = null;
    [SerializeField][Tooltip("ȸ�� �߽� ��")] Image pivotPoint = null;
    [SerializeField][Tooltip("�ٴ� �浹 ��")] Image touchPoint = null;

    [Header("Properties")]
    [SerializeField][Tooltip("�ʴ� ȸ�� ���� (speed)")] float rotationAnglePerSecond = 0f;
    [SerializeField][Tooltip("ȸ�� ���� ( true : right(��) , false : left(��) )")] bool rotationDirection = true;
    bool prevRotationDirection = true;

    [Header("Statuses")]
    [SerializeField][Tooltip("���� ����")] Vector3 StartPoint = Vector3.zero;
    [SerializeField][Tooltip("���� ȸ����")] float curRotation = 0f;
    [SerializeField] int cycloidPos = -1;
    [SerializeField] int pivotPos = -1;
    [SerializeField] int touchPos = -1;

    Image[] corners = new Image[4];

    void Start()
    {
        if (null == leftTopCorner ||
            null == leftBottomCorner ||
            null == rightTopCorner ||
            null == rightBottomCorner)
        {
            throw new System.Exception("������ �Ϸ���� �ʾҽ��ϴ�.");
        }

        corners[0] = leftBottomCorner;  // Corner.LB
        corners[1] = leftTopCorner;     // Corner.LT
        corners[2] = rightTopCorner;    // Corner.RT
        corners[3] = rightBottomCorner; // Corner.RB

        cycloidPos = (int)CornerName.LB;
        pivotPos = (int)CornerName.RB;
        touchPos = (int)CornerName.RT;

        cycloidPoint.rectTransform.localPosition = corners[cycloidPos].rectTransform.localPosition;
        pivotPoint.rectTransform.localPosition = corners[pivotPos].rectTransform.localPosition;
        touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;

        rotationAnglePerSecond = 50;
    }

    void Update()
    {
        // When rotation direction change
        if(prevRotationDirection != rotationDirection)
        {
            // When new rotation direction is RIGHT
            if(rotationDirection == true)
            {
                // Touch Point Position = pivot - 1
                touchPos = ModuloOperatorRight(pivotPos);
            }
            // When new rotation direction is LEFT
            else
            {
                // Touch Point Position = pivot + 1
                touchPos = ModuloOperatorLeft(pivotPos);
            }
            touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;
        }


        // Rotation Direction : right
        if (rotationDirection)
        {
            rectImage.rectTransform.RotateAround(pivotPoint.rectTransform.position, Vector3.back, Mathf.Abs(rotationAnglePerSecond) * Time.deltaTime);

            // When the TouchPoint touch ground
            if (touchPoint.transform.position.y < pivotPoint.transform.position.y)
            {
                //rectImage.rectTransform.localRotation = Quaternion.Euler(0, 0, InterpolationAngle(curRotation));
                //rectImage.rectTransform.rotation = Quaternion.Euler(0, 0, InterpolationAngle(curRotation));
                //Time.timeScale = 0;

                pivotPos = ModuloOperatorRight(pivotPos);
                touchPos = ModuloOperatorRight(touchPos);
                pivotPoint.rectTransform.localPosition = corners[pivotPos].rectTransform.localPosition;
                touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;
            }
        }
        // Rotation Direction : left
        else
        {
            rectImage.rectTransform.RotateAround(pivotPoint.rectTransform.position, Vector3.forward, Mathf.Abs(rotationAnglePerSecond) * Time.deltaTime);

            // When the TouchPoint touch ground
            if (touchPoint.transform.position.y <= pivotPoint.transform.position.y)
            {
                //rectImage.rectTransform.rotation = Quaternion.Euler(0, 0, InterpolationAngle(curRotation));

                pivotPos = ModuloOperatorLeft(pivotPos);
                touchPos = ModuloOperatorLeft(touchPos);
                pivotPoint.rectTransform.localPosition = corners[pivotPos].rectTransform.localPosition;
                touchPoint.rectTransform.localPosition = corners[touchPos].rectTransform.localPosition;
            }
        }

        curRotation = rectImage.rectTransform.rotation.eulerAngles.z;
        prevRotationDirection = rotationDirection;
    }

    float InterpolationAngle(float angle)
    {
        float result = 0f;

        if (angle <= 90) result = 90f;
        else if (angle <= 180) result = 180f;
        else if (angle <= 270) result = 270f;
        else result = 360f;

        return result;
    }

    int ModuloOperatorRight(int curValue)
    {
        int nextValue = (curValue - 1 + 4) % 4;
        return nextValue;
    }

    int ModuloOperatorLeft(int curValue)
    {
        int nextValue = (curValue + 1) % 4;
        return nextValue;
    }

}
