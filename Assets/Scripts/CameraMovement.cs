using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(
            Input.GetAxis("Horizontal"), 
            Input.GetAxis("UpDown"), 
            Input.GetAxis("Vertical")) * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, Input.GetAxis("RotateHorizontal") * rotateSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right, Input.GetAxis("RotateVertical") * rotateSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
    }
}
