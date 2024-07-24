using static Assets.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

public class RobotControl : MonoBehaviour
{
    private enum Direction { Left = -1, Right = 1 };

    private const float middleLinkLength = 0.325f;
    private const float endLinkLength = 0.345f;
    private const float instrumentBaseHeight = 0.254f;

    private const float middleLinkLimit = 132f;
    private const float endLinkLimit = 150f;
    private const float instrumentUpLimit = 0.150f;
    private const float instrumentDownLimit = -0.50f;
    private const float innerLimit = 0.18f;
    private const float backLimit = 0.24f;
    private const float sideLimit = 0.115f;

    private Vector3 rightMiddleLimitPoint;
    private Vector3 leftMiddleLimitPoint;
    private float frontLimit;

    [SerializeField]
    private GameObject baseLink;
    [SerializeField]
    private GameObject middleLink;
    [SerializeField]
    private GameObject endLink;
    [SerializeField]
    private GameObject instrument;

    private Vector3 _targetPosition;
    private float targetAngle;
    private float targetMiddleAngle;
    private float targetEndAngle;
    private Direction targetDirection = Direction.Right;

    public float rotatingSpeed;
    public float verticalSpeed;

    void Start()
    {
        rightMiddleLimitPoint = Quaternion.AngleAxis(middleLinkLimit, transform.up) *
            -transform.right * middleLinkLength + middleLink.transform.position;
        leftMiddleLimitPoint = Quaternion.AngleAxis(middleLinkLimit, -transform.up) *
            -transform.right * middleLinkLength + middleLink.transform.position;
        frontLimit = (Quaternion.AngleAxis(middleLinkLimit, transform.up) *
            -transform.right * innerLimit + middleLink.transform.position).z;

        #region WorkingAreasDebug
        //var backLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //backLeft.transform.position = leftInnerLimitPoint;
        //backLeft.transform.localScale = Vector3.one * 0.001f;
        //backLeft.GetComponent<Renderer>().material.color = Color.cyan;

        //var backRight= GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //backRight.transform.position = rightInnerLimitPoint;
        //backRight.transform.localScale = Vector3.one * 0.001f;
        //backRight.GetComponent<Renderer>().material.color = Color.cyan;

        //var outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //outer.transform.position = middleLink.transform.position;
        //outer.transform.localScale = new Vector3(1, 0.00001f, 1) * (middleLinkLength + endLinkLength) * 2;
        //outer.GetComponent<Renderer>().material.color = Color.red;

        //var inner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //inner.transform.position = middleLink.transform.position + Vector3.up * 0.0002f;
        //inner.transform.localScale = new Vector3(1, 0.00001f, 1) * innerLimit * 2;
        //inner.GetComponent<Renderer>().material.color = Color.yellow;

        //var right = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //var left = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //right.transform.position = rightMiddleLimitPoint + Vector3.up * (middleLink.transform.position.y + 0.0001f);
        //left.transform.position = leftMiddleLimitPoint + Vector3.up * (middleLink.transform.position.y + 0.0001f); ;
        //right.transform.localScale = left.transform.localScale = new Vector3(1, 0.00001f, 1) * endLinkLength * 2;
        //right.GetComponent<Renderer>().material.color = left.GetComponent<Renderer>().material.color = Color.green;
        #endregion

        TargetPosition = GameObject.Find("Target").transform.position - Vector3.up * instrumentBaseHeight;
    }

    void Update()
    {
        MoveLink(middleLink, targetMiddleAngle);
        MoveLink(endLink, targetEndAngle);
        Vector3 pos = instrument.transform.position;
        if (Mathf.Abs(pos.y - TargetPosition.y) >  1e-6f)
        {
            pos.y += verticalSpeed * (TargetPosition.y > pos.y ? 1 : -1) * Time.deltaTime;
            instrument.transform.position = pos;
        }
    }

    public Vector3 TargetPosition
    {
        get => _targetPosition;
        set
        {
            _targetPosition = value;
            float y = _targetPosition.y;
            _targetPosition.y = middleLink.transform.position.y;

            targetAngle = Vector3.SignedAngle(-transform.right, _targetPosition - middleLink.transform.position, transform.up);
            float distance = Vector3.Distance(_targetPosition, middleLink.transform.position);
            float middleLinkAngle = CalculateAngle(middleLinkLength, distance, endLinkLength);
            float endLinkAngle = CalculateAngle(middleLinkLength, endLinkLength, distance);

            /*print(string.Join("\n",
                string.Join(" ", "ѕозиции:", _targetPosition, middleLink.transform.position),
                string.Join(" ", "–ассто€ни€:", distance, middleLinkLength, endLinkLength),
                string.Join(" ", "”глы:", targetAngle, middleLinkAngle, endLinkAngle)));*/

            (float, float) targetAngles = GetLinkAnglesByDirection(middleLinkAngle, endLinkAngle, targetDirection);
            targetMiddleAngle = targetAngles.Item1;
            targetEndAngle = targetAngles.Item2;

            if (!CheckAngles())
            {
                targetDirection = (Direction)(-(int)targetDirection);
                targetAngles = GetLinkAnglesByDirection(middleLinkAngle, endLinkAngle, targetDirection);
                targetMiddleAngle = targetAngles.Item1;
                targetEndAngle = targetAngles.Item2;
            }

            _targetPosition.y = instrumentBaseHeight + y;
            GameObject.Find("Target").transform.position = _targetPosition;
        }
    }

    private void MoveLink(GameObject link, float angle)
    {
        Vector3 rot = link.transform.localRotation.eulerAngles;
        rot.z = rot.z > 180 ? rot.z - 360 : rot.z;
        if (Mathf.Abs(rot.z - angle) > 0.1f)
        {
            Direction rotDirection = angle > rot.z ? Direction.Right : Direction.Left;
            rot.z += rotatingSpeed * (int)rotDirection * Time.deltaTime;
            link.transform.localRotation = Quaternion.Euler(rot);
        }
    }

    /// <summary>
    /// ¬озвращает углы дл€ первого и второго звена в мировом пространстве, в зависимости от угла между ними
    /// </summary>
    /// <param name="middleAngle">угол между осью z и первым звеном</param>
    /// <param name="endAngle">угол между первым и вторым звеном</param>
    /// <param name="direction"></param>
    /// <returns>”гол первого звена, затем угол второго звена</returns>
    private (float, float) GetLinkAnglesByDirection(float middleAngle, float endAngle, Direction direction)
    {
        return (targetAngle + middleAngle * ((int)direction), (int)direction * (endAngle - 180));
    }

    public bool CheckLimits(Vector3 point)
    {
        float y = point.y;
        point.y = middleLink.transform.position.y;
        targetAngle = Vector3.SignedAngle(-transform.right, _targetPosition + middleLink.transform.position, transform.up);
        point.y = y;

        if (point.y < instrumentDownLimit || point.y > instrumentUpLimit)
            return false;
        if (!IsPointInCircle(point, middleLink.transform.position, middleLinkLength + endLinkLength))
            return false;
        if (IsPointInCircle(point, middleLink.transform.position, innerLimit))
            return false;
        if (targetAngle > middleLinkLimit && !IsPointInCircle(point, rightMiddleLimitPoint, endLinkLength))
            return false;
        if (targetAngle < -middleLinkLimit && !IsPointInCircle(point, leftMiddleLimitPoint, endLinkLength))
            return false;
        if (IsPointInRectangle(point,
            middleLink.transform.position.x - sideLimit,
            middleLink.transform.position.x + sideLimit,
            middleLink.transform.position.z - backLimit,
            frontLimit))
            return false;

        return true;
    }

    private bool CheckAngles()
    {
        return Mathf.Abs(targetMiddleAngle) < middleLinkLimit && Mathf.Abs(targetEndAngle) < endLinkLimit;
    }


}
